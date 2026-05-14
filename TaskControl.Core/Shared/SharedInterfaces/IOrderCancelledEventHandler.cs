namespace TaskControl.Core.Shared.SharedInterfaces
{
    public interface IOrderCancelledEventHandler
    {
        Task HandleOrderCancelledAsync(int orderId, int branchId, OrderStatus previousStatus);
    }
}