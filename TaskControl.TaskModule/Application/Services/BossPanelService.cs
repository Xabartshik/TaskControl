using Microsoft.Extensions.Logging;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.DAL.Repositories;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Repositories;
using TaskControl.OrderModule.DataAccess.Interface;
using TaskControl.OrderModule.Domain;
using TaskControl.TaskModule.Application.DTOs.BossPanelDTOs;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;


namespace TaskControl.TaskModule.Application.Services
{
    /// <summary>
    /// Сервис для панели начальника
    /// </summary>
    public class BossPanelService : IBossPanelService
    {
        private readonly IInventoryProcessService _inventoryProcessService;
        private readonly IInventoryReportService _inventoryReportService;
        private readonly IDiscrepancyManagementService _discrepancyManagementService;
        private readonly ActiveEmployeeService _activeEmployeeService;
        private readonly IInventoryAssignmentRepository _assignmentRepository;
        private readonly IOrderAssemblyAssignmentRepository _orderAssemblyRepository;
        private readonly IPositionCellRepository _positionCellRepository;
        private readonly IItemPositionRepository _itemPositionRepository;
        private readonly IActiveTaskRepository _activeTaskRepository;
        private readonly TaskWorkloadAggregator _aggregator;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<BossPanelService> _logger;
        private readonly IPostamatRepository _postamatRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IOrderPositionRepository _orderPositionRepository;
        private readonly IPostamatCellRepository _postamatCellRepository;

        public BossPanelService(
            IInventoryProcessService inventoryProcessService,
            IInventoryReportService inventoryReportService,
            IDiscrepancyManagementService discrepancyManagementService,
            ActiveEmployeeService activeEmployeeService,
            IInventoryAssignmentRepository assignmentRepository,
            IOrderAssemblyAssignmentRepository orderAssemblyRepository,
            IPositionCellRepository positionCellRepository,
            IItemPositionRepository itemPositionRepository,
            IActiveTaskRepository activeTaskRepository,
            TaskWorkloadAggregator aggregator,
            IOrderRepository orderRepository,
            IPostamatRepository postamatRepository,
            IItemRepository itemRepository,
            IOrderPositionRepository orderPositionRepository,
            IPostamatCellRepository postamatCellRepository,
            ILogger<BossPanelService> logger)
        {
            _inventoryProcessService = inventoryProcessService ?? throw new ArgumentNullException(nameof(inventoryProcessService));
            _inventoryReportService = inventoryReportService ?? throw new ArgumentNullException(nameof(inventoryReportService));
            _discrepancyManagementService = discrepancyManagementService ?? throw new ArgumentNullException(nameof(discrepancyManagementService));
            _activeEmployeeService = activeEmployeeService ?? throw new ArgumentNullException(nameof(activeEmployeeService));
            _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
            _orderAssemblyRepository = orderAssemblyRepository ?? throw new ArgumentNullException(nameof(orderAssemblyRepository));
            _positionCellRepository = positionCellRepository ?? throw new ArgumentNullException(nameof(positionCellRepository));
            _itemPositionRepository = itemPositionRepository ?? throw new ArgumentNullException(nameof(itemPositionRepository));
            _activeTaskRepository = activeTaskRepository ?? throw new ArgumentNullException(nameof(activeTaskRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _postamatRepository = postamatRepository ?? throw new ArgumentNullException(nameof(postamatRepository));
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _orderPositionRepository = orderPositionRepository ?? throw new ArgumentNullException(nameof(orderPositionRepository));
            _postamatCellRepository = postamatCellRepository ?? throw new ArgumentNullException(nameof(postamatCellRepository));
        }


        public async Task<CompleteInventoryDto> CreateInventoryTaskAsync(CreateInventoryTaskDto dto, int bossBranchId)
        {
            _logger.LogInformation("+--- [Boss] создание инвентаризации: начальник {BossBranchId} -> филиал {DtoBranchId}", bossBranchId, dto.BranchId);

            if (dto.BranchId != bossBranchId)
            {
                _logger.LogWarning("|   ! отказ: начальник {BossBranchId} пытался создать задачу для чужого филиала {DtoBranchId}", bossBranchId, dto.BranchId);
                throw new InvalidOperationException($"Вы не можете создать инвентаризацию для филиала {dto.BranchId}. Доступен только филиал {bossBranchId}.");
            }

            var availableWorkers = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(dto.BranchId);
            var workerIds = availableWorkers.Select(w => w.EmployeeId).ToList();

            if (!workerIds.Any())
            {
                throw new InvalidOperationException($"В филиале {dto.BranchId} нет активных сотрудников для назначения инвентаризации.");
            }

            return await _inventoryProcessService.CreateAndDistributeInventoryAsync(dto, workerIds);
        }

        public async Task<IEnumerable<WorkerPerformanceReportDto>> GetBranchWorkersPerformanceAsync(int bossBranchId, DateTime from, DateTime to)
        {
            _logger.LogInformation("|   [Boss] запрос производительности (филиал {BossBranchId})", bossBranchId);

            var employees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
            var reports = new List<WorkerPerformanceReportDto>();

            foreach (var emp in employees)
            {
                try
                {
                    var report = await _inventoryReportService.GetWorkerPerformanceAsync(emp.EmployeeId, from, to);
                    reports.Add(report);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("|   ! ошибка получения отчета для сотрудника {EmployeeId}", emp.EmployeeId);
                }
            }

            return reports;
        }

        public async Task<DiscrepancyReportDto> GetBranchInventoryDiscrepanciesAsync(int bossBranchId, int assignmentId)
        {
            _logger.LogInformation("|   [Boss] запрос расхождений по назначению {AssignmentId}", assignmentId);

            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            if (assignment == null)
            {
                throw new InvalidOperationException($"Назначение {assignmentId} не найдено.");
            }

            if (assignment.BranchId != bossBranchId)
            {
                _logger.LogWarning("|   ! отказ: назначение {AssignmentId} не в филиале начальника", assignmentId);
                throw new InvalidOperationException($"Назначение {assignmentId} не относится к вашему филиалу.");
            }

            return await _discrepancyManagementService.GetDiscrepanciesAsync(assignmentId);
        }
        public async Task<IEnumerable<string>> GetAvailableZonesAsync(int bossBranchId)
        {
            _logger.LogInformation("|   [Boss] запрос доступных зон (филиал {BossBranchId})", bossBranchId);

            var cells = await _positionCellRepository.GetByBranchAsync(bossBranchId);

            // Возвращаем уникальные префиксы, например "1-ZA-RACK"
            var zones = cells
                .Select(c => $"{c.Code.BranchId}-{c.Code.ZoneCode}-{c.Code.FirstLevelStorageType}")
                .Distinct()
                .OrderBy(z => z)
                .ToList();

            return zones;
        }

        public async Task<CompleteInventoryDto> CreateInventoryByZoneAsync(CreateInventoryByZoneDto dto, int bossBranchId)
        {
            _logger.LogInformation("Создание инвентаризации по зонам для филиала {BossBranchId}", bossBranchId);

            var cells = await _positionCellRepository.GetByBranchAsync(bossBranchId);

            // Ищем все ячейки, которые начинаются с одного из префиксов
            var targetPositionIds = cells
                .Where(c => dto.ZonePrefixes.Any(prefix => c.Code.ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                .Select(c => c.PositionId)
                .ToList();

            if (!targetPositionIds.Any())
            {
                throw new InvalidOperationException("Не найдено ни одной позиции по указанным зонам.");
            }

            // Получаем ItemPositions для этих позиций
            var allItemPositions = await _itemPositionRepository.GetAllAsync();
            var targetItemPositionIds = allItemPositions
                .Where(ip => targetPositionIds.Contains(ip.PositionId))
                .Select(ip => ip.Id)
                .Distinct()
                .ToList();

            if (!targetItemPositionIds.Any())
            {
                throw new InvalidOperationException("В выбранных зонах нет ни одного привязанного товара.");
            }

            var workerIds = dto.WorkerIds;
            if (workerIds == null || !workerIds.Any())
            {
                // Если работники не переданы, выбираем активных
                var availableWorkers = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
                workerIds = availableWorkers.Select(w => w.EmployeeId).Take(dto.WorkerCount).ToList();

                if (!workerIds.Any())
                {
                    throw new InvalidOperationException($"В филиале {bossBranchId} нет активных сотрудников.");
                }
            }

            var createDto = new CreateInventoryTaskDto
            {
                BranchId = bossBranchId,
                ItemPositionIds = targetItemPositionIds,
                WorkerCount = workerIds.Count,
                DivisionStrategy = DivisionStrategy.ByQuantity,
                PriorityLevel = dto.PriorityLevel,
                //TODO: Добавить время дедлайна
                DeadlineDate = DateTime.Now.AddDays(3)
            };

            return await _inventoryProcessService.CreateAndDistributeInventoryAsync(createDto, workerIds);
        }

        public async Task<IEnumerable<WorkerStatusDto>> GetActiveWorkersStatusAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение статуса работников для филиала {BossBranchId}", bossBranchId);

            var employees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
            var result = new List<WorkerStatusDto>();

            foreach (var emp in employees)
            {
                //// Подсчет активных назначений инвентаризации
                //var invAssignments = await _assignmentRepository.GetByUserIdAsync(emp.EmployeeId);
                //var invActiveCount = invAssignments.Count(a => a.Status != AssignmentStatus.Completed && a.Status != AssignmentStatus.Cancelled);

                //// Подсчет активных назначений сборки заказов
                //var oaAssignments = await _orderAssemblyRepository.GetByUserIdAsync(emp.EmployeeId);
                //var oaActiveCount = oaAssignments.Count(a => a.Status != AssignmentStatus.Completed && a.Status != AssignmentStatus.Cancelled);

                //result.Add(new WorkerStatusDto
                //{
                //    EmployeeId = emp.EmployeeId,
                //    FullName = $"{emp.Surname} {emp.Name}",
                //    Role = emp.Role,
                //    IsWorking = true,
                //    ActiveTaskCount = invActiveCount + oaActiveCount
                //});
                result.Add(new WorkerStatusDto
                {
                    EmployeeId = emp.EmployeeId,
                    FullName = $"{emp.Surname} {emp.Name}",
                    Role = emp.Role,
                    IsWorking = true,
                    ActiveTaskCount = await _aggregator.GetTotalActiveWorkloadAsync(emp.EmployeeId)
                });
            }

            return result;
        }

        public async Task<IEnumerable<TaskReportGroupDto>> GetGroupedTaskReportsAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение сгруппированных отчетов по задачам для филиала {BossBranchId}", bossBranchId);

            var tasks = await _activeTaskRepository.GetByBranchAsync(bossBranchId);
            var result = new List<TaskReportGroupDto>();

            var assignments = await _assignmentRepository.GetByBranchIdAsync(bossBranchId);

            foreach (var task in tasks.OrderByDescending(t => t.CreatedAt))
            {
                var taskAssignments = assignments.Where(a => a.TaskId == task.TaskId).ToList();
                int total = taskAssignments.Count;
                int completed = taskAssignments.Count(a => a.Status == Domain.AssignmentStatus.Completed);

                string progress = total == 0 ? "Нет назначений" : $"{completed}/{total} завершено";

                result.Add(new TaskReportGroupDto
                {
                    TaskId = task.TaskId,
                    Title = task.Title,
                    Status = task.Status.ToString(),
                    CreatedAt = task.CreatedAt,
                    CompletedAt = task.CompletedAt,
                    AssignmentsProgress = progress
                });
            }

            return result;
        }

        public async Task<IEnumerable<BossPanelTaskCardDto>> GetActiveTasksAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение активных задач для филиала {BossBranchId}", bossBranchId);

            var tasks = await _activeTaskRepository.GetByBranchAsync(bossBranchId);
            var invAssignments = await _assignmentRepository.GetByBranchIdAsync(bossBranchId);

            var activeTasks = tasks.Where(t => t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled);
            var result = new List<BossPanelTaskCardDto>();

            foreach (var t in activeTasks)
            {
                var dict = new Dictionary<int, TaskAssigneeProgressDto>();

                if (t.Type == "OrderAssembly")
                {
                    // Для задач сборки заказов берём данные из репозитория сборки
                    var oaAssignment = await _orderAssemblyRepository.GetByTaskIdAsync(t.TaskId);
                    if (oaAssignment != null)
                    {
                        var emp = await _activeEmployeeService.GetEmployeeByIdAsync(oaAssignment.AssignedToUserId);
                        int total = oaAssignment.TotalLines;
                        int completed = oaAssignment.Lines.Count(l => l.Status == OrderAssemblyLineStatus.Placed);

                        dict[oaAssignment.AssignedToUserId] = new TaskAssigneeProgressDto
                        {
                            EmployeeId = oaAssignment.AssignedToUserId,
                            FullName = emp != null ? $"{emp.Surname} {emp.Name}" : $"Работник {oaAssignment.AssignedToUserId}",
                            AssignedVolume = total,
                            CompletedVolume = completed,
                            Status = oaAssignment.Status == AssignmentStatus.InProgress ? "В процессе"
                                     : oaAssignment.Status == AssignmentStatus.Completed ? "Завершено" : "Назначено"
                        };
                    }
                }
                else
                {
                    // Для инвентаризации оригинальная логика
                    var taskAssignments = invAssignments.Where(a => a.TaskId == t.TaskId).ToList();
                    foreach (var a in taskAssignments)
                    {
                        if (!dict.ContainsKey(a.AssignedToUserId))
                        {
                            var emp = await _activeEmployeeService.GetEmployeeByIdAsync(a.AssignedToUserId);
                            dict[a.AssignedToUserId] = new TaskAssigneeProgressDto
                            {
                                EmployeeId = a.AssignedToUserId,
                                FullName = emp != null ? $"{emp.Surname} {emp.Name}" : $"Работник {a.AssignedToUserId}",
                                AssignedVolume = 1,
                                CompletedVolume = a.Status == AssignmentStatus.Completed ? 1 : 0,
                                Status = a.Status == AssignmentStatus.InProgress ? "В процессе"
                                         : a.Status == AssignmentStatus.Completed ? "Завершено" : "Ожидается"
                            };
                        }
                        else
                        {
                            dict[a.AssignedToUserId].AssignedVolume += 1;
                            if (a.Status == AssignmentStatus.Completed)
                                dict[a.AssignedToUserId].CompletedVolume += 1;
                        }
                    }
                }

                var assignees = dict.Values.ToList();
                int totalAssigned = assignees.Sum(x => x.AssignedVolume);
                int totalCompleted = assignees.Sum(x => x.CompletedVolume);
                int progress = totalAssigned > 0 ? (totalCompleted * 100 / totalAssigned) : 0;

                result.Add(new BossPanelTaskCardDto
                {
                    Id = t.TaskId,
                    Title = t.Title,
                    TaskType = t.Type,
                    CreatedAt = t.CreatedAt,
                    ExpectedCompletionDate = null,
                    OverallProgressPercentage = progress,
                    Assignees = assignees
                });
            }

            return result;
        }

        public async Task<IEnumerable<EmployeeWorkloadDto>> GetEmployeeWorkloadAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение загруженности сотрудников филиала {BossBranchId}", bossBranchId);
            var employees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
            var result = new List<EmployeeWorkloadDto>();

            foreach (var emp in employees)
            {
                var activeTaskDtos = new List<ActiveTaskBriefDto>();

                // Задачи инвентаризации
                var invAssignments = await _assignmentRepository.GetByUserIdAsync(emp.EmployeeId);
                var activeInv = invAssignments.Where(a => a.Status != AssignmentStatus.Completed && a.Status != AssignmentStatus.Cancelled).ToList();
                foreach (var a in activeInv)
                {
                    var task = await _activeTaskRepository.GetByIdAsync(a.TaskId);
                    if (task != null)
                    {
                        activeTaskDtos.Add(new ActiveTaskBriefDto
                        {
                            TaskId = task.TaskId,
                            Title = task.Title,
                            TaskType = task.Type,
                            Status = a.Status.ToString()
                        });
                    }
                }

                // Задачи сборки заказов
                var oaAssignments = await _orderAssemblyRepository.GetByUserIdAsync(emp.EmployeeId);
                var activeOa = oaAssignments.Where(a => a.Status != AssignmentStatus.Completed && a.Status != AssignmentStatus.Cancelled).ToList();
                foreach (var a in activeOa)
                {
                    var task = await _activeTaskRepository.GetByIdAsync(a.TaskId);
                    if (task != null)
                    {
                        activeTaskDtos.Add(new ActiveTaskBriefDto
                        {
                            TaskId = task.TaskId,
                            Title = task.Title,
                            TaskType = task.Type,
                            Status = a.Status.ToString()
                        });
                    }
                }

                result.Add(new EmployeeWorkloadDto
                {
                    EmployeeId = emp.EmployeeId,
                    FullName = $"{emp.Surname} {emp.Name}",
                    IsAtWork = true,
                    ActiveTasksCount = activeTaskDtos.Count,
                    ActiveTasks = activeTaskDtos
                });
            }
            return result;
        }

        public async Task<IEnumerable<AvailableEmployeeDto>> GetAvailableEmployeesAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение доступных сотрудников филиала {BossBranchId}", bossBranchId);
            var employees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
            var result = new List<AvailableEmployeeDto>();

            foreach (var emp in employees)
            {
                // Инвентаризационные назначения
                var invAssignments = await _assignmentRepository.GetByUserIdAsync(emp.EmployeeId);
                var invActiveCount = invAssignments.Count(a => a.Status != AssignmentStatus.Completed && a.Status != AssignmentStatus.Cancelled);

                // Назначения сборки заказов
                var oaAssignments = await _orderAssemblyRepository.GetByUserIdAsync(emp.EmployeeId);
                var oaActiveCount = oaAssignments.Count(a => a.Status != AssignmentStatus.Completed && a.Status != AssignmentStatus.Cancelled);

                result.Add(new AvailableEmployeeDto
                {
                    EmployeeId = emp.EmployeeId,
                    FullName = $"{emp.Surname} {emp.Name}",
                    IsAtWork = true,
                    ActiveTasksCount = invActiveCount + oaActiveCount,
                    IsRecommended = false
                });
            }

            // Помечаем до 3 сотрудников с минимальным кол-вом задач как "Рекомендовано"
            var recommended = result.OrderBy(x => x.ActiveTasksCount).Take(3).ToList();
            foreach (var r in recommended) r.IsRecommended = true;

            return result;
        }

        public async Task<IEnumerable<PositionCellDto>> GetPositionsAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение позиций для дерева филиала {BossBranchId}", bossBranchId);
            var cells = await _positionCellRepository.GetByBranchAsync(bossBranchId);
            return cells.Select(c => PositionCellDto.ToDto(c)).ToList();
        }

        public async Task<IEnumerable<AvailableOrderDto>> GetAvailableOrdersAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение доступных заказов для филиала {BossBranchId}", bossBranchId);

            // Получаем все заказы в статусе Created для этого филиала
            var allOrders = (await _orderRepository.GetByBranchAsync(bossBranchId)).ToList();
            var newOrders = allOrders.Where(o => o.Status == OrderStatus.Created).ToList();

            _logger.LogInformation("Найдено заказов в филиале: {Total}, из них в статусе Created: {NewCount}", allOrders.Count, newOrders.Count);

            // Исключаем те, для которых уже созданы активные задания сборки
            var assemblyAssignments = await _orderAssemblyRepository.GetByBranchIdAsync(bossBranchId);
            var assignedOrderIds = assemblyAssignments
                .Where(a => a.Status != AssignmentStatus.Cancelled && a.Status != AssignmentStatus.Completed)
                .Select(a => a.OrderId)
                .ToHashSet();

            _logger.LogInformation("Активных назначений сборки в филиале: {ActiveCount}", assignedOrderIds.Count);

            var availableOrders = new List<AvailableOrderDto>();

            foreach (var o in newOrders.Where(o => !assignedOrderIds.Contains(o.OrderId)))
            {
                var dto = new AvailableOrderDto
                {
                    OrderId = o.OrderId,
                    OrderNumber = $"ORD-{o.OrderId}",
                    CreatedAt = o.CreatedAt,
                    Status = o.Status.ToString(),
                    DeliveryType = o.DeliveryType.ToString(),
                    PaymentType = o.PaymentType.ToString(),
                    DestinationAddress = o.DestinationAddress
                };

                // 1. Обогащаем данными о постамате, если это доставка в постамат
                if (o.PostamatId.HasValue)
                {
                    var postamat = await _postamatRepository.GetByIdAsync(o.PostamatId.Value);
                    dto.PostamatAddress = postamat?.Address;

                    if (o.PostamatCellId.HasValue)
                    {
                        var cell = await _postamatCellRepository.GetByIdAsync(o.PostamatCellId.Value);
                        dto.PostamatCellNumber = cell?.CellNumber;
                        dto.PostamatCellSize = cell?.SizeLabel;
                    }
                }

                // 2. Обогащаем составом заказа (списком товаров)
                var positions = await _orderPositionRepository.GetByOrderIdAsync(o.OrderId);
                foreach (var pos in positions)
                {
                    var item = await _itemRepository.GetByIdAsync(pos.ItemId);
                    dto.Items.Add(new OrderItemDetailDto
                    {
                        ItemId = pos.ItemId,
                        Name = item?.Name ?? "Неизвестный товар",
                        Quantity = pos.Quantity
                    });
                }

                availableOrders.Add(dto);
            }

            // 3. Сортировка (Оптимизация процессов сборки)
            // Экспресс-заказы поднимаются наверх, остальные сортируются по времени ожидания (старые первыми)
            var sortedOrders = availableOrders
                .OrderByDescending(o => o.IsHighPriority)
                .ThenBy(o => o.CreatedAt)
                .ToList();

            _logger.LogInformation("Итого доступно для выбора: {AvailableCount}", sortedOrders.Count);

            return sortedOrders;
        }


        public async Task<IEnumerable<AvailableOrderDto>> GetAllOrdersAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение всех заказов для филиала {BossBranchId}", bossBranchId);

            // 1. Получаем все заказы филиала без исключений
            var allOrders = (await _orderRepository.GetByBranchAsync(bossBranchId)).ToList();

            _logger.LogInformation("Найдено заказов в филиале: {Total}", allOrders.Count);

            var availableOrders = new List<AvailableOrderDto>();

            foreach (var o in allOrders)
            {
                var dto = new AvailableOrderDto
                {
                    OrderId = o.OrderId,
                    OrderNumber = $"ORD-{o.OrderId}",
                    CreatedAt = o.CreatedAt,
                    Status = o.Status.ToString(),
                    DeliveryType = o.DeliveryType.ToString(),
                    PaymentType = o.PaymentType.ToString(),
                    DestinationAddress = o.DestinationAddress
                };

                // 2. Обогащаем данными о постамате (если применимо)
                if (o.PostamatId.HasValue)
                {
                    var postamat = await _postamatRepository.GetByIdAsync(o.PostamatId.Value);
                    dto.PostamatAddress = postamat?.Address;

                    if (o.PostamatCellId.HasValue)
                    {
                        var cell = await _postamatCellRepository.GetByIdAsync(o.PostamatCellId.Value);
                        dto.PostamatCellNumber = cell?.CellNumber;
                        dto.PostamatCellSize = cell?.SizeLabel;
                    }
                }

                // 3. Обогащаем составом заказа
                var positions = await _orderPositionRepository.GetByOrderIdAsync(o.OrderId);
                foreach (var pos in positions)
                {
                    var item = await _itemRepository.GetByIdAsync(pos.ItemId);
                    dto.Items.Add(new OrderItemDetailDto
                    {
                        ItemId = pos.ItemId,
                        Name = item?.Name ?? "Неизвестный товар",
                        Quantity = pos.Quantity
                    });
                }

                availableOrders.Add(dto);
            }

            // 4. Сортировка: приоритетные вперед, затем по дате создания
            var sortedOrders = availableOrders
                .OrderByDescending(o => o.IsHighPriority)
                .ThenBy(o => o.CreatedAt)
                .ToList();

            _logger.LogInformation("Итого обработано заказов: {TotalCount}", sortedOrders.Count);

            return sortedOrders;
        }
    }

}
