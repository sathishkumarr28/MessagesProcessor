namespace MessagesProcessor.Services
{
    public interface IOrderHandler
    {
        Task HandleOrders<T>(T orderData);
    }
}
