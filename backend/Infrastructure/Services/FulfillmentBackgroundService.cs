using System.Threading.Channels;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class FulfillmentBackgroundService : BackgroundService
{
    private readonly Channel<Guid> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FulfillmentBackgroundService> _logger;

    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromSeconds(60)
    ];

    public FulfillmentBackgroundService(
        Channel<Guid> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<FulfillmentBackgroundService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Fulfillment background service starting");

        await RecoverPendingOrdersAsync(stoppingToken);

        _logger.LogInformation("Fulfillment background service processing queue");

        await foreach (var orderId in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            await ProcessOrderAsync(orderId, stoppingToken);
        }
    }

    private async Task RecoverPendingOrdersAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var pendingOrders = await context.FulfillmentOrders
                .Where(fo => fo.IsActive && fo.Status == FulfillmentStatus.Pending)
                .Select(fo => fo.OrderId)
                .ToListAsync(cancellationToken);

            if (pendingOrders.Count == 0)
            {
                _logger.LogInformation("Startup sweep: no pending fulfillment orders to recover");
                return;
            }

            _logger.LogInformation("Startup sweep: recovering {Count} pending fulfillment orders", pendingOrders.Count);

            foreach (var orderId in pendingOrders)
            {
                await _channel.Writer.WriteAsync(orderId, cancellationToken);
                _logger.LogInformation("Startup sweep: enqueued OrderId {OrderId}", orderId);
            }

            _logger.LogInformation("Startup sweep: completed recovery of {Count} pending fulfillment orders", pendingOrders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Startup sweep failed to recover pending fulfillment orders");
        }
    }

    private async Task ProcessOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var qikinkClient = scope.ServiceProvider.GetRequiredService<IQikinkClient>();

        for (var attempt = 0; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                var fulfillmentOrder = await context.FulfillmentOrders
                    .Include(fo => fo.Order)
                    .Include(fo => fo.Items)
                    .Include(fo => fo.Provider)
                    .FirstOrDefaultAsync(fo => fo.OrderId == orderId, cancellationToken);

                if (fulfillmentOrder == null)
                {
                    _logger.LogWarning("Fulfillment order not found for OrderId: {OrderId}", orderId);
                    return;
                }

                if (fulfillmentOrder.Status == FulfillmentStatus.Shipped ||
                    fulfillmentOrder.Status == FulfillmentStatus.Delivered)
                {
                    _logger.LogInformation("Fulfillment order {OrderId} already completed", orderId);
                    return;
                }

                if (fulfillmentOrder.Status == FulfillmentStatus.Cancelled)
                {
                    _logger.LogInformation("Fulfillment order {OrderId} was cancelled", orderId);
                    return;
                }

                if (fulfillmentOrder.Status == FulfillmentStatus.Processing)
                {
                    _logger.LogInformation("Fulfillment order {OrderId} already processing with Qikink", orderId);
                    return;
                }

                fulfillmentOrder.Status = FulfillmentStatus.Submitted;
                fulfillmentOrder.SubmittedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);

                var payload = new
                {
                    OrderId = orderId.ToString(),
                    ExternalReference = fulfillmentOrder.Order.OrderNumber,
                    Items = fulfillmentOrder.Items.Select(i => new
                    {
                        ExternalProductId = i.ExternalProductId,
                        ExternalVariantId = i.ExternalVariantId,
                        ExternalSku = i.ExternalSku,
                        Quantity = i.Quantity
                    })
                };

                var result = await qikinkClient.SubmitOrderAsync(payload);

                fulfillmentOrder.Status = FulfillmentStatus.Processing;
                fulfillmentOrder.ExternalOrderId = result?.ToString();
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Fulfillment submitted successfully for OrderId: {OrderId}, Attempt: {Attempt}",
                    orderId, attempt + 1);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Fulfillment submission failed for OrderId: {OrderId}, Attempt: {Attempt}/{MaxAttempts}",
                    orderId, attempt + 1, MaxRetryAttempts + 1);

                if (attempt < MaxRetryAttempts)
                {
                    var delay = RetryDelays[attempt];
                    _logger.LogInformation(
                        "Retrying fulfillment for OrderId: {OrderId} in {Delay}s",
                        orderId, delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    await MarkFulfillmentFailedAsync(orderId, ex.Message, cancellationToken);
                }
            }
        }
    }

    private async Task MarkFulfillmentFailedAsync(Guid orderId, string errorMessage, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var fulfillmentOrder = await context.FulfillmentOrders
                .FirstOrDefaultAsync(fo => fo.OrderId == orderId, cancellationToken);

            if (fulfillmentOrder != null)
            {
                fulfillmentOrder.Status = FulfillmentStatus.Failed;
                fulfillmentOrder.FailureReason = errorMessage.Length > 2000
                    ? errorMessage[..2000]
                    : errorMessage;
                fulfillmentOrder.ErrorCategory = "SubmissionFailed";
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Fulfillment order {FulfillmentOrderId} marked as failed after max retries",
                    fulfillmentOrder.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to mark fulfillment as failed for OrderId: {OrderId}", orderId);
        }
    }
}
