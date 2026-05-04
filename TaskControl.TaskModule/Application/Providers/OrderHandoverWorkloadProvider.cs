using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model; // Для EmployeeModel
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.TaskModule.Application.DTOs; // Для HandoverTaskDetailsDto
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Application.Providers
{
    public class OrderHandoverWorkloadProvider : ITaskWorkloadProvider
    {
        private readonly IBaseTaskService _baseTaskService;
        private readonly ITaskDataConnection _db;

        public string TaskType => "OrderHandover";

        public OrderHandoverWorkloadProvider(
            IBaseTaskService baseTaskService,
            ITaskDataConnection db)
        {
            _baseTaskService = baseTaskService ?? throw new ArgumentNullException(nameof(baseTaskService));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetUnassignedPoolTasksAsync(int branchId)
        {
            // Связываем назначения с базовой таблицей задач, чтобы отфильтровать по BranchId
            var unassignedHandoverTaskIds = await (
                from a in _db.GetTable<OrderHandoverAssignmentModel>()
                join t in _db.GetTable<BaseTaskModel>() on a.TaskId equals t.TaskId
                where a.AssignedToUserId == null
                      && a.Status == 0 // 0 = New
                      && t.BranchId == branchId // Фильтр по филиалу теперь проверяется у главной задачи
                select a.TaskId
            ).Distinct().ToListAsync();

            if (!unassignedHandoverTaskIds.Any())
                return new List<MobileBaseTaskDto>();

            var availableTasks = await _db.GetTable<BaseTaskModel>()
                .Where(t => unassignedHandoverTaskIds.Contains(t.TaskId) && t.Status == "New")
                .ToListAsync();

            var dtos = new List<MobileBaseTaskDto>();
            foreach (var task in availableTasks)
            {
                var orderCount = await _db.GetTable<OrderHandoverAssignmentModel>()
                    .Where(a => a.TaskId == task.TaskId)
                    .CountAsync();

                dtos.Add(new MobileBaseTaskDto
                {
                    TaskId = task.TaskId,
                    Title = task.Title,
                    TaskType = this.TaskType,
                    Status = Enum.Parse<TaskStatus>(task.Status),
                    PriorityLevel = task.PriorityLevel,
                    CreatedAt = task.CreatedAt,
                    Deadline = task.Deadline,
                    Description = $"Отгрузка: {orderCount} заказ(ов)"
                });
            }

            return dtos;
        }
        public async Task<int> GetActiveWorkloadCountAsync(int workerId)
        {
            // 0 = Assigned, 1 = InProgress
            return await _db.GetTable<OrderHandoverAssignmentModel>()
                .CountAsync(a => a.AssignedToUserId == workerId && (a.Status == 0 || a.Status == 1));
        }

        public async Task<double> GetActiveWorkloadComplexityAsync(int workerId)
        {
            // Для выдачи пока можно считать 1 задача = 1 сложность
            var count = await GetActiveWorkloadCountAsync(workerId);
            return count * 1.0;
        }

        public async Task<bool> HasNewAssignmentsAsync(int workerId)
        {
            return await _db.GetTable<OrderHandoverAssignmentModel>()
                .AnyAsync(a => a.AssignedToUserId == workerId && a.Status == 0);
        }

        // ==========================================
        // МЕТОДЫ СПИСКА (ВОЗВРАЩАЮТ ТОЛЬКО ЗАГОЛОВКИ)
        // ==========================================

        public async Task<IEnumerable<MobileBaseTaskDto>> GetAvailableTasksAsync(int workerId)
        {
            var pendingAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.AssignedToUserId == workerId && (a.Status == 0 || a.Status == 1))
                .ToListAsync();

            return await BuildHeaderListAsync(pendingAssignments);
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetActiveTasksAsync(int workerId)
        {
            var activeAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.AssignedToUserId == workerId && a.Status == 1)
                .ToListAsync();

            return await BuildHeaderListAsync(activeAssignments);
        }

        private async Task<List<MobileBaseTaskDto>> BuildHeaderListAsync(List<OrderHandoverAssignmentModel> assignments)
        {
            var result = new List<MobileBaseTaskDto>();

            // ИСПРАВЛЕНИЕ 3: Группируем назначения по TaskId, 
            // чтобы маршрут из 5 заказов рисовался как ОДНА карточка.
            var groupedAssignments = assignments.GroupBy(a => a.TaskId).ToList();

            foreach (var group in groupedAssignments)
            {
                var mainAssignment = group.First(); // берем первое для базы
                var baseTask = await _baseTaskService.GetById(mainAssignment.TaskId);

                // Собираем линии со всех заказов группы
                var assignmentIds = group.Select(a => a.Id).ToList();
                var lines = await _db.GetTable<OrderHandoverLineModel>()
                    .Where(l => assignmentIds.Contains(l.OrderHandoverAssignmentId))
                    .ToListAsync();

                int totalLines = lines.Count;
                int completedLines = lines.Count(l => l.ScannedQuantity >= l.Quantity);

                var headerDetails = new
                {
                    AssignmentId = mainAssignment.Id,
                    OrderId = mainAssignment.OrderId,
                    HandoverType = mainAssignment.HandoverType,
                    TotalLines = totalLines,
                    CompletedLines = completedLines
                };

                result.Add(new MobileBaseTaskDto
                {
                    TaskId = mainAssignment.TaskId,
                    Title = baseTask?.Title ?? $"Выдача: {group.Count()} заказ(ов)",
                    TaskType = this.TaskType,
                    PriorityLevel = baseTask?.PriorityLevel ?? 1,
                    Status = baseTask?.Status ?? TaskStatus.Assigned,
                    AssignmentStatus = (AssignmentStatus)mainAssignment.Status,
                    CreatedAt = mainAssignment.AssignedAt,
                    Deadline = baseTask?.Deadline,
                    TaskDetails = headerDetails
                });
            }
            return result;
        }


        public async Task<IEnumerable<int>> GetAssignedEmployeeIdsAsync(int taskId)
        {
            return await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.Status != 2 && a.Status != 3 && a.AssignedToUserId != null) // Отсекаем пустые
                .Select(a => a.AssignedToUserId.Value) 
                .Distinct()
                .ToListAsync();
        }

        // ==========================================
        // ПОЛНЫЕ ДЕТАЛИ ЗАДАЧИ
        // ==========================================

        public async Task<MobileBaseTaskDto?> GetTaskDetailsAsync(int taskId, int workerId)
        {
            // Поскольку у нас УЖЕ написан метод сборки богатого DTO в OrderHandoverExecutionProvider,
            // мы не будем дублировать код. Мы просто достанем базовые данные и 
            // "попросим" ExecutionProvider (или просто скопируем логику, если не хотим связывать провайдеры).

            // Но чтобы соблюсти чистоту (Workload отвечает за сборку DTO), перенесем логику сюды:

            var assignment = await _db.GetTable<OrderHandoverAssignmentModel>()
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedToUserId == workerId);

            if (assignment == null) return null;

            var baseTask = await _baseTaskService.GetById(taskId);

            // Чтобы не дублировать код из ExecutionProvider.GetTaskDetailsAsync, 
            // мы можем либо внедрить его сюда, либо скопировать метод генерации HandoverTaskDetailsDto.
            // Для простоты скопируем базовую логику сборки DTO:

            var richDetails = await BuildFullDetailsDtoAsync(assignment, baseTask);

            return new MobileBaseTaskDto
            {
                TaskId = assignment.TaskId,
                Title = baseTask?.Title ?? $"Выдача заказа #{assignment.OrderId}",
                TaskType = this.TaskType,
                PriorityLevel = baseTask?.PriorityLevel ?? 1,
                Status = baseTask?.Status ?? TaskStatus.Assigned,
                AssignmentStatus = (AssignmentStatus)assignment.Status,
                CreatedAt = assignment.AssignedAt,
                Deadline = baseTask?.Deadline,
                TaskDetails = richDetails
            };
        }

        private async Task<HandoverTaskDetailsDto> BuildFullDetailsDtoAsync(OrderHandoverAssignmentModel currentAssignment, BaseTaskDto baseTask)
        {
            var allAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                                          .Where(a => a.TaskId == currentAssignment.TaskId)
                                          .ToListAsync();

            // ИСПРАВЛЕНИЕ 4: Напарник - это человек с другим ID
            var partnerAssignment = allAssignments.FirstOrDefault(a =>
                a.AssignedToUserId != null &&
                a.AssignedToUserId != currentAssignment.AssignedToUserId);

            bool isCooperative = partnerAssignment != null;
            string partnerName = null;

            if (isCooperative)
            {
                var partnerUser = await _db.GetTable<EmployeeModel>().FirstOrDefaultAsync(u => u.EmployeesId == partnerAssignment.AssignedToUserId);
                partnerName = partnerUser?.FullName ?? $"ID: {partnerAssignment.AssignedToUserId}";
            }

            var dto = new HandoverTaskDetailsDto
            {
                AssignmentId = currentAssignment.Id,
                TaskId = currentAssignment.TaskId,
                TaskNumber = baseTask?.Title ?? $"Выдача заказа #{currentAssignment.OrderId}",
                OrderId = currentAssignment.OrderId,
                HandoverType = currentAssignment.HandoverType,
                Status = currentAssignment.Status,
                IsCooperative = isCooperative,
                PartnerName = partnerName,
                PartnerStatus = partnerAssignment?.Status
            };
            if (currentAssignment.HandoverType == "ToCourier" && currentAssignment.TargetCourierId.HasValue)
            {
                var courierInfo = await _db.GetTable<EmployeeModel>()
                    .FirstOrDefaultAsync(u => u.EmployeesId == currentAssignment.TargetCourierId.Value);
                dto.TargetName = courierInfo?.FullName ?? $"Курьер ID: {currentAssignment.TargetCourierId.Value}";
            }
            else
            {
                dto.TargetName = "Покупатель (Самовывоз)";
            }
            var workerAssignments = allAssignments
                .Where(a => a.AssignedToUserId == currentAssignment.AssignedToUserId)
                .Select(a => a.Id)
                .ToList();
            var lines = await _db.GetTable<OrderHandoverLineModel>()
                                             .Where(l => workerAssignments.Contains(l.OrderHandoverAssignmentId))
                                             .ToListAsync();

            var positions = _db.GetTable<PositionModel>();
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var items = _db.GetTable<ItemModel>();

            foreach (var line in lines)
            {
                var itemInfo = await (from ip in itemPositions
                                      join i in items on ip.ItemId equals i.ItemId
                                      where ip.Id == line.ItemPositionId
                                      select new { i.ItemId, i.Name, ip.PositionId }).FirstOrDefaultAsync();

                string sourceCellCode = "Неизвестная ячейка";
                if (itemInfo != null)
                {
                    var sourcePosModel = await positions.FirstOrDefaultAsync(p => p.PositionId == itemInfo.PositionId);
                    sourceCellCode = GetFullPositionCode(sourcePosModel) ?? itemInfo.PositionId.ToString();
                }

                dto.ItemsToScan.Add(new HandoverItemDto
                {
                    LineId = line.Id,
                    ItemId = itemInfo?.ItemId ?? 0,
                    ItemName = itemInfo?.Name ?? "Неизвестный товар",
                    Barcode = (itemInfo?.ItemId ?? 0).ToString(),
                    SourceCellCode = sourceCellCode,
                    Quantity = line.Quantity,
                    ScannedQuantity = line.ScannedQuantity,
                    // Price опущен, если он не нужен для выдачи, или нужно дотянуть OrderPositionModel
                });
            }

            return dto;
        }

        private string GetFullPositionCode(PositionModel pos)
        {
            if (pos == null) return null;
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(pos.ZoneCode)) parts.Add(pos.ZoneCode);
            if (!string.IsNullOrEmpty(pos.FirstLevelStorageType)) parts.Add(pos.FirstLevelStorageType);
            if (!string.IsNullOrEmpty(pos.FLSNumber)) parts.Add(pos.FLSNumber);
            if (!string.IsNullOrEmpty(pos.SecondLevelStorage)) parts.Add(pos.SecondLevelStorage);
            if (!string.IsNullOrEmpty(pos.ThirdLevelStorage)) parts.Add(pos.ThirdLevelStorage);
            return string.Join("-", parts);
        }
    }
}