using LinqToDB;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.Domain;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.DataAccess.Interface;

namespace TaskControl.TaskModule.Application.Handlers
{
    public class OrderCancelledHandler : IOrderCancelledEventHandler
    {
        private readonly ITaskDataConnection _db;
        private readonly IOrderCancellationService _cancellationService;
        private readonly ReturnTaskGeneratorService _returnTaskGenerator;

        public OrderCancelledHandler(
            ITaskDataConnection db,
            IOrderCancellationService cancellationService,
            ReturnTaskGeneratorService returnTaskGenerator)
        {
            _db = db;
            _cancellationService = cancellationService;
            _returnTaskGenerator = returnTaskGenerator;
        }

        public async Task HandleOrderCancelledAsync(int orderId, int branchId, OrderStatus previousStatus)
        {
            // 1. Находим все резервы (товары), связанные с этим заказом
            var orderPositions = await _db.GetTable<OrderPositionModel>()
                .Where(op => op.OrderId == orderId)
                .ToListAsync();

            var positionIds = orderPositions.Select(op => op.UniqueId).ToList();

            var reservations = await _db.GetTable<OrderReservationModel>()
                .Where(r => positionIds.Contains(r.OrderPositionId) && r.ItemPositionId != null)
                .ToListAsync();

            // Формируем словарь <ItemPositionId, Qty> для сервиса отмены
            var cancelledPositions = reservations
                .GroupBy(r => r.ItemPositionId.Value)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

            if (!cancelledPositions.Any()) return;

            // 2. Вызываем универсальный метод отмены. 
            // Он удалит резервы (OrderReservationModel) и вернет список "сирот"
            var itemsToReturn = await _cancellationService.ProcessCancellationAsync(
                orderId,
                cancelledPositions,
                isFullCancellation: true
            );

            // 3. ГЛАВНАЯ ПРОВЕРКА: Нужна ли физическая задача?
            if (previousStatus != OrderStatus.Created && itemsToReturn.Any())
            {
                // Заказ уже собирали (Assembly) или он ждал выдачи (Ready). 
                // Товары физически лежат в корзине сборщика или зоне выдачи.
                // Запускаем режим отмены: генерируем задачу ReturnToStock.
                await _returnTaskGenerator.GenerateReturnTaskFromCancelledItemsAsync(orderId, branchId, itemsToReturn);
            }
            else
            {
                // Если previousStatus == Created, товары физически не покидали свои полки.
                // Резервы в БД уже удалены на шаге 2 внутри ProcessCancellationAsync.
                // Физическая задача на возврат не требуется!
            }
        }
    }
}