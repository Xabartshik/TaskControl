using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.DTOs.InventorizationDTOs;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;
using TaskControl.TaskModule.Domain;
using TaskControl.TaskModule.Presentation.Interface;

//TODO: Вынести методы перевода в Dto и обратно, пересмотреть 

namespace TaskControl.TaskModule.Application.Services
{
    /// <summary>
    /// Реализация основного сервиса обработки инвентаризации
    /// </summary>
    public class InventoryProcessService : IInventoryProcessService
    {
        private readonly IInventoryAssignmentRepository _assignmentRepository;
        private readonly IInventoryAssignmentLineRepository _lineRepository;
        private readonly IInventoryDiscrepancyRepository _discrepancyRepository;
        private readonly IInventoryStatisticsRepository _statisticsRepository;
        private readonly ILogger<InventoryProcessService> _logger;

        public InventoryProcessService(
            IInventoryAssignmentRepository assignmentRepository,
            IInventoryAssignmentLineRepository lineRepository,
            IInventoryDiscrepancyRepository discrepancyRepository,
            IInventoryStatisticsRepository statisticsRepository,
            ILogger<InventoryProcessService> logger)
        {
            _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
            _lineRepository = lineRepository ?? throw new ArgumentNullException(nameof(lineRepository));
            _discrepancyRepository = discrepancyRepository ?? throw new ArgumentNullException(nameof(discrepancyRepository));
            _statisticsRepository = statisticsRepository ?? throw new ArgumentNullException(nameof(statisticsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //TODO: Поработать над этой частью
        /// <summary>
        /// Создать новую инвентаризацию и распределить товары между работниками
        /// </summary>
        public async Task<CompleteInventoryDto> CreateAndDistributeInventoryAsync(
            CreateInventoryTaskDto dto,
            List<int> availableWorkers)
        {
            throw new NotImplementedException();
            //try
            //{
            //    _logger.LogInformation(
            //        "Создание инвентаризации: филиал {BranchId}, приоритет {Priority}, товаров {ItemCount}, работников {WorkerCount}",
            //        dto.BranchId, dto.Priority, dto.ItemPositionIds.Count, dto.WorkerCount);

            //    if (dto.ItemPositionIds.Count == 0)
            //        throw new ArgumentException("Список товаров не может быть пустым");

            //    if (availableWorkers.Count == 0)
            //        throw new ArgumentException("Нет доступных работников для распределения");

            //    var workerCount = Math.Min(dto.WorkerCount, availableWorkers.Count);

            //    // Разделить товары на зоны в зависимости от стратегии
            //    var zones = DivideItemsByStrategy(dto.ItemPositionIds, workerCount, dto.DivisionStrategy);

            //    _logger.LogInformation("Товары разделены на {ZoneCount} зон", zones.Count);

            //    // Создать BaseTask
            //    var taskId = 1; // TODO: Интегрировать с IRepository<BaseTask>

            //    var assignments = new List<InventoryAssignment>();

            //    // Создать InventoryAssignment для каждого работника
            //    for (int i = 0; i < zones.Count && i < availableWorkers.Count; i++)
            //    {
            //        var zoneCode = GetZoneCode(i);
            //        var assignment = new InventoryAssignment(
            //            taskId: taskId,
            //            assignedToUserId: availableWorkers[i],
            //            branchId: dto.BranchId,
            //            zoneCode: zoneCode);

            //        assignments.Add(assignment);
            //        _logger.LogInformation(
            //            "Назначение создано: работник {UserId}, зона {Zone}",
            //            availableWorkers[i], zoneCode);
            //    }

            //    // Сохранить назначения
            //    var assignmentIds = new List<int>();
            //    foreach (var assignment in assignments)
            //    {
            //        var id = await _assignmentRepository.AddAsync(assignment);
            //        assignmentIds.Add(id);
            //    }

            //    _logger.LogInformation("Назначения сохранены: {Count}", assignmentIds.Count);

            //    // Создать Lines и Statistics для каждого назначения
            //    for (int i = 0; i < assignmentIds.Count; i++)
            //    {
            //        var assignmentId = assignmentIds[i];
            //        var zoneItems = zones[i];

            //        // Создать Lines
            //        var lines = new List<InventoryAssignmentLine>();
            //        foreach (var itemId in zoneItems)
            //        {
            //            var line = new InventoryAssignmentLine(
            //                inventoryAssignmentId: assignmentId,
            //                itemPositionId: itemId);
            //            lines.Add(line);
            //        }

            //        // Массовое добавление Lines
            //        await _lineRepository.AddBatchAsync(lines);
            //        _logger.LogInformation("Добавлено {Count} строк для назначения {AssignmentId}", lines.Count, assignmentId);

            //        // Создать Statistics
            //        var statistics = new InventoryStatistics(
            //            inventoryAssignmentId: assignmentId,
            //            totalPositions: lines.Count);

            //        await _statisticsRepository.AddAsync(statistics);
            //        _logger.LogInformation("Статистика создана для назначения {AssignmentId}", assignmentId);
            //    }

            //    _logger.LogInformation(
            //        "Инвентаризация успешно создана и распределена между {WorkerCount} работниками",
            //        assignmentIds.Count);

            //    return new CompleteInventoryDto
            //    {
            //        InventoryAssignmentId = assignmentIds.FirstOrDefault(),
            //        CompletedAt = DateTime.UtcNow,
            //        Message = $"Инвентаризация создана и распределена между {assignmentIds.Count} работниками"
            //    };
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Ошибка при создании инвентаризации");
            //    throw;
            //}
        }

        /// <summary>
        /// Получить текущий прогресс выполнения инвентаризации
        /// </summary>
        public async Task<GetInventoryProgressDto> GetInventoryProgressAsync(
            int assignmentId)
        {
            try
            {
                _logger.LogInformation("Получение прогресса инвентаризации {AssignmentId}", assignmentId);

                var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                if (assignment == null)
                    throw new InvalidOperationException($"Назначение {assignmentId} не найдено");

                var statistics = await _statisticsRepository.GetByAssignmentIdAsync(assignmentId);
                if (statistics == null)
                    throw new InvalidOperationException($"Статистика для назначения {assignmentId} не найдена");

                var uncountedItems = await _lineRepository.GetUncountedAsync(assignmentId);

                var statisticsDto = MapStatisticsToDto(statistics);

                return new GetInventoryProgressDto
                {
                    AssignmentId = assignmentId,
                    CurrentStatistics = statisticsDto,
                    RemainingItems = uncountedItems.Select(MapLineToDto).ToList(),
                    Status = (InventoryAssignmentStatus)assignment.Status,
                    TimeSpentMinutes = CalculateTimeSpent(statistics.StartedAt),
                    EstimatedTimeRemainingMinutes = EstimateRemainingTime(statistics, uncountedItems.Count)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении прогресса инвентаризации {AssignmentId}", assignmentId);
                throw;
            }
        }

        /// <summary>
        /// Обработать сканирование товара при инвентаризации
        /// </summary>
        public async Task<InventoryStatisticsDto> ProcessInventoryScanAsync(
            ProcessInventoryScanDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Обработка сканирования: назначение {AssignmentId}, строка {LineId}, количество {ActualQuantity}",
                    dto.AssignmentId, dto.LineId, dto.ActualQuantity);

                // Получить Line
                var line = await _lineRepository.GetByIdAsync(dto.LineId);
                if (line == null)
                    throw new InvalidOperationException($"Строка {dto.LineId} не найдена");

                if (line.InventoryAssignmentId != dto.AssignmentId)
                    throw new InvalidOperationException("Строка не принадлежит указанному назначению");

                // Обновить ActualQuantity
                line.ActualQuantity = dto.ActualQuantity;
                await _lineRepository.UpdateAsync(line);

                // Проверить на расхождение
                var variance = dto.ActualQuantity - line.ExpectedQuantity;
                if (variance != 0)
                {
                    var discrepancyType = variance > 0 ? DiscrepancyType.Surplus : DiscrepancyType.Shortage;
                    var discrepancy = new InventoryDiscrepancy(
                        inventoryAssignmentLineId: dto.LineId,
                        itemPositionId: line.ItemPositionId,
                        expectedQuantity: line.ExpectedQuantity,
                        actualQuantity: dto.ActualQuantity,
                        note: dto.Note);

                    await _discrepancyRepository.AddAsync(discrepancy);
                    _logger.LogInformation(
                        "Расхождение создано: тип {Type}, дисперсия {Variance}",
                        discrepancyType, variance);
                }

                // Получить и обновить Statistics
                var statistics = await _statisticsRepository.GetByAssignmentIdAsync(dto.AssignmentId);
                if (statistics == null)
                    throw new InvalidOperationException($"Статистика для назначения {dto.AssignmentId} не найдена");

                // Пересчитать статистику
                var allLines = await _lineRepository.GetByAssignmentIdAsync(dto.AssignmentId);
                var countedItems = allLines.Count(l => l.ActualQuantity.HasValue);
                var discrepancies = await _discrepancyRepository.GetByAssignmentIdAsync(dto.AssignmentId);

                statistics.CountedPositions = countedItems;
                statistics.DiscrepancyCount = discrepancies.Count;
                statistics.SurplusCount = discrepancies.Count(d => d.Type == DiscrepancyType.Surplus);
                statistics.ShortageCount = discrepancies.Count(d => d.Type == DiscrepancyType.Shortage);

                // Пересчитать суммарные количества
                statistics.TotalSurplusQuantity = discrepancies
                    .Where(d => d.Type == DiscrepancyType.Surplus)
                    .Sum(d => d.Variance);
                statistics.TotalShortageQuantity = discrepancies
                    .Where(d => d.Type == DiscrepancyType.Shortage)
                    .Sum(d => Math.Abs(d.Variance));

                await _statisticsRepository.UpdateAsync(statistics);

                _logger.LogInformation(
                    "Сканирование обработано: учтено {Counted}/{Total} позиций",
                    countedItems, allLines.Count);

                return MapStatisticsToDto(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке сканирования");
                throw;
            }
        }

        /// <summary>
        /// Завершить инвентаризацию и получить финальный отчёт
        /// </summary>
        public async Task<CompleteInventoryDto> CompleteInventoryAsync(
            int assignmentId)
        {
            try
            {
                _logger.LogInformation("Завершение инвентаризации {AssignmentId}", assignmentId);

                var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                if (assignment == null)
                    throw new InvalidOperationException($"Назначение {assignmentId} не найдено");

                // Отметить как Completed
                assignment.Status = InventoryAssignmentStatus.Completed;
                assignment.CompletedAt = DateTime.UtcNow;
                await _assignmentRepository.UpdateAsync(assignment);

                // Получить статистику
                var statistics = await _statisticsRepository.GetByAssignmentIdAsync(assignmentId);
                statistics!.CompletedAt = DateTime.UtcNow;
                await _statisticsRepository.UpdateAsync(statistics);

                // Получить расхождения
                var discrepancies = await _discrepancyRepository.GetByAssignmentIdAsync(assignmentId);

                var statisticsDto = MapStatisticsToDto(statistics);
                var discrepancyReport = new DiscrepancyReportDto
                {
                    InventoryAssignmentId = assignmentId,
                    TotalDiscrepancies = discrepancies.Count,
                    SurplusCount = discrepancies.Count(d => d.Type == DiscrepancyType.Surplus),
                    ShortageCount = discrepancies.Count(d => d.Type == DiscrepancyType.Shortage),
                    DiscrepancyPercentage = statistics.TotalPositions > 0
                        ? discrepancies.Count / statistics.TotalPositions * 100
                        : 0,
                    Discrepancies = discrepancies.Select(MapDiscrepancyToDto).ToList()
                };

                _logger.LogInformation(
                    "Инвентаризация завершена: назначение {AssignmentId}, найдено {DiscrepancyCount} расхождений",
                    assignmentId, discrepancies.Count);

                return new CompleteInventoryDto
                {
                    InventoryAssignmentId = assignmentId,
                    Statistics = statisticsDto,
                    DiscrepancyReport = discrepancyReport,
                    CompletedAt = DateTime.UtcNow,
                    Message = $"Инвентаризация завершена. Найдено {discrepancies.Count} расхождений"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при завершении инвентаризации {AssignmentId}", assignmentId);
                throw;
            }
        }

        /// <summary>
        /// Отменить инвентаризацию
        /// </summary>
        public async Task<bool> CancelInventoryAsync(
            int assignmentId,
            string? reason = null)
        {
            try
            {
                _logger.LogInformation("Отмена инвентаризации {AssignmentId}, причина: {Reason}", assignmentId, reason ?? "не указана");

                var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                if (assignment == null)
                    throw new InvalidOperationException($"Назначение {assignmentId} не найдено");

                assignment.Status = InventoryAssignmentStatus.Cancelled;
                assignment.CompletedAt = DateTime.UtcNow;
                await _assignmentRepository.UpdateAsync(assignment);

                _logger.LogInformation("Инвентаризация {AssignmentId} отменена", assignmentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отмене инвентаризации {AssignmentId}", assignmentId);
                throw;
            }
        }

        /// <summary>
        /// Получить все назначения инвентаризации для конкретного пользователя
        /// </summary>
        public async Task<List<InventoryAssignmentDetailedDto>> GetUserAssignmentsAsync(
            int userId)
        {
            try
            {
                _logger.LogInformation("Получение назначений для пользователя {UserId}", userId);

                var assignments = await _assignmentRepository.GetByUserIdAsync(userId);
                var result = new List<InventoryAssignmentDetailedDto>();

                foreach (var assignment in assignments)
                {
                    var statistics = await _statisticsRepository.GetByAssignmentIdAsync(assignment.Id);
                    var lines = await _lineRepository.GetByAssignmentIdAsync(assignment.Id);

                    result.Add(new InventoryAssignmentDetailedDto
                    {
                        Id = assignment.Id,
                        TaskId = assignment.TaskId,
                        AssignedToUserId = assignment.AssignedToUserId,
                        BranchId = assignment.BranchId,
                        ZoneCode = assignment.ZoneCode,
                        Status = (InventoryAssignmentStatus)assignment.Status,
                        AssignedAt = assignment.AssignedAt,
                        CompletedAt = assignment.CompletedAt,
                        Statistics = statistics != null ? MapStatisticsToDto(statistics) : null,
                        Lines = lines.Select(MapLineToDto).ToList()
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении назначений для пользователя {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Получить активные (незавершенные) инвентаризации
        /// </summary>
        public async Task<List<InventoryAssignmentDetailedDto>> GetActiveInventoriesAsync(
            int? branchId = null)
        {
            try
            {
                _logger.LogInformation("Получение активных инвентаризаций, филиал: {BranchId}", branchId);

                var activeStatus = (int)InventoryAssignmentStatus.InProgress;
                var assignments = await _assignmentRepository.GetByStatusAsync((InventoryAssignmentStatus)activeStatus);

                if (branchId.HasValue)
                    assignments = assignments.Where(a => a.BranchId == branchId.Value).ToList();

                var result = new List<InventoryAssignmentDetailedDto>();
                foreach (var assignment in assignments)
                {
                    var statistics = await _statisticsRepository.GetByAssignmentIdAsync(assignment.Id);
                    var lines = await _lineRepository.GetByAssignmentIdAsync(assignment.Id);

                    result.Add(new InventoryAssignmentDetailedDto
                    {
                        Id = assignment.Id,
                        TaskId = assignment.TaskId,
                        AssignedToUserId = assignment.AssignedToUserId,
                        BranchId = assignment.BranchId,
                        ZoneCode = assignment.ZoneCode,
                        Status = (InventoryAssignmentStatus)assignment.Status,
                        AssignedAt = assignment.AssignedAt,
                        CompletedAt = assignment.CompletedAt,
                        Statistics = statistics != null ? MapStatisticsToDto(statistics) : null,
                        Lines = lines.Select(MapLineToDto).ToList()
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении активных инвентаризаций");
                throw;
            }
        }

        /// <summary>
        /// Получить завершенные инвентаризации за период
        /// </summary>
        public async Task<List<InventoryAssignmentDetailedDto>> GetCompletedInventoriesAsync(
            DateTime startDate,
            DateTime endDate,
            int? branchId = null)
        {
            try
            {
                _logger.LogInformation(
                    "Получение завершённых инвентаризаций с {StartDate} по {EndDate}, филиал: {BranchId}",
                    startDate, endDate, branchId);

                var completedStatus = (int)InventoryAssignmentStatus.Completed;
                var assignments = await _assignmentRepository.GetByStatusAsync((InventoryAssignmentStatus)completedStatus);

                assignments = assignments
                    .Where(a => a.CompletedAt.HasValue &&
                               a.CompletedAt.Value >= startDate &&
                               a.CompletedAt.Value <= endDate)
                    .ToList();

                if (branchId.HasValue)
                    assignments = assignments.Where(a => a.BranchId == branchId.Value).ToList();

                var result = new List<InventoryAssignmentDetailedDto>();
                foreach (var assignment in assignments)
                {
                    var statistics = await _statisticsRepository.GetByAssignmentIdAsync(assignment.Id);
                    var lines = await _lineRepository.GetByAssignmentIdAsync(assignment.Id);

                    result.Add(new InventoryAssignmentDetailedDto
                    {
                        Id = assignment.Id,
                        TaskId = assignment.TaskId,
                        AssignedToUserId = assignment.AssignedToUserId,
                        BranchId = assignment.BranchId,
                        ZoneCode = assignment.ZoneCode,
                        Status = (InventoryAssignmentStatus)assignment.Status,
                        AssignedAt = assignment.AssignedAt,
                        CompletedAt = assignment.CompletedAt,
                        Statistics = statistics != null ? MapStatisticsToDto(statistics) : null,
                        Lines = lines.Select(MapLineToDto).ToList()
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении завершённых инвентаризаций");
                throw;
            }
        }

        /// <summary>
        /// Переназначить инвентаризацию другому работнику
        /// </summary>
        public async Task<bool> ReassignInventoryAsync(
            int assignmentId,
            int newUserId,
            string? reason = null
            )
        {
            try
            {
                _logger.LogInformation(
                    "Переназначение инвентаризации {AssignmentId} новому пользователю {UserId}, причина: {Reason}",
                    assignmentId, newUserId, reason ?? "не указана");

                var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                if (assignment == null)
                    throw new InvalidOperationException($"Назначение {assignmentId} не найдено");

                assignment.AssignedToUserId = newUserId;
                await _assignmentRepository.UpdateAsync(assignment);

                _logger.LogInformation("Инвентаризация {AssignmentId} переназначена пользователю {UserId}", assignmentId, newUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при переназначении инвентаризации");
                throw;
            }
        }

        /// <summary>
        /// Возобновить незавершённую инвентаризацию
        /// </summary>
        public async Task<GetInventoryProgressDto> ResumeInventoryAsync(
            int assignmentId)
        {
            try
            {
                _logger.LogInformation("Возобновление инвентаризации {AssignmentId}", assignmentId);

                var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
                if (assignment == null)
                    throw new InvalidOperationException($"Назначение {assignmentId} не найдено");

                assignment.Status = InventoryAssignmentStatus.InProgress;
                await _assignmentRepository.UpdateAsync(assignment);

                return await GetInventoryProgressAsync(assignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при возобновлении инвентаризации {AssignmentId}", assignmentId);
                throw;
            }
        }

        /// <summary>
        /// Получить статистику инвентаризации по конкретному назначению
        /// </summary>
        public async Task<InventoryStatisticsDto> GetStatisticsAsync(
            int assignmentId)
        {
            try
            {
                var statistics = await _statisticsRepository.GetByAssignmentIdAsync(assignmentId);
                if (statistics == null)
                    throw new InvalidOperationException($"Статистика для назначения {assignmentId} не найдена");

                return MapStatisticsToDto(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики {AssignmentId}", assignmentId);
                throw;
            }
        }

        /// <summary>
        /// Отменить сканирование (удалить последнее сканированное значение)
        /// </summary>
        public async Task<bool> UndoScanAsync(
            int lineId)
        {
            try
            {
                _logger.LogInformation("Отмена сканирования для строки {LineId}", lineId);

                var line = await _lineRepository.GetByIdAsync(lineId);
                if (line == null)
                    throw new InvalidOperationException($"Строка {lineId} не найдена");

                // Удалить расхождение если оно есть
                var discrepancies = await _discrepancyRepository.GetByAssignmentLineIdAsync(lineId);
                foreach (var discrepancy in discrepancies)
                {
                    await _discrepancyRepository.DeleteAsync(discrepancy.Id);
                }

                // Очистить ActualQuantity
                line.ActualQuantity = null;
                await _lineRepository.UpdateAsync(line);

                // Обновить статистику
                var statistics = await _statisticsRepository.GetByAssignmentIdAsync(line.InventoryAssignmentId);
                if (statistics != null)
                {
                    var allLines = await _lineRepository.GetByAssignmentIdAsync(line.InventoryAssignmentId);
                    statistics.CountedPositions = allLines.Count(l => l.ActualQuantity.HasValue);
                    statistics.DiscrepancyCount = await _discrepancyRepository.GetCountByAssignmentIdAsync(line.InventoryAssignmentId);

                    var allDiscrepancies = await _discrepancyRepository.GetByAssignmentIdAsync(line.InventoryAssignmentId);
                    statistics.SurplusCount = allDiscrepancies.Count(d => d.Type == DiscrepancyType.Surplus);
                    statistics.ShortageCount = allDiscrepancies.Count(d => d.Type == DiscrepancyType.Shortage);

                    await _statisticsRepository.UpdateAsync(statistics);
                }

                _logger.LogInformation("Сканирование для строки {LineId} отменено", lineId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отмене сканирования {LineId}", lineId);
                throw;
            }
        }

        /// <summary>
        /// Получить список товаров которые ещё не отсчитаны в назначении
        /// </summary>
        public async Task<List<InventoryAssignmentLineDto>> GetUncountedItemsAsync(
            int assignmentId)
        {
            try
            {
                var uncounted = await _lineRepository.GetUncountedAsync(assignmentId);
                return uncounted.Select(MapLineToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении неотсчитанных товаров {AssignmentId}", assignmentId);
                throw;
            }
        }

        /// <summary>
        /// Получить рекомендации по ускорению процесса
        /// </summary>
        public async Task<List<string>> GetOptimizationRecommendationsAsync(
            int assignmentId)
        {
            var recommendations = new List<string>();

            try
            {
                var statistics = await _statisticsRepository.GetByAssignmentIdAsync(assignmentId);
                if (statistics == null)
                    return recommendations;

                var completionPercentage = statistics.CompletionPercentage;

                if (completionPercentage < 25)
                    recommendations.Add("Низкий прогресс. Рекомендуется проверить наличие товаров и их доступность.");

                if (statistics.DiscrepancyCount > statistics.TotalPositions * 0.1m)
                    recommendations.Add("Высокий процент расхождений. Проверьте точность сканирования и маркировки товаров.");

                if (statistics.SurplusCount > statistics.ShortageCount)
                    recommendations.Add("Больше излишков, чем недостач. Проверьте систему хранения и учета.");

                if (statistics.CountedPositions > 0)
                {
                    var timePerItem = (DateTime.UtcNow - statistics.StartedAt).TotalMinutes / statistics.CountedPositions;
                    if (timePerItem > 5)
                        recommendations.Add($"Медленный темп (более {timePerItem:F1} мин на товар). Оптимизируйте маршрут.");
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении рекомендаций {AssignmentId}", assignmentId);
                return recommendations;
            }
        }

        /// <summary>
        /// Валидировать сканирование перед применением
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> ValidateScanAsync(
            ProcessInventoryScanDto dto)
        {
            try
            {
                if (dto.AssignmentId <= 0)
                    return (false, "ID назначения должен быть больше 0");

                if (dto.LineId <= 0)
                    return (false, "ID строки должен быть больше 0");

                if (dto.ActualQuantity < 0)
                    return (false, "Количество не может быть отрицательным");

                var line = await _lineRepository.GetByIdAsync(dto.LineId);
                if (line == null)
                    return (false, $"Строка {dto.LineId} не найдена");

                if (line.InventoryAssignmentId != dto.AssignmentId)
                    return (false, "Строка не принадлежит указанному назначению");

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при валидации сканирования");
                return (false, "Ошибка при валидации сканирования");
            }
        }

        /// <summary>
        /// Синхронизировать данные инвентаризации
        /// </summary>
        public async Task<InventoryStatisticsDto> SyncInventoryDataAsync(
            int assignmentId)
        {
            try
            {
                _logger.LogInformation("Синхронизация данных инвентаризации {AssignmentId}", assignmentId);

                var statistics = await _statisticsRepository.GetByAssignmentIdAsync(assignmentId);
                if (statistics == null)
                    throw new InvalidOperationException($"Статистика для назначения {assignmentId} не найдена");

                var allLines = await _lineRepository.GetByAssignmentIdAsync(assignmentId);
                var allDiscrepancies = await _discrepancyRepository.GetByAssignmentIdAsync(assignmentId);

                // Пересчитать все метрики
                statistics.CountedPositions = allLines.Count(l => l.ActualQuantity.HasValue);
                statistics.DiscrepancyCount = allDiscrepancies.Count;
                statistics.SurplusCount = allDiscrepancies.Count(d => d.Type == DiscrepancyType.Surplus);
                statistics.ShortageCount = allDiscrepancies.Count(d => d.Type == DiscrepancyType.Shortage);
                statistics.TotalSurplusQuantity = allDiscrepancies.Where(d => d.Type == DiscrepancyType.Surplus).Sum(d => d.Variance);
                statistics.TotalShortageQuantity = allDiscrepancies.Where(d => d.Type == DiscrepancyType.Shortage).Sum(d => Math.Abs(d.Variance));

                await _statisticsRepository.UpdateAsync(statistics);

                _logger.LogInformation("Синхронизация завершена для {AssignmentId}", assignmentId);
                return MapStatisticsToDto(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при синхронизации данных {AssignmentId}", assignmentId);
                throw;
            }
        }

        /// <summary>
        /// Добавить примечание к инвентаризации
        /// </summary>
        public async Task<bool> AddNoteAsync(
            int assignmentId,
            string note)
        {
            try
            {
                _logger.LogInformation("Добавление примечания к назначению {AssignmentId}", assignmentId);

                if (string.IsNullOrWhiteSpace(note))
                    throw new ArgumentException("Примечание не может быть пустым");

                // TODO: Сохранить примечание в БД (требуется добавить поле Note в InventoryAssignment или отдельную таблицу)
                _logger.LogInformation("Примечание добавлено к назначению {AssignmentId}", assignmentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении примечания {AssignmentId}", assignmentId);
                throw;
            }
        }

        /// <summary>
        /// Получить метрики производительности для инвентаризации
        /// </summary>
        public async Task<Dictionary<string, object>> GetPerformanceMetricsAsync(
            int assignmentId
            )
        {
            try
            {
                var statistics = await _statisticsRepository.GetByAssignmentIdAsync(assignmentId);
                if (statistics == null)
                    throw new InvalidOperationException($"Статистика для назначения {assignmentId} не найдена");

                var duration = (DateTime.UtcNow - statistics.StartedAt).TotalMinutes;
                var itemsPerHour = statistics.CountedPositions > 0 ? (60 / duration) * statistics.CountedPositions : 0;

                return new Dictionary<string, object>
            {
                { "totalPositions", statistics.TotalPositions },
                { "countedPositions", statistics.CountedPositions },
                { "completionPercentage", statistics.CompletionPercentage },
                { "discrepancyCount", statistics.DiscrepancyCount },
                { "discrepancyPercentage", statistics.TotalPositions > 0 ? (decimal)statistics.DiscrepancyCount / statistics.TotalPositions * 100 : 0 },
                { "durationMinutes", Math.Round(duration, 2) },
                { "itemsPerHour", Math.Round(itemsPerHour, 2) },
                { "accuracy", 100 - (statistics.TotalPositions > 0 ? (decimal)statistics.DiscrepancyCount / statistics.TotalPositions * 100 : 0) }
            };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении метрик производительности {AssignmentId}", assignmentId);
                throw;
            }
        }

        /// <summary>
        /// Экспортировать результаты инвентаризации
        /// </summary>
        public async Task<Stream> ExportResultsAsync(
            int assignmentId,
            ExportFormat format
            )
        {
            try
            {
                _logger.LogInformation("Экспорт результатов инвентаризации {AssignmentId} в формате {Format}", assignmentId, format);

                // TODO: Реализовать экспорт в различные форматы
                throw new NotImplementedException("Экспорт пока не реализован");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при экспорте результатов {AssignmentId}", assignmentId);
                throw;
            }
        }

        /// <summary>
        /// Разделить товары по стратегии распределения
        /// </summary>
        private List<List<int>> DivideItemsByStrategy(
            List<int> itemIds,
            int workerCount,
            DivisionStrategy strategy)
        {
            var zones = new List<List<int>>();

            return strategy switch
            {
                DivisionStrategy.ByQuantity => DivideByQuantity(itemIds, workerCount),
                DivisionStrategy.ByZone => DivideByZone(itemIds, workerCount),
                DivisionStrategy.ByDistance => DivideByDistance(itemIds, workerCount),
                _ => DivideByQuantity(itemIds, workerCount)
            };
        }

        private List<List<int>> DivideByQuantity(List<int> itemIds, int workerCount)
        {
            var zones = new List<List<int>>();
            var itemsPerWorker = (int)Math.Ceiling((double)itemIds.Count / workerCount);

            for (int i = 0; i < workerCount; i++)
            {
                var start = i * itemsPerWorker;
                var end = Math.Min(start + itemsPerWorker, itemIds.Count);
                zones.Add(itemIds.GetRange(start, end - start));
            }

            return zones;
        }

        private List<List<int>> DivideByZone(List<int> itemIds, int workerCount)
        {
            // TODO: Реализовать распределение по существующим зонам
            return DivideByQuantity(itemIds, workerCount);
        }

        private List<List<int>> DivideByDistance(List<int> itemIds, int workerCount)
        {
            // TODO: Реализовать оптимизацию по маршруту расстояния
            return DivideByQuantity(itemIds, workerCount);
        }

        private double CalculateTimeSpent(DateTime startedAt) => (DateTime.UtcNow - startedAt).TotalMinutes;

        private double EstimateRemainingTime(InventoryStatistics stats, int remainingItems)
        {
            if (stats.CountedPositions == 0) return 0;

            var timePerItem = (DateTime.UtcNow - stats.StartedAt).TotalMinutes / stats.CountedPositions;
            return remainingItems * timePerItem;
        }

        //TODO: Вынести в файлы к самим Dto.
        private InventoryStatisticsDto MapStatisticsToDto(InventoryStatistics stats) =>
            new()
            {
                Id = stats.Id,
                InventoryAssignmentId = stats.InventoryAssignmentId,
                TotalPositions = stats.TotalPositions,
                CountedPositions = stats.CountedPositions,
                CompletionPercentage = stats.CompletionPercentage,
                DiscrepancyCount = stats.DiscrepancyCount,
                SurplusCount = stats.SurplusCount,
                ShortageCount = stats.ShortageCount,
                TotalSurplusQuantity = stats.TotalSurplusQuantity,
                TotalShortageQuantity = stats.TotalShortageQuantity,
                StartedAt = stats.StartedAt,
                CompletedAt = stats.CompletedAt
            };

        private InventoryAssignmentLineDto MapLineToDto(InventoryAssignmentLine line) =>
            new()
            {
                //Id = line.Id,
                //InventoryAssignmentId = line.InventoryAssignmentId,
                //ItemPositionId = line.ItemPositionId,
                //ExpectedQuantity = line.ExpectedQuantity,
                //ActualQuantity = line.ActualQuantity,
                //ZoneCode = line.ZoneCode,
                //FirstLevelStorageType = line.FirstLevelStorageType,
                //FlsNumber = line.FlsNumber
            };

        private DiscrepancyDto MapDiscrepancyToDto(InventoryDiscrepancy d) =>
            new()
            {
                Id = d.Id,
                InventoryAssignmentLineId = d.InventoryAssignmentLineId,
                ItemPositionId = d.ItemPositionId,
                ExpectedQuantity = d.ExpectedQuantity,
                ActualQuantity = d.ActualQuantity,
                Variance = d.Variance,
                Type = d.Type,
                Note = d.Note,
                IdentifiedAt = d.IdentifiedAt,
                ResolutionStatus = d.ResolutionStatus
            };
    }

}
