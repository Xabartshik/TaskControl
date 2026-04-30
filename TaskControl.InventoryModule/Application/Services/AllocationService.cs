using Microsoft.Extensions.Logging;
using System.Data.Common;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DAL.Repositories;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.Domain;
using TaskControl.OrderModule.Application.Interface;

namespace TaskControl.InventoryModule.Application.Services
{
    public interface IAllocationService
    {
        Task SoftAllocateOrderAsync(int orderId, int branchId);
        Task<bool> HardAllocateOrderItemsAsync(int branchId, int orderPositionId, int itemId, int neededQuantity);
    }

    public class AllocationService : IItemAllocationService
    {
        private readonly IOrderPositionRepository _orderPosRepo;
        private readonly IItemPositionRepository _itemPosRepo;
        private readonly IOrderReservationRepository _reservationRepo;
        private readonly ILogger<AllocationService> _logger;

        public AllocationService(
            IOrderPositionRepository orderPosRepo,
            IItemPositionRepository itemPosRepo,
            IOrderReservationRepository reservationRepo,
            ILogger<AllocationService> logger)
        {
            _orderPosRepo = orderPosRepo;
            _itemPosRepo = itemPosRepo;
            _reservationRepo = reservationRepo;
            _logger = logger;
        }

        public async Task<bool> HardAllocateOrderItemsAsync(int branchId, int orderPositionId, int itemId, int neededQuantity)
        {
            _logger.LogInformation("Запуск Hard Allocation: Филиал {BranchId}, Товар {ItemId}, Кол-во {Qty}",
                branchId, itemId, neededQuantity);

            // 1. Получаем из БД только подходящие ItemPositions
            var availableStocks = await _itemPosRepo.GetByItemAndBranchAsync(branchId, itemId);

            var stocks = new List<(int ItemPositionId, int AvailableQty)>();

            foreach (var ip in availableStocks)
            {
                // 2. Получаем сумму резервов для конкретной физической позиции
                // Предполагается, что в IOrderReservationRepository добавлен этот метод
                var reservedQty = await _reservationRepo.GetReservedQuantityByItemPositionAsync(ip.Id);

                int availableQty = ip.Quantity - reservedQty;

                if (availableQty > 0)
                {
                    stocks.Add((ItemPositionId: ip.Id, AvailableQty: availableQty));
                }
            }

            int remainingToAllocate = neededQuantity;

            // 3. Распределение по полкам
            foreach (var stock in stocks)
            {
                if (remainingToAllocate <= 0) break;

                int takeQty = Math.Min(remainingToAllocate, stock.AvailableQty);

                await _reservationRepo.AddAsync(new OrderReservation
                {
                    OrderPositionId = orderPositionId,
                    ItemPositionId = stock.ItemPositionId,
                    Quantity = takeQty
                });

                remainingToAllocate -= takeQty;

                _logger.LogDebug("Товар зарезервирован: Позиция {ItemPosId}, Кол-во {Qty}",
                    stock.ItemPositionId, takeQty);
            }

            if (remainingToAllocate > 0)
            {
                _logger.LogWarning("Нехватка товара {ItemId} в филиале {BranchId}. Не распределено: {Remaining}",
                    itemId, branchId, remainingToAllocate);
            }

            return remainingToAllocate == 0;
        }
    }
}