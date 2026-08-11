namespace Application.Interfaces;

public interface IQikinkClient
{
    Task<object?> SubmitOrderAsync(object orderPayload);
    Task<object?> GetOrderStatusAsync(string externalOrderId);
}
