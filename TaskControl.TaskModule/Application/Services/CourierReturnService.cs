using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.Application.Services
{
    public interface ICourierReturnService
    {
        Task GenerateReturnsIfAnyAsync(int courierId, int branchId);
    }

    public class CourierReturnService : ICourierReturnService
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<CourierReturnService> _logger;
        private readonly IOrderCancellationService _cancellationService;
        private readonly ReturnTaskGeneratorService _returnTaskGenerator;

        public CourierReturnService(ITaskDataConnection db, ILogger<CourierReturnService> logger, IOrderCancellationService cancellationService, ReturnTaskGeneratorService returnTaskGenerator)
        {
            _db = db;
            _logger = logger;
            _cancellationService = cancellationService;
            _returnTaskGenerator = returnTaskGenerator;
        }

        public async Task GenerateReturnsIfAnyAsync(int courierId, int branchId)
        {
            var db = (DataConnection)_db;

            // 1. Ищем багажник курьера (ZoneCode = "COURIER", FLSNumber = ID курьера)
            var courierCell = await db.GetTable<PositionModel>()
                .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == courierId.ToString());

            if (courierCell == null)
            {
                _logger.LogWarning("Багажник для курьера {CourierId} не найден в системе.", courierId);
                return;
            }

            // 2. Находим все товары, физически находящиеся в багажнике
            var itemsInTrunk = await db.GetTable<ItemPositionModel>()
                .Where(ip => ip.PositionId == courierCell.PositionId)
                .ToListAsync();

            if (!itemsInTrunk.Any()) return;

            var itemsToReturnFinal = new List<(int ItemPositionId, int Qty)>();

            // Временные структуры для группировки
            var orderGroups = new Dictionary<int, Dictionary<int, int>>(); // OrderId -> { ItemPositionId: Qty }
            var orphans = new List<(int ItemPositionId, int Qty)>();

            // 3. Анализируем содержимое: что привязано к заказам, а что - "сироты"
            foreach (var item in itemsInTrunk)
            {
                var reservation = await db.GetTable<OrderReservationModel>()
                    .FirstOrDefaultAsync(r => r.ItemPositionId == item.Id);

                if (reservation != null)
                {
                    // Находим OrderId через OrderPosition
                    var orderId = await db.GetTable<OrderPositionModel>()
                        .Where(op => op.UniqueId == reservation.OrderPositionId)
                        .Select(op => op.OrderId)
                        .FirstOrDefaultAsync();

                    if (orderId != 0)
                    {
                        if (!orderGroups.ContainsKey(orderId))
                            orderGroups[orderId] = new Dictionary<int, int>();

                        orderGroups[orderId][item.Id] = item.Quantity;
                    }
                    else
                    {
                        orphans.Add((item.Id, item.Quantity));
                    }
                }
                else
                {
                    orphans.Add((item.Id, item.Quantity));
                }
            }

            using var transaction = await db.BeginTransactionAsync();
            try
            {
                // 4. Обрабатываем заказы через единый сервис отмены
                foreach (var group in orderGroups)
                {
                    int orderId = group.Key;
                    var positionsToCancel = group.Value;

                    // Вызываем сервис: он снимет резервы, поправит финансы и вернет список для ReturnTask
                    var cancelledItems = await _cancellationService.ProcessCancellationAsync(
                        orderId,
                        positionsToCancel,
                        isFullCancellation: true // Курьер возвращает товары => для него этот заказ отменен
                    );

                    itemsToReturnFinal.AddRange(cancelledItems);
                }

                // 5. Добавляем "сирот" (товары без резервов) в общий список возврата
                itemsToReturnFinal.AddRange(orphans);

                // 6. Генерируем одну общую задачу разгрузки возвратов на склад
                if (itemsToReturnFinal.Any())
                {
                    // Передаем 0 как OrderId, так как это сводная задача по нескольким заказам (или без них)
                    await _returnTaskGenerator.GenerateReturnTaskFromCancelledItemsAsync(0, branchId, itemsToReturnFinal);

                    _logger.LogInformation("Сформирована задача возврата для курьера {CourierId}: {Count} позиций.", courierId, itemsToReturnFinal.Count);
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при автоматической генерации возвратов для курьера {CourierId}", courierId);
            }
        }
    }
}