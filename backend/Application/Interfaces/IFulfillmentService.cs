namespace Application.Interfaces;

public interface IFulfillmentService
{
    Task EnqueueSubmissionAsync(Guid orderId);
    Task<object?> SubmitOrderAsync(Guid orderId);
    Task<object?> GetOrderStatusAsync(Guid orderId);
}
