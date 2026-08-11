using System.Threading.Channels;
using Application.Interfaces;

namespace Infrastructure.Services;

public class FulfillmentService : IFulfillmentService
{
    private readonly Channel<Guid> _channel;

    public FulfillmentService(Channel<Guid> channel)
    {
        _channel = channel;
    }

    public async Task EnqueueSubmissionAsync(Guid orderId)
    {
        await _channel.Writer.WriteAsync(orderId);
    }

    public Task<object?> SubmitOrderAsync(Guid orderId)
    {
        return Task.FromResult<object?>(null);
    }

    public Task<object?> GetOrderStatusAsync(Guid orderId)
    {
        return Task.FromResult<object?>(null);
    }
}
