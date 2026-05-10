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
                .CountAsync(a => a.AssignedToUserId == workerId && (a.Status == 0 || a.Status == 1 || a.Status == 2));
        }

        public async Task<double> GetActiveWorkloadComplexityAsync(int workerId)
        {
            // Находим начало текущей смены
            var shiftStart = await _db.GetTable<CheckIOEmployeeModel>()
                .Where(c => c.EmployeeId == workerId &&
                            c.CheckType == "in" &&
                            c.CheckTimeStamp >= DateTime.UtcNow.AddHours(-14))
                .OrderByDescending(c => c.CheckTimeStamp)
                .Select(c => (DateTime?)c.CheckTimeStamp)
                .FirstOrDefaultAsync();

            if (shiftStart == null) return 0;

            // Суммируем сложность всех не отмененных задач возврата за смену
            return await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.AssignedToUserId == workerId &&
                            a.Status != 4 && // Исключаем отмененные
                            a.AssignedAt >= shiftStart)
                .SumAsync(a => a.Complexity);
        }

        public async Task<bool> HasNewAssignmentsAsync(int workerId)
        {
            return await _db.GetTable<ReturnAssignmentModel>()
                .AnyAsync(a => a.AssignedToUserId == workerId && a.Status == 0);
        }

        // --- БЛОК СПИСКОВ ЗАДАЧ ---

        public async Task<IEnumerable<MobileBaseTaskDto>> GetAvailableTasksAsync(int workerId)
        {
            // Возвращаем задачи как в статусе New (0), так и InProgress (1)
            return await GetMobileTasksByStatusesAsync(workerId, 0, 1, 2); 
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetActiveTasksAsync(int workerId)
        {
            // Только InProgress (1)
            return await GetMobileTasksByStatusesAsync(workerId, 1);
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetUnassignedPoolTasksAsync(int branchId)
        {
            // Фильтр: показываем задачу только если это Main, 
            // ИЛИ если это Helper, но Main уже кем-то занят.
            var unassignedTaskIds = await (
                from a in _db.GetTable<ReturnAssignmentModel>()
                join t in _db.GetTable<BaseTaskModel>() on a.TaskId equals t.TaskId
                where a.AssignedToUserId == null
                      && a.Status == 0
                      && t.BranchId == branchId
                      && (
                          a.Role == "Main" ||
                          (a.Role == "Helper" && _db.GetTable<ReturnAssignmentModel>()
                              .Any(ma => ma.TaskId == a.TaskId && ma.Role == "Main" && ma.AssignedToUserId != null))
                      )
                select a.TaskId
            ).Distinct().ToListAsync();

            if (!unassignedTaskIds.Any())
                return new List<MobileBaseTaskDto>();

            var tasks = new List<MobileBaseTaskDto>();

            var availableTasks = await _db.GetTable<BaseTaskModel>()
                .Where(t => unassignedTaskIds.Contains(t.TaskId) && t.Status == "New")
                .ToListAsync();

            foreach (var baseTask in availableTasks)
            {
                tasks.Add(new MobileBaseTaskDto
                {
                    TaskId = baseTask.TaskId,
                    Title = baseTask.Title,
                    Description = baseTask.Description,
                    BranchId = baseTask.BranchId,
                    Type = baseTask.Type,
                    CreatedAt = baseTask.CreatedAt,
                    PriorityLevel = baseTask.PriorityLevel,
                    Status = Enum.Parse<TaskStatus>(baseTask.Status),
                    TaskType = this.TaskType,
                    AssignmentStatus = AssignmentStatus.Assigned
                });
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

        private async Task<IEnumerable<MobileBaseTaskDto>> GetMobileTasksByStatusesAsync(int workerId, params int[] statuses)
        {
            var assignments = await _db.GetTable<ReturnAssignmentModel>()
                .Where(a => a.AssignedToUserId == workerId && statuses.Contains(a.Status))
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
            // 1. Поиск напарника для кооперативных задач
            var partnerAssignment = allAssignments.FirstOrDefault(a =>
                a.AssignedToUserId != null &&
                a.AssignedToUserId != workerAssignment.AssignedToUserId);

            bool isCooperative = partnerAssignment != null;
            string partnerName = null;
            int? partnerStatus = null;

            if (isCooperative)
            {
                var partnerUser = await _db.GetTable<EmployeeModel>()
                    .FirstOrDefaultAsync(u => u.EmployeesId == partnerAssignment.AssignedToUserId);

                partnerName = partnerUser?.FullName ?? $"Сотрудник ID: {partnerAssignment.AssignedToUserId}";
                partnerStatus = partnerAssignment.Status;
            }

            // 2. Формирование базового DTO
            var dto = new ReturnTaskDetailsDto
            {
                AssignmentId = workerAssignment.Id,
                TaskId = taskId,
                TaskNumber = $"RET-{taskId}",
                Status = workerAssignment.Status,
                Role = workerAssignment.Role,
                IsCooperative = isCooperative,
                PartnerName = partnerName,
                PartnerStatus = partnerStatus,
                ItemsToScan = new List<ReturnItemDto>()
            };

            // 3. Подготовка данных
            var lines = await _db.GetTable<ReturnLineModel>()
                .Where(l => l.ReturnAssignmentId == workerAssignment.Id)
                .ToListAsync();

            var positions = _db.GetTable<PositionModel>();
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var items = _db.GetTable<ItemModel>();

            // Кэширование всех ячеек филиала для оптимизации производительности
            var branchPositions = await positions
                .Where(p => p.BranchId == workerAssignment.BranchId)
                .ToListAsync();

            // 4. Обработка товарных позиций возврата
            foreach (var line in lines)
            {
                var itemData = await (from ip in itemPositions
                                      join i in items on ip.ItemId equals i.ItemId
                                      where ip.Id == line.ItemPositionId
                                      select new { Item = i, ip.PositionId }).FirstOrDefaultAsync();

                string sourceCellCode = "Неизвестная ячейка";
                if (itemData != null)
                {
                    var sourcePosModel = branchPositions.FirstOrDefault(p => p.PositionId == itemData.PositionId);
                    sourceCellCode = GetFullPositionCode(sourcePosModel) ?? itemData.PositionId.ToString();
                }

                // === ИНТЕЛЛЕКТУАЛЬНЫЙ ПОДБОР ЦЕЛЕВОЙ ЯЧЕЙКИ ===
                if (!line.TargetPositionId.HasValue && itemData != null)
                {
                    // Вызываем логику автоподбора с учетом габаритов и доступных зон
                    int? suggestedPosId = await SuggestTargetPositionAsync(itemData.Item, workerAssignment.BranchId);

                    if (suggestedPosId.HasValue)
                    {
                        // Фиксируем выбор в базе данных
                        await _db.GetTable<ReturnLineModel>()
                            .Where(l => l.Id == line.Id)
                            .Set(l => l.TargetPositionId, suggestedPosId.Value)
                            .UpdateAsync();

                        line.TargetPositionId = suggestedPosId.Value;
                    }
                }

                string targetCellCode = null;
                if (line.TargetPositionId.HasValue)
                {
                    var targetPosInfo = branchPositions.FirstOrDefault(p => p.PositionId == line.TargetPositionId.Value);
                    targetCellCode = GetFullPositionCode(targetPosInfo);
                }

                dto.ItemsToScan.Add(new ReturnItemDto
                {
                    LineId = line.Id,
                    ItemId = itemData?.Item.ItemId ?? 0,
                    ItemName = itemData?.Item.Name ?? "Неизвестный товар",
                    Barcode = (itemData?.Item.ItemId ?? 0).ToString(),
                    Quantity = line.Quantity,
                    ScannedQuantity = line.ScannedQuantity,
                    SourceCellCode = sourceCellCode,
                    TargetCellCode = targetCellCode ?? "Определить на месте"
                });
            }

            return dto;
        }

        // =====================================================================
        // Логика подбора оптимальной ячейки с фильтрацией зарезервированных зон
        // =====================================================================
        private async Task<int?> SuggestTargetPositionAsync(ItemModel item, int branchId)
        {
            var positions = _db.GetTable<PositionModel>();
            var itemPositions = _db.GetTable<ItemPositionModel>();

            // Список зон, зарезервированных под отгрузку и выдачу, куда нельзя возвращать товар
            var reservedZones = new[] { "PICKUP", "BULK", "COURIER", "EXPRESS" };

            double itemL = item.Length;
            double itemW = item.Width;
            double itemH = item.Height;

            // ПРИОРИТЕТ 1: Консолидация
            // Ищем ячейку в любой разрешенной зоне, где этот товар уже хранится.
            var existingPos = await (from p in positions
                                     join ip in itemPositions on p.PositionId equals ip.PositionId
                                     where p.BranchId == branchId
                                        && !reservedZones.Contains(p.ZoneCode)
                                        && ip.ItemId == item.ItemId
                                     select p.PositionId).FirstOrDefaultAsync();

            if (existingPos != 0) return existingPos;

            // ПРИОРИТЕТ 2: Поиск пустой ячейки по габаритам
            // Выбираем активную ячейку в любой разрешенной зоне, способную вместить товар.
            var emptyFittedPos = await (from p in positions
                                        where p.BranchId == branchId
                                           && !reservedZones.Contains(p.ZoneCode)
                                           && p.Status == "Active"
                                           && !_db.GetTable<ItemPositionModel>().Any(ip => ip.PositionId == p.PositionId)
                                           && (p.Length >= itemL && p.Width >= itemW && p.Height >= itemH)
                                        select p.PositionId).FirstOrDefaultAsync();

            if (emptyFittedPos != 0) return emptyFittedPos;

            // ПРИОРИТЕТ 3: Резервный вариант (Fallback)
            // Любая пустая активная ячейка вне зарезервированных зон.
            var fallbackPos = await (from p in positions
                                     where p.BranchId == branchId
                                        && !reservedZones.Contains(p.ZoneCode)
                                        && p.Status == "Active"
                                        && !_db.GetTable<ItemPositionModel>().Any(ip => ip.PositionId == p.PositionId)
                                     select p.PositionId).FirstOrDefaultAsync();

            if (fallbackPos != 0) return fallbackPos;

            return null;
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