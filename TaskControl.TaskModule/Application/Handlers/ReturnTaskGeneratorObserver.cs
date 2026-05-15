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
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

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

            // 3. Ищем резервы, чтобы отсеять отказников (товары, которые еще в пути к клиенту)
            var itemPosIds = itemsInTrunk.Select(i => i.Id).ToList();
            var reservations = await db.GetTable<OrderReservationModel>()
                .Where(r => r.ItemPositionId.HasValue && itemPosIds.Contains(r.ItemPositionId.Value))
                .ToListAsync();

            var reservedIds = reservations.Select(r => r.ItemPositionId!.Value).ToHashSet();

            // Получаем список сирот (товаров без резервов)
            var orphanedItems = itemsInTrunk.Where(ip => !reservedIds.Contains(ip.Id)).ToList();

            if (!orphanedItems.Any()) return;

            // ЗАЩИТА ОТ ДВОЙНЫХ КЛИКОВ И ДУБЛЕЙ ЗАДАЧ
            var orphanedIds = orphanedItems.Select(o => o.Id).ToList();

            // Ищем, нет ли уже активной задачи на эти товары через return_lines -> return_assignments -> base_tasks
            var itemsAlreadyInTasks = await db.GetTable<ReturnLineModel>()
                // 1. Присоединяем назначения возврата
                .Join(db.GetTable<ReturnAssignmentModel>(),
                      line => line.ReturnAssignmentId,
                      assignment => assignment.Id,
                      (line, assignment) => new { line.ItemPositionId, assignment.TaskId })
                // 2. Присоединяем базовую задачу, чтобы проверить строковый статус
                .Join(db.GetTable<BaseTaskModel>(),
                      temp => temp.TaskId,
                      task => task.TaskId,
                      (temp, task) => new { temp.ItemPositionId, task.Status })
                // 3. Фильтруем по нашим отказникам и статусам незавершенных задач
                .Where(x => orphanedIds.Contains(x.ItemPositionId)
                         && (x.Status == "New" || x.Status == "Assigned" || x.Status == "InProgress"))
                .Select(x => x.ItemPositionId)
                .ToListAsync();

            var alreadyInTaskSet = itemsAlreadyInTasks.ToHashSet();
            // -------------------------------------------------------------

            // Оставляем только те товары, для которых еще нет активных задач
            var itemsToReturn = orphanedItems
                .Where(o => !alreadyInTaskSet.Contains(o.Id))
                .Select(o => (o.Id, o.Quantity))
                .ToList();

            if (!itemsToReturn.Any())
            {
                _logger.LogInformation("Для всех отказников курьера {CourierId} уже сгенерированы задачи на возврат. Пропуск.", employeeId);
                return;
            }
            // -------------------------------------------------------------

            _logger.LogInformation("Обнаружены новые возвраты в багажнике курьера {CourierId}. Передача задачи в генератор.", employeeId);

            await _returnTaskGenerator.GenerateReturnTaskForCourierAsync(employeeId, branchId, itemsToReturn);
        }
    }
}