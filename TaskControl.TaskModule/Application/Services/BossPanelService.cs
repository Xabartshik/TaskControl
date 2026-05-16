using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Model;
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
using TaskControl.TaskModule.DataAccess.Models;
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
        private readonly ITaskDataConnection _db;

        public BossPanelService(
            ITaskDataConnection db,
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
            _db = db ?? throw new ArgumentNullException(nameof(db));
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

        public async Task<IEnumerable<AvailableEmployeeDto>> GetAvailableCouriersAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение доступных курьеров для филиала {BossBranchId}", bossBranchId);

            var employees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
            var result = new List<AvailableEmployeeDto>();

            var courierCapabilities = await _db.GetTable<CourierCapabilityModel>().ToListAsync();

            var employeeIds = employees.Select(e => e.EmployeeId).ToList();
            var today = DateTime.UtcNow.Date;
            var recentChecks = await _db.GetTable<CheckIOEmployeeModel>()
                .Where(c => employeeIds.Contains(c.EmployeeId) && c.CheckTimeStamp >= today)
                .ToListAsync();

            foreach (var emp in employees)
            {
                var capability = courierCapabilities.FirstOrDefault(c => c.EmployeeId == emp.EmployeeId);

                if (capability != null || emp.Role == "Курьер" || emp.Role == "Courier")
                {
                    var latestCheck = recentChecks
                        .Where(c => c.EmployeeId == emp.EmployeeId)
                        .OrderByDescending(c => c.CheckTimeStamp)
                        .FirstOrDefault();

                    bool isOnRoute = latestCheck != null && latestCheck.CheckType == "dispatch";

                    result.Add(new AvailableEmployeeDto
                    {
                        EmployeeId = emp.EmployeeId,
                        FullName = $"{emp.Surname} {emp.Name}",
                        IsAtWork = true,
                        ActiveTasksCount = await _aggregator.GetTotalActiveWorkloadAsync(emp.EmployeeId),
                        IsRecommended = false,

                        MaxWeightKg = capability != null ? capability.MaxWeightGrams / 1000.0 : 0.0,

                        VehicleName = capability != null
                            ? (capability.VehicleTypeId switch
                            {
                                1 => "Пешком",
                                2 => "Велосипед",
                                3 => "Легковое авто",
                                4 => "Фургон",
                                5 => "Грузовик",
                                _ => $"Неизвестно (ID: {capability.VehicleTypeId})"
                            })
                            : "Пеший / Не указан",

                        IsOnRoute = isOnRoute
                    });
                }
            }
            return result;
        }

        public async Task<IEnumerable<AvailableOrderDto>> GetReadyForDispatchOrdersAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение готовых заказов для курьеров филиала {BossBranchId}", bossBranchId);

            var allOrders = await _orderRepository.GetByBranchAsync(bossBranchId);

            var readyOrders = allOrders.Where(o =>
                o.Status == OrderStatus.Ready &&
                o.DeliveryType == DeliveryType.Delivery).ToList();

            var result = new List<AvailableOrderDto>();

            foreach (var o in readyOrders)
            {
                var activeAssignments = await _db.GetTable<OrderHandoverAssignmentModel>()
                    .Where(a => a.OrderId == o.OrderId && a.Status != 3)
                    .ToListAsync();

                // Если есть хотя бы одна активная задача на передачу, заказ курьеру уже назначен
                if (activeAssignments.Any())
                {
                    continue; // Пропускаем этот заказ, он не должен висеть в свободных
                }
                // --------------------------------

                var dto = new AvailableOrderDto
                {
                    OrderId = o.OrderId,
                    OrderNumber = $"ORD-{o.OrderId}",
                    CreatedAt = o.CreatedAt,
                    Status = o.Status.ToString(),
                    DeliveryType = o.DeliveryType.ToString(),
                    DeliveryDate = o.DeliveryDate,
                    DestinationAddress = o.DestinationAddress
                };

                var positions = await _orderPositionRepository.GetByOrderIdAsync(o.OrderId);
                foreach (var pos in positions)
                {
                    var item = await _itemRepository.GetByIdAsync(pos.ItemId);
                    dto.Items.Add(new OrderItemDetailDto
                    {
                        ItemId = pos.ItemId,
                        Name = item?.Name ?? "Неизвестный товар",
                        Quantity = pos.Quantity,
                        WeightKg = item?.Weight.Kilograms ?? 0.0
                    });
                }

                result.Add(dto);
            }

            return result;
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

            var targetPositionIds = cells
                .Where(c => dto.ZonePrefixes.Any(prefix => c.Code.ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                .Select(c => c.PositionId)
                .ToList();

            if (!targetPositionIds.Any())
            {
                throw new InvalidOperationException("Не найдено ни одной позиции по указанным зонам.");
            }

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
            _logger.LogInformation("Получение активных задач для филиала {BossBranchId} через агрегатор", bossBranchId);

            var tasks = await _activeTaskRepository.GetByBranchAsync(bossBranchId);
            var activeTasks = tasks.Where(t => t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled);
            var result = new List<BossPanelTaskCardDto>();

            foreach (var t in activeTasks)
            {
                var dict = new Dictionary<int, TaskAssigneeProgressDto>();
                if (t.Status == TaskStatus.Completed)
                    continue;
                // 1. Спрашиваем агрегатор: кто работает над этой задачей? (Поддерживает ВСЕ типы задач)
                var assignedIds = await _aggregator.GetAssignedEmployeeIdsAsync(t.TaskId);

                foreach (var empId in assignedIds)
                {
                    // 2. Получаем детали конкретного назначения через агрегатор
                    var details = await _aggregator.GetTaskDetailsAsync(t.TaskId, empId);
                    var emp = await _activeEmployeeService.GetEmployeeByIdAsync(empId);

                    int assignedVolume = 1;
                    int completedVolume = 0;
                    string statusStr = "Назначено";

                    if (details != null)
                    {
                        statusStr = details.AssignmentStatus switch
                        {
                            Domain.AssignmentStatus.InProgress => "В процессе",
                            Domain.AssignmentStatus.Completed => "Завершено",
                            Domain.AssignmentStatus.Paused => "На паузе",
                            Domain.AssignmentStatus.Cancelled => "Отменена",
                            _ => "Назначена"
                        };

                        // 3. Динамически вытягиваем прогресс из TaskDetails (так как структура зависит от типа задачи)
                        if (details.TaskDetails != null)
                        {
                            try
                            {
                                var json = System.Text.Json.JsonSerializer.Serialize(details.TaskDetails);
                                using var doc = System.Text.Json.JsonDocument.Parse(json);
                                var root = doc.RootElement;

                                if (root.TryGetProperty("totalLines", out var tLines) || root.TryGetProperty("TotalLines", out tLines))
                                    if (tLines.ValueKind == System.Text.Json.JsonValueKind.Number) assignedVolume = tLines.GetInt32();

                                if (root.TryGetProperty("completedLinesCount", out var cLines) || root.TryGetProperty("CompletedLinesCount", out cLines))
                                    if (cLines.ValueKind == System.Text.Json.JsonValueKind.Number) completedVolume = cLines.GetInt32();
                            }
                            catch
                            {
                                // Игнорируем ошибки парсинга, оставляем значения по умолчанию (1 и 0)
                            }
                        }

                        // Защита: если статус завершен, прогресс 100%
                        if (details.AssignmentStatus == Domain.AssignmentStatus.Completed)
                        {
                            completedVolume = assignedVolume;
                        }
                    }

                    dict[empId] = new TaskAssigneeProgressDto
                    {
                        EmployeeId = empId,
                        FullName = emp != null ? $"{emp.Surname} {emp.Name}" : $"Работник {empId}",
                        AssignedVolume = assignedVolume,
                        CompletedVolume = completedVolume,
                        Status = statusStr
                    };
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
            _logger.LogInformation("Получение загруженности сотрудников филиала {BossBranchId} через агрегатор", bossBranchId);

            // Агрегатор сам соберет все задачи (Инвентаризация, Выдача, Сборка, Возврат)
            return await _aggregator.GetBranchWorkloadAsync(bossBranchId);
        }

        public async Task<IEnumerable<AvailableEmployeeDto>> GetAllBranchEmployeesAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение всех сотрудников филиала {BossBranchId} через фильтрацию последних чекинов", bossBranchId);

            // 1. Получаем только те записи, которые являются последними для конкретного сотрудника
            // и при этом относятся к вашему филиалу.
            var lastChecks = await _db.GetTable<CheckIOEmployeeModel>()
                .Where(c => c.CheckTimeStamp == _db.GetTable<CheckIOEmployeeModel>()
                    .Where(sub => sub.EmployeeId == c.EmployeeId)
                    .Max(sub => sub.CheckTimeStamp))
                .Where(c => c.BranchId == bossBranchId)
                .ToListAsync();

            var result = new List<AvailableEmployeeDto>();

            foreach (var check in lastChecks)
            {
                var emp = await _activeEmployeeService.GetEmployeeByIdAsync(check.EmployeeId);
                if (emp == null) continue;

                result.Add(new AvailableEmployeeDto
                {
                    EmployeeId = emp.EmployeesId,
                    FullName = $"{emp.Surname} {emp.Name}",
                    // Если последний тип записи не 'check-out' — значит он на месте или на маршруте
                    IsAtWork = check.CheckType != "check-out",
                    ActiveTasksCount = await _aggregator.GetTotalActiveWorkloadAsync(emp.EmployeesId),
                    IsRecommended = false
                });
            }

            return result.OrderBy(e => e.FullName);
        }
        public async Task<IEnumerable<AvailableEmployeeDto>> GetAvailableEmployeesAsync(int bossBranchId)
        {
            _logger.LogInformation("Получение доступных сотрудников филиала {BossBranchId}", bossBranchId);
            var employees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(bossBranchId);
            var result = new List<AvailableEmployeeDto>();

            foreach (var emp in employees)
            {
                result.Add(new AvailableEmployeeDto
                {
                    EmployeeId = emp.EmployeeId,
                    FullName = $"{emp.Surname} {emp.Name}",
                    IsAtWork = true,
                    ActiveTasksCount = await _aggregator.GetTotalActiveWorkloadAsync(emp.EmployeeId),
                    IsRecommended = false
                });
            }

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

            var allOrders = (await _orderRepository.GetByBranchAsync(bossBranchId)).ToList();
            var newOrders = allOrders.Where(o => o.Status == OrderStatus.Created).ToList();

            _logger.LogInformation("Найдено заказов в филиале: {Total}, из них в статусе Created: {NewCount}", allOrders.Count, newOrders.Count);

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
                    DeliveryDate = o.DeliveryDate,
                    PaymentType = o.PaymentType.ToString(),
                    DestinationAddress = o.DestinationAddress
                };

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
                    DeliveryDate = o.DeliveryDate,
                    PaymentType = o.PaymentType.ToString(),
                    DestinationAddress = o.DestinationAddress
                };

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

            var sortedOrders = availableOrders
                .OrderByDescending(o => o.IsHighPriority)
                .ThenBy(o => o.CreatedAt)
                .ToList();

            _logger.LogInformation("Итого обработано заказов: {TotalCount}", sortedOrders.Count);

            return sortedOrders;
        }
    }
}