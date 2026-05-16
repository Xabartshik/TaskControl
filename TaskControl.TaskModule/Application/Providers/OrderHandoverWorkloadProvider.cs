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
            // ИСПРАВЛЕНИЕ: Фильтр в Общем пуле. 
            // Помощник видит задачу только если роль Main уже кем-то занята.
            var availableTasks = await (
                from a in _db.GetTable<OrderHandoverAssignmentModel>()
                join t in _db.GetTable<BaseTaskModel>() on a.TaskId equals t.TaskId
                where a.AssignedToUserId == null
                      && a.Status == 0 // 0 = New
                      && t.BranchId == branchId
                      && (
                          a.Role == "Main" || // Главные роли всегда видны
                          (a.Role == "Helper" && _db.GetTable<OrderHandoverAssignmentModel>()
                              .Any(ma => ma.TaskId == a.TaskId && ma.Role == "Main" && ma.AssignedToUserId != null)) // Помощник виден только если есть Главный
                      )
                select t
            ).Distinct().ToListAsync();

            if (!availableTasks.Any())
                return new List<MobileBaseTaskDto>();

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
                .CountAsync(a => a.AssignedToUserId == workerId && (a.Status == 0 || a.Status == 1 || a.Status == 2));
        }

        public async Task<double> GetActiveWorkloadComplexityAsync(int workerId)
        {
            // 1. Находим время начала текущей смены (последний "in" за последние 14 часов)
            var shiftStart = await _db.GetTable<CheckIOEmployeeModel>()
                .Where(c => c.EmployeeId == workerId &&
                            c.CheckType == "in" &&
                            c.CheckTimeStamp >= DateTime.UtcNow.AddHours(-14))
                .OrderByDescending(c => c.CheckTimeStamp)
                .Select(c => (DateTime?)c.CheckTimeStamp)
                .FirstOrDefaultAsync();

            // Если отметки о входе нет, считаем, что сотрудник не на смене и нагрузка 0
            if (shiftStart == null) return 0;

            // 2. Высчитываем суммарную сложность всех задач по выдаче (Handover)
            return await _db.GetTable<OrderHandoverAssignmentModel>()
                .Where(a => a.AssignedToUserId == workerId &&
                            a.Status != 4 &&
                            a.AssignedAt >= shiftStart)
                .SumAsync(a => a.Complexity);
        }

        public async Task<bool> HasNewAssignmentsAsync(int workerId)
        {
            return await _db.GetTable<OrderHandoverAssignmentModel>()
                .AnyAsync(a => a.AssignedToUserId == workerId && a.Status == 0);
        }

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
                .Where(a => a.AssignedToUserId == workerId
                         && (a.Status == 0 || a.Status == 1 || a.Status == 2))
                .ToListAsync();

            return await BuildHeaderListAsync(activeAssignments);
        }

        private async Task<List<MobileBaseTaskDto>> BuildHeaderListAsync(List<OrderHandoverAssignmentModel> assignments)
        {
            var result = new List<MobileBaseTaskDto>();

            var groupedAssignments = assignments.GroupBy(a => a.TaskId).ToList();

            foreach (var group in groupedAssignments)
            {
                var mainAssignment = group.First();
                var baseTask = await _baseTaskService.GetById(mainAssignment.TaskId);

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
                .Where(a => a.TaskId == taskId && a.Status != 2 && a.Status != 3 && a.AssignedToUserId != null)
                .Select(a => a.AssignedToUserId.Value)
                .Distinct()
                .ToListAsync();
        }

        public async Task<MobileBaseTaskDto?> GetTaskDetailsAsync(int taskId, int workerId)
        {
            var assignment = await _db.GetTable<OrderHandoverAssignmentModel>()
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.AssignedToUserId == workerId);

            if (assignment == null) return null;

            var baseTask = await _baseTaskService.GetById(taskId);
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
                                      select new { i.ItemId, i.Name, i.Barcode, ip.PositionId }).FirstOrDefaultAsync();

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
                    Barcode = itemInfo?.Barcode ?? "Неизвестный штрих-код",
                    SourceCellCode = sourceCellCode,
                    Quantity = line.Quantity,
                    ScannedQuantity = line.ScannedQuantity
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