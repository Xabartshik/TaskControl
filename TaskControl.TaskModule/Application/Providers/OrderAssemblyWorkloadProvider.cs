using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;
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
    public class OrderAssemblyWorkloadProvider : ITaskWorkloadProvider
    {
        private readonly IOrderAssemblyAssignmentRepository _assemblyRepo;
        private readonly IBaseTaskService _baseTaskService;
        private readonly ITaskDataConnection _db;

        public string TaskType => "OrderAssembly";

        public OrderAssemblyWorkloadProvider(
            IOrderAssemblyAssignmentRepository assemblyRepo,
            IBaseTaskService baseTaskService,
            ITaskDataConnection db)
        {
            _assemblyRepo = assemblyRepo;
            _baseTaskService = baseTaskService;
            _db = db;
        }

        /// <summary>
        /// Получить список неназначенных задач сборки для конкретного филиала
        /// </summary>
        public async Task<IEnumerable<MobileBaseTaskDto>> GetUnassignedPoolTasksAsync(int branchId)
        {
            // 1. Ищем ID задач в таблице назначений сборки, где исполнитель не указан
            var unassignedAssemblyTaskIds = await _db.GetTable<OrderAssemblyAssignmentModel>()
                .Where(a => a.AssignedToUserId == null && a.BranchId == branchId && a.Status == 0) // 0 = New
                .Select(a => a.TaskId)
                .Distinct()
                .ToListAsync();

            if (!unassignedAssemblyTaskIds.Any())
                return new List<MobileBaseTaskDto>();

            // 2. Получаем базовые данные задач из таблицы base_tasks
            var availableTasks = await _db.GetTable<BaseTaskModel>()
                .Where(t => unassignedAssemblyTaskIds.Contains(t.TaskId) && t.Status == "New")
                .ToListAsync();

            var dtos = new List<MobileBaseTaskDto>();
            foreach (var task in availableTasks)
            {
                // Для сборки заказов информативно будет показать номер заказа в описании
                var assignment = await _db.GetTable<OrderAssemblyAssignmentModel>()
                    .FirstOrDefaultAsync(a => a.TaskId == task.TaskId);

                dtos.Add(new MobileBaseTaskDto
                {
                    TaskId = task.TaskId,
                    Title = task.Title,
                    TaskType = this.TaskType,
                    Status = Enum.Parse<TaskStatus>(task.Status),
                    PriorityLevel = task.PriorityLevel,
                    CreatedAt = task.CreatedAt,
                    Deadline = task.Deadline,
                    Description = assignment != null ? $"Сборка заказа #{assignment.OrderId}" : "Cборка заказа"
                });
            }

            return dtos;
        }

        public async Task<int> GetActiveWorkloadCountAsync(int workerId)
        {
            var tasks = await _assemblyRepo.GetByUserIdAsync(workerId);
            return tasks.Count(t => (int)t.Status == 0 || (int)t.Status == 1);
        }

        public async Task<double> GetActiveWorkloadComplexityAsync(int workerId)
        {
            // 1. Находим время последней отметки "in" (не ранее чем 14 часов назад)
            var shiftStart = await _db.GetTable<CheckIOEmployeeModel>()
                .Where(c => c.EmployeeId == workerId &&
                            c.CheckType == "in" &&
                            c.CheckTimeStamp >= DateTime.UtcNow.AddHours(-14))
                .OrderByDescending(c => c.CheckTimeStamp)
                .Select(c => (DateTime?)c.CheckTimeStamp)
                .FirstOrDefaultAsync(); 

            // Если сотрудник не зачекинен, нагрузка за смену считается нулевой
            if (shiftStart == null) return 0;

            // 2. Получаем все задачи сотрудника из репозитория
            var tasks = await _assemblyRepo.GetByUserIdAsync(workerId);

            // 3. Фильтруем и суммируем:
            // Берем всё, кроме отмененных (4), что было назначено в текущую смену
            return tasks
                .Where(t => (int)t.Status != 4 && t.AssignedAt >= shiftStart)
                .Sum(t => t.Complexity);
        }

        public async Task<bool> HasNewAssignmentsAsync(int workerId)
        {
            var tasks = await _assemblyRepo.GetByUserIdAsync(workerId);
            return tasks.Any(t => (int)t.Status == 0);
        }

        // ==========================================
        // МЕТОДЫ СПИСКА (ВОЗВРАЩАЮТ ТОЛЬКО ЗАГОЛОВКИ)
        // ==========================================

        public async Task<IEnumerable<MobileBaseTaskDto>> GetAvailableTasksAsync(int workerId)
        {
            var assignments = await _assemblyRepo.GetByUserIdAsync(workerId);
            var pending = assignments.Where(t => t.Status == AssignmentStatus.Assigned || t.Status == AssignmentStatus.InProgress || t.Status == AssignmentStatus.Paused).ToList();

            var result = new List<MobileBaseTaskDto>();
            foreach (var a in pending)
            {
                var baseTask = await _baseTaskService.GetById(a.TaskId);

                // ЛЕГКОВЕСНЫЙ ЗАГОЛОВОК: Никаких ячеек, только ID и прогресс
                var headerDetails = new
                {
                    AssignmentId = a.Id,
                    OrderId = a.OrderId,
                    TotalLines = a.TotalLines,
                    CompletedLines = a.Lines?.Count(l => l.Status == OrderAssemblyLineStatus.Placed) ?? 0
                };

                result.Add(new MobileBaseTaskDto
                {
                    TaskId = a.TaskId,
                    Title = baseTask?.Title ?? $"Сборка заказа #{a.OrderId}",
                    TaskType = this.TaskType,
                    PriorityLevel = baseTask?.PriorityLevel ?? 1,
                    Status = baseTask?.Status ?? TaskStatus.Assigned,
                    AssignmentStatus = a.Status,
                    CreatedAt = a.AssignedAt,
                    Deadline = baseTask?.Deadline,
                    TaskDetails = headerDetails
                });
            }
            return result;
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetActiveTasksAsync(int workerId)
        {
            var assignments = await _assemblyRepo.GetByUserIdAsync(workerId);
            var activeAssignments = assignments.Where(t => t.Status == AssignmentStatus.InProgress).ToList();

            var result = new List<MobileBaseTaskDto>();
            foreach (var a in activeAssignments)
            {
                var baseTask = await _baseTaskService.GetById(a.TaskId);

                var headerDetails = new
                {
                    AssignmentId = a.Id,
                    OrderId = a.OrderId,
                    TotalLines = a.TotalLines,
                    CompletedLines = a.Lines?.Count(l => l.Status == OrderAssemblyLineStatus.Placed) ?? 0
                };

                result.Add(new MobileBaseTaskDto
                {
                    TaskId = a.TaskId,
                    Title = baseTask?.Title ?? $"Сборка заказа #{a.OrderId}",
                    TaskType = this.TaskType,
                    PriorityLevel = baseTask?.PriorityLevel ?? 1,
                    Status = baseTask?.Status ?? TaskStatus.Assigned,
                    AssignmentStatus = a.Status,
                    CreatedAt = a.AssignedAt,
                    Deadline = baseTask?.Deadline,
                    TaskDetails = headerDetails
                });
            }
            return result;
        }

        public async Task<IEnumerable<int>> GetAssignedEmployeeIdsAsync(int taskId)
        {
            var assignments = await _assemblyRepo.GetByTaskIdAsync(taskId);
            return assignments
                .Where(a => a.Status != AssignmentStatus.Completed && a.Status != AssignmentStatus.Cancelled)
                .Select(a => a.AssignedToUserId)
                .Distinct()
                .ToList();
        }

        // ==========================================
        // НОВЫЙ МЕТОД: ПОЛНЫЕ ДЕТАЛИ ЗАДАЧИ
        // ==========================================

        public async Task<MobileBaseTaskDto?> GetTaskDetailsAsync(int taskId, int workerId)
        {
            var assignments = await _assemblyRepo.GetByUserIdAsync(workerId);
            var assignment = assignments.FirstOrDefault(a => a.TaskId == taskId);

            if (assignment == null) return null;

            var baseTask = await _baseTaskService.GetById(taskId);
            var richDetails = await BuildTaskDetailsAsync(assignment, baseTask);

            return new MobileBaseTaskDto
            {
                TaskId = assignment.TaskId,
                Title = baseTask?.Title ?? $"Сборка заказа #{assignment.OrderId}",
                TaskType = this.TaskType,
                PriorityLevel = baseTask?.PriorityLevel ?? 1,
                Status = baseTask?.Status ?? TaskStatus.Assigned,
                AssignmentStatus = assignment.Status,
                CreatedAt = assignment.AssignedAt,
                Deadline = baseTask?.Deadline,
                TaskDetails = richDetails
            };
        }

        // ==========================================
        // ПРИВАТНЫЕ МЕТОДЫ (ГЕНЕРАЦИЯ БОГАТОГО DTO)
        // ==========================================

        private async Task<WorkerAssemblyTaskDto> BuildTaskDetailsAsync(OrderAssemblyAssignment assignment, BaseTaskDto baseTaskModel)
        {
            var allTaskAssignments = await _db.GetTable<OrderAssemblyAssignmentModel>()
                                              .Where(x => x.TaskId == assignment.TaskId)
                                              .ToListAsync();

            var partnerAssignment = allTaskAssignments.FirstOrDefault(x => x.Id != assignment.Id);
            bool isCooperative = partnerAssignment != null;
            string partnerName = null;

            if (isCooperative)
            {
                var partnerUser = await _db.GetTable<EmployeeModel>().FirstOrDefaultAsync(u => u.EmployeesId == partnerAssignment.AssignedToUserId);
                partnerName = partnerUser?.FullName ?? $"ID: {partnerAssignment.AssignedToUserId}";
            }

            var dto = new WorkerAssemblyTaskDto
            {
                AssignmentId = assignment.Id,
                TaskId = assignment.TaskId,
                TaskNumber = baseTaskModel?.Title ?? $"T-{assignment.TaskId}",
                OrderId = assignment.OrderId,
                Status = assignment.Status,
                Deadline = baseTaskModel?.Deadline,
                CreatedDate = baseTaskModel?.CreatedAt,
                TotalLines = assignment.TotalLines,

                // ВАЖНО: Добавлено заполнение CompletedLines
                CompletedLines = assignment.Lines?.Count(l => l.Status == OrderAssemblyLineStatus.Placed) ?? 0,

                IsCooperative = isCooperative,
                PartnerName = partnerName,
                PartnerStatus = partnerAssignment != null ? (AssignmentStatus?)partnerAssignment.Status : null
            };

            var positions = _db.GetTable<PositionModel>();
            var itemPositions = _db.GetTable<ItemPositionModel>();
            var items = _db.GetTable<ItemModel>();

            var cellGroups = assignment.Lines.GroupBy(l => l.TargetPositionId);
            foreach (var g in cellGroups)
            {
                var targetId = g.Key;
                var posModel = await positions.FirstOrDefaultAsync(p => p.PositionId == targetId);
                var fullCode = GetFullPositionCode(posModel) ?? targetId.ToString();

                var cellDto = new CellPlacementInfoDto
                {
                    TargetPositionId = targetId,
                    CellCode = fullCode,
                    CellDisplayName = fullCode
                };

                foreach (var l in g)
                {
                    var itemInfo = await (from ip in itemPositions
                                          join i in items on ip.ItemId equals i.ItemId
                                          where ip.Id == l.ItemPositionId
                                          select new { i.ItemId, i.Name, ip.PositionId }).FirstOrDefaultAsync();

                    string sourceCellCode = "Неизвестная ячейка";
                    if (itemInfo != null)
                    {
                        var sourcePosModel = await positions.FirstOrDefaultAsync(p => p.PositionId == itemInfo.PositionId);
                        sourceCellCode = GetFullPositionCode(sourcePosModel) ?? itemInfo.PositionId.ToString();
                    }

                    cellDto.Items.Add(new PlacementLineDto
                    {
                        LineId = l.Id,
                        ItemPositionId = l.ItemPositionId,
                        ItemId = itemInfo?.ItemId ?? 0,
                        ItemName = itemInfo?.Name ?? "Неизвестный товар",
                        Barcode = (itemInfo?.ItemId ?? 0).ToString(),
                        SourceCellCode = sourceCellCode,
                        Quantity = l.Quantity,
                        PickedQuantity = l.PickedQuantity,
                        Status = l.Status// Явный перевод в строку
                    });
                }
                dto.CellPlacements.Add(cellDto);
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