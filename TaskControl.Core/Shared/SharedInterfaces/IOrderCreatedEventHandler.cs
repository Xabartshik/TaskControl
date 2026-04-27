using System.Threading.Tasks;

namespace TaskControl.Core.Shared.SharedInterfaces
{
    /// <summary>
    /// Интерфейс для подписчиков, которым нужно реагировать на создание нового заказа
    /// </summary>
    public interface IOrderCreatedEventHandler
    {
        Task HandleOrderCreatedAsync(int orderId, int branchId);
    }
}