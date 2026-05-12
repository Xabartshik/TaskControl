using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;

namespace TaskControl.TaskModule.Application.Services
{
    public interface IOrderCancellationService
    {
        /// <summary>
        /// Универсальный метод отмены товаров в заказе. 
        /// Снимает резервы, обновляет чек (или статус заказа) и возвращает список товаров для возврата на полку.
        /// </summary>
        /// <param name="orderId">ID заказа</param>
        /// <param name="cancelledItemPositions">Словарь: Key = ItemPositionId (физический товар), Value = Количество к отмене</param>
        /// <param name="isFullCancellation">Флаг полной отмены (чтобы сохранить историю позиций)</param>
        /// <returns>Список кортежей (ItemPositionId, Qty) для генератора задач возврата</returns>
        Task<List<(int ItemPositionId, int Qty)>> ProcessCancellationAsync(int orderId, Dictionary<int, int> cancelledItemPositions, bool isFullCancellation);
    }

    public class OrderCancellationService : IOrderCancellationService
    {
        private readonly ITaskDataConnection _db;
        private readonly ILogger<OrderCancellationService> _logger;

        public OrderCancellationService(ITaskDataConnection db, ILogger<OrderCancellationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<(int ItemPositionId, int Qty)>> ProcessCancellationAsync(int orderId, Dictionary<int, int> cancelledItemPositions, bool isFullCancellation)
        {
            var itemsToReturn = new List<(int ItemPositionId, int Qty)>();
            if (cancelledItemPositions == null || !cancelledItemPositions.Any())
                return itemsToReturn;

            // Оборачиваем всё в транзакцию, чтобы финансы и резервы всегда были синхронны
            using var transaction = await ((DataConnection)_db).BeginTransactionAsync();
            try
            {
                foreach (var kvp in cancelledItemPositions)
                {
                    int itemPositionId = kvp.Key;
                    int cancelQty = kvp.Value;

                    if (cancelQty <= 0) continue;

                    // 1. Ищем товар, чтобы узнать его ItemId
                    var itemPos = await _db.GetTable<ItemPositionModel>()
                        .FirstOrDefaultAsync(ip => ip.Id == itemPositionId);

                    if (itemPos == null) continue;

                    // 2. Ищем позицию в чеке
                    var orderPosition = await _db.GetTable<OrderPositionModel>()
                        .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ItemId == itemPos.ItemId);

                    if (orderPosition != null)
                    {
                        // === 3. СНИМАЕМ РЕЗЕРВЫ (Исправлено условие удаления) ===
                        var reservation = await _db.GetTable<OrderReservationModel>()
                            .FirstOrDefaultAsync(r => r.OrderPositionId == orderPosition.UniqueId);

                        if (reservation != null)
                        {
                            // ИСПРАВЛЕНИЕ: Проверяем количество именно в резерве, а не в позиции заказа
                            if (reservation.Quantity <= cancelQty)
                            {
                                await _db.GetTable<OrderReservationModel>().Where(r => r.Id == reservation.Id).DeleteAsync();
                            }
                            else
                            {
                                await _db.GetTable<OrderReservationModel>().Where(r => r.Id == reservation.Id)
                                    .Set(r => r.Quantity, r => r.Quantity - cancelQty).UpdateAsync();
                            }
                        }

                        // === 4. ОБНОВЛЯЕМ ЧЕК (Только при частичной отмене) ===
                        if (!isFullCancellation)
                        {
                            if (orderPosition.Quantity <= cancelQty)
                            {
                                await _db.GetTable<OrderPositionModel>().Where(op => op.UniqueId == orderPosition.UniqueId).DeleteAsync();
                                await _db.GetTable<OrderModel>().Where(o => o.OrderId == orderId)
                                    .Set(o => o.TotalPrice, o => o.TotalPrice - orderPosition.Price).UpdateAsync();
                            }
                            else
                            {
                                decimal unitPrice = orderPosition.Price;
                                decimal priceToDeduct = unitPrice * cancelQty;

                                await _db.GetTable<OrderPositionModel>().Where(op => op.UniqueId == orderPosition.UniqueId)
                                    .Set(op => op.Quantity, op => op.Quantity - cancelQty).UpdateAsync();

                                await _db.GetTable<OrderModel>().Where(o => o.OrderId == orderId)
                                    .Set(o => o.TotalPrice, o => o.TotalPrice - priceToDeduct).UpdateAsync();
                            }
                        }
                    }

                    // Добавляем товар в список "сирот" для возврата на склад
                    itemsToReturn.Add((itemPositionId, cancelQty));
                }

                // === 5. ОБНОВЛЯЕМ СТАТУС ЗАКАЗА ПРИ ПОЛНОЙ ОТМЕНЕ ===
                if (isFullCancellation)
                {
                    await _db.GetTable<OrderModel>()
                        .Where(o => o.OrderId == orderId)
                        .Set(o => o.Status, "Cancelled")
                        .Set(o => o.TotalPrice, 0) // Добавляем обнуление суммы
                        .UpdateAsync();
                }

                await transaction.CommitAsync();
                return itemsToReturn;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка транзакции при отмене заказа {OrderId}", orderId);
                throw;
            }
        }

    }
}