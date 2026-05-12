using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Interface;

namespace TaskControl.TaskModule.Application.Observers
{
    public class ReturnTaskGeneratorObserver : IEmployeeCheckInObserver
    {
        private readonly ITaskDataConnection _taskDb;
        private readonly ILogger<ReturnTaskGeneratorObserver> _logger;
        private readonly Services.ReturnTaskGeneratorService _returnTaskGenerator;

        public ReturnTaskGeneratorObserver(
            ITaskDataConnection taskDb,
            ILogger<ReturnTaskGeneratorObserver> logger,
            Services.ReturnTaskGeneratorService returnTaskGenerator)
        {
            _taskDb = taskDb;
            _logger = logger;
            _returnTaskGenerator = returnTaskGenerator;
        }

        public async Task OnEmployeeCheckedAsync(int employeeId, int branchId, string checkType)
        {
            // Нас интересует только конец смены или прибытие на пандус
            if (checkType != "out" && checkType != "dock") return;

            var db = (DataConnection)_taskDb;

            // 1. Ищем виртуальную ячейку курьера
            var courierPos = await db.GetTable<PositionModel>()
                .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == employeeId.ToString());

            if (courierPos == null) return;

            // 2. Достаем все товары из багажника
            var itemsInTrunk = await db.GetTable<ItemPositionModel>()
                .Where(ip => ip.PositionId == courierPos.PositionId)
                .ToListAsync();

            if (!itemsInTrunk.Any()) return;

            // 3. Ищем резервы, чтобы отсеять отказников
            var itemPosIds = itemsInTrunk.Select(i => i.Id).ToList();
            var reservations = await db.GetTable<OrderReservationModel>()
                .Where(r => r.ItemPositionId.HasValue && itemPosIds.Contains(r.ItemPositionId.Value))
                .ToListAsync();

            var reservedIds = reservations.Select(r => r.ItemPositionId!.Value).ToHashSet();

            // Получаем список сирот (товаров без резервов)
            var orphanedItems = itemsInTrunk.Where(ip => !reservedIds.Contains(ip.Id)).ToList();

            if (!orphanedItems.Any()) return;

            var itemsToReturn = orphanedItems.Select(o => (o.Id, o.Quantity)).ToList();

            _logger.LogInformation("Обнаружены возвраты в багажнике курьера {CourierId}. Передача задачи в генератор.", employeeId);

            await _returnTaskGenerator.GenerateReturnTaskForCourierAsync(employeeId, branchId, itemsToReturn);
        }
    }
}