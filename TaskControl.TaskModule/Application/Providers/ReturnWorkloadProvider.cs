using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model; // Для EmployeeModel
using TaskControl.InventoryModule.DataAccess.Model;   // Для PositionModel, ItemPositionModel, ItemModel
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Application.Providers
{
    public class ReturnWorkloadProvider : ITaskWorkloadProvider
    {
        private readonly IBaseTaskService _baseTaskService;
        private readonly ITaskDataConnection _db;

        // Провайдер отвечает только за задачи возврата
        public string TaskType => "ReturnToStock";

        public ReturnWorkloadProvider(
            IBaseTaskService baseTaskService,
            ITaskDataConnection db)
        {
            _baseTaskService = baseTaskService ?? throw new ArgumentNullException(nameof(baseTaskService));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // --- БЛОК РАСЧЕТА НАГРУЗКИ ---

        public async Task<int> GetActiveWorkloadCountAsync(int workerId)
        {
            // Считаем задачи в статусе Assigned (0) или InProgress (1)
            return await _db.GetTable<ReturnAssignmentModel>()
                .CountAsync(a => a.AssignedToUserId == workerId && (a.Status == 0 || a.Status == 1));
        }

        public async Task<double> GetActiveWorkloadComplexityAsync(int workerId)
        {
            // Суммируем Complexity для запущенных задач (InProgress = 1)
            var activeAssignments = await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.AssignedToUserId == workerId && a.Status == 1)
                .ToListAsync();

            return activeAssignments.Sum(a => a.Complexity);
        }

        public async Task<bool> HasNewAssignmentsAsync(int workerId)
        {
            return await _db.GetTable<ReturnAssignmentModel>()
                .AnyAsync(a => a.AssignedToUserId == workerId && a.Status == 0);
        }

        // --- БЛОК СПИСКОВ ЗАДАЧ ---

        public async Task<IEnumerable<MobileBaseTaskDto>> GetAvailableTasksAsync(int workerId)
        {
            return await GetMobileTasksByStatusAsync(workerId, 0); // New (Assigned)
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetActiveTasksAsync(int workerId)
        {
            return await GetMobileTasksByStatusAsync(workerId, 1); // InProgress
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetUnassignedPoolTasksAsync(int branchId)
        {
            var unassignedAssignments = await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.BranchId == branchId && a.AssignedToUserId == null && a.Status == 0) // Только New
                .ToListAsync();

            var tasks = new List<MobileBaseTaskDto>();
            foreach (var assignment in unassignedAssignments)
            {
                var baseTask = await _baseTaskService.GetById(assignment.TaskId);
                if (baseTask != null && baseTask.Status != TaskStatus.Completed && baseTask.Status != TaskStatus.Cancelled)
                {
                    // Детали для общего пула не генерируем, только базовую карточку
                    tasks.Add(MapToMobileDto(baseTask, assignment, null));
                }
            }
            return tasks;
        }

        public async Task<IEnumerable<int>> GetAssignedEmployeeIdsAsync(int taskId)
        {
            return await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.TaskId == taskId && a.AssignedToUserId != null)
                .Select(a => a.AssignedToUserId.Value)
                .Distinct()
                .ToListAsync();
        }

        // --- БЛОК ДЕТАЛЕЙ ЗАДАЧИ ---

        public async Task<MobileBaseTaskDto?> GetTaskDetailsAsync(int taskId, int workerId)
        {
            var baseTask = await _baseTaskService.GetById(taskId);
            if (baseTask == null || baseTask.Type != TaskType) return null;

            var assignments = await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.TaskId == taskId)
                .ToListAsync();

            var workerAssignment = assignments.FirstOrDefault(a => a.AssignedToUserId == workerId);
            if (workerAssignment == null) return null;

            // Генерируем вложенные детали задачи (ReturnTaskDetailsDto)
            var taskDetails = await BuildReturnDetailsAsync(taskId, workerAssignment, assignments);

            // Возвращаем цельный объект с деталями внутри
            return MapToMobileDto(baseTask, workerAssignment, taskDetails);
        }

        // --- ПРИВАТНЫЕ Вспомогательные методы ---

        private async Task<IEnumerable<MobileBaseTaskDto>> GetMobileTasksByStatusAsync(int workerId, int assignmentStatus)
        {
            var assignments = await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.AssignedToUserId == workerId && a.Status == assignmentStatus)
                .ToListAsync();

            var tasks = new List<MobileBaseTaskDto>();
            foreach (var assignment in assignments)
            {
                var baseTask = await _baseTaskService.GetById(assignment.TaskId);
                if (baseTask != null)
                {
                    // Для списков нам не нужны полные детали, чтобы не грузить БД
                    tasks.Add(MapToMobileDto(baseTask, assignment, null));
                }
            }
            return tasks;
        }

        /// <summary>
        /// ИСПРАВЛЕНИЕ: Используем Object Initializer для init-свойств record'ов
        /// </summary>
        private MobileBaseTaskDto MapToMobileDto(BaseTaskDto baseTask, ReturnAssignmentModel assignment, object details)
        {
            return new MobileBaseTaskDto
            {
                TaskId = baseTask.TaskId,
                Title = baseTask.Title,
                Description = baseTask.Description,
                BranchId = baseTask.BranchId,
                Type = baseTask.Type,
                CreatedAt = baseTask.CreatedAt,
                CompletedAt = baseTask.CompletedAt,
                PriorityLevel = baseTask.PriorityLevel,
                Status = baseTask.Status,
                Deadline = baseTask.Deadline,

                // Свойства MobileBaseTaskDto
                TaskType = this.TaskType,
                AssignmentStatus = (AssignmentStatus)assignment.Status,
                TaskDetails = details // null для списка, заполненный DTO для экрана деталей
            };
        }

        private async Task<ReturnTaskDetailsDto> BuildReturnDetailsAsync(int taskId, ReturnAssignmentModel workerAssignment, List<ReturnAssignmentModel> allAssignments)
        {
            // Поиск напарника (если задача кооперативная)
            var partnerAssignment = allAssignments.FirstOrDefault(a =>
                a.AssignedToUserId != null &&
                a.AssignedToUserId != workerAssignment.AssignedToUserId);

            bool isCooperative = partnerAssignment != null;
            string partnerName = null;
            int? partnerStatus = null;

            if (isCooperative)
            {
                // ИСПРАВЛЕНИЕ: Используем EmployeesId и FullName
                var partnerUser = await _db.GetTable<EmployeeModel>().FirstOrDefaultAsync(u => u.EmployeesId == partnerAssignment.AssignedToUserId);
                partnerName = partnerUser?.FullName ?? $"Сотрудник ID: {partnerAssignment.AssignedToUserId}";
                partnerStatus = partnerAssignment.Status;
            }

            var dto = new ReturnTaskDetailsDto
            {
                AssignmentId = workerAssignment.Id,
                TaskId = taskId,
                TaskNumber = $"RET-{taskId}",
                Status = workerAssignment.Status,
                Role = workerAssignment.Role,
                IsCooperative = isCooperative,
                PartnerName = partnerName,
                PartnerStatus = partnerStatus
            };

            // Получаем линии возврата
            var lines = await _db.GetTable<ReturnLineModel>()
                .Where(l => l.ReturnAssignmentId == workerAssignment.Id)
                .ToListAsync();

            var positions = _db.GetTable<PositionModel>();
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var items = _db.GetTable<ItemModel>();

            foreach (var line in lines)
            {
                // Собираем инфу о товаре и исходной ячейке
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

                string targetCellCode = null;
                if (line.TargetPositionId.HasValue)
                {
                    var targetPosInfo = await positions.FirstOrDefaultAsync(p => p.PositionId == line.TargetPositionId.Value);
                    targetCellCode = GetFullPositionCode(targetPosInfo);
                }

                dto.ItemsToScan.Add(new ReturnItemDto
                {
                    LineId = line.Id,
                    ItemId = itemInfo?.ItemId ?? 0,
                    ItemName = itemInfo?.Name ?? "Неизвестный товар",
                    Barcode = (itemInfo?.ItemId ?? 0).ToString(),
                    Quantity = line.Quantity,
                    ScannedQuantity = line.ScannedQuantity,
                    SourceCellCode = sourceCellCode,
                    TargetCellCode = targetCellCode ?? "Определить на месте"
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