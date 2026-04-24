using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DAL.Repositories;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.Application.Services
{
    public interface IAllocationService
    {
        Task SoftAllocateOrderAsync(int orderId, int branchId);
    }

    public class AllocationService : IAllocationService, IOrderCreatedEventHandler
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

        public async Task HandleOrderCreatedAsync(int orderId, int branchId)
        {
            _logger.LogInformation("Обработка события создания заказа {OrderId} в филиале {BranchId}", orderId, branchId);
            await SoftAllocateOrderAsync(orderId, branchId);
        }

        public async Task SoftAllocateOrderAsync(int orderId, int branchId)
        {
            var positions = await _orderPosRepo.GetByOrderIdAsync(orderId);

            foreach (var pos in positions)
            {
                // 1. Считаем сколько товара всего в филиале
                var branchStock = await _itemPosRepo.GetAllAsync();
                var totalInBranch = branchStock
                    .Where(s => s.ItemId == pos.ItemId)
                    // Здесь в идеале нужен фильтр по BranchId в репозитории ItemPosition
                    .Sum(s => s.Quantity);

                // 2. Считаем сколько уже зарезервировано
                var alreadyReserved = await _reservationRepo.GetReservedQuantityInBranchAsync(pos.ItemId, branchId);

                var available = totalInBranch - (int)alreadyReserved;

                if (available < pos.Quantity)
                {
                    throw new InvalidOperationException($"Недостаточно товара {pos.ItemId} для Soft Allocation. Доступно: {available}, нужно: {pos.Quantity}");
                }

                // 3. Создаем soft-резерв (ItemPositionId = null)
                await _reservationRepo.AddAsync(new OrderReservation
                {
                    OrderPositionId = pos.UniqueId,
                    ItemPositionId = null,
                    Quantity = pos.Quantity
                });

                _logger.LogInformation("Создан Soft-резерв для OrderPosition {Id}, количество {Qty}", pos.UniqueId, pos.Quantity);
            }
        }
    }
}