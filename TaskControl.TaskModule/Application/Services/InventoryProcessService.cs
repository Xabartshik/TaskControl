using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InventoryModule.Application.Services;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.Domain;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Application.Services
{
    public class InventoryProcessService : IInventoryProcessService
    {
        private readonly IInventoryAssignmentRepository _assignmentRepository;
        private readonly IInventoryAssignmentLineRepository _lineRepository;
        private readonly IInventoryDiscrepancyRepository _discrepancyRepository;
        private readonly IInventoryStatisticsRepository _statisticsRepository;
        private readonly IBaseTaskService _baseTaskService;
        private readonly IItemPositionRepository _itemPositionRepository;
        private readonly PositionDetailsService _positionDetailsService;
        private readonly IItemRepository _itemRepository;
        private readonly ITelemetryService _telemetryService;
        private readonly TaskWorkloadAggregator _aggregator;
        private readonly ActiveEmployeeService _activeEmployeeService;
        private readonly ILogger<InventoryProcessService> _logger;

        public InventoryProcessService(
            IInventoryAssignmentRepository assignmentRepository,
            IInventoryAssignmentLineRepository lineRepository,
            IInventoryDiscrepancyRepository discrepancyRepository,
            IInventoryStatisticsRepository statisticsRepository,
            IBaseTaskService baseTaskService,
            IItemPositionRepository itemPositionRepository,
            PositionDetailsService positionDetailsService,
            IItemRepository itemRepository,
            ITelemetryService telemetryService,
            TaskWorkloadAggregator aggregator,
            ActiveEmployeeService activeEmployeeService,
            ILogger<InventoryProcessService> logger)
        {
            _assignmentRepository = assignmentRepository;
            _lineRepository = lineRepository;
            _discrepancyRepository = discrepancyRepository;
            _statisticsRepository = statisticsRepository;
            _baseTaskService = baseTaskService;
            _itemPositionRepository = itemPositionRepository;
            _positionDetailsService = positionDetailsService;
            _itemRepository = itemRepository;
            _telemetryService = telemetryService;
            _aggregator = aggregator;
            _activeEmployeeService = activeEmployeeService;
            _logger = logger;
        }

        public async Task<CompleteInventoryDto> CreateAndDistributeInventoryAsync(CreateInventoryTaskDto dto, List<int> availableWorkers)
        {
            List<int> targetWorkerIds = availableWorkers ?? new List<int>();

            // Если список работников не передан, выбираем автоматически
            if (!targetWorkerIds.Any())
            {
                var activeEmployees = await _activeEmployeeService.GetWorkingEmployeesByBranchAsync(dto.BranchId);
                var activeList = activeEmployees.ToList();

                if (!activeList.Any())
                    throw new InvalidOperationException("Нет активных сотрудников в филиале для автоматического назначения.");

                // Берем нужное количество (WorkerCount) самых свободных
                int countToTake = dto.WorkerCount > 0 ? Math.Min(dto.WorkerCount, activeList.Count) : 1;

                var loadTasks = activeList.Select(async e => new { id = e.EmployeeId, load = await _aggregator.GetTotalActiveWorkloadAsync(e.EmployeeId) });
                var results = await Task.WhenAll(loadTasks);

                targetWorkerIds = results.OrderBy(x => x.load).Take(countToTake).Select(x => x.id).ToList();
                _logger.LogInformation("Автоматически выбрано {Count} исполнителей.", targetWorkerIds.Count);
            }

            var taskId = await _baseTaskService.Add(new BaseTaskDto
            {
                Title = $"Инвентаризация {DateTime.UtcNow:dd.MM HH:mm}",
                BranchId = dto.BranchId,
                Type = "inventory",
                Status = TaskStatus.New
            });

            var itemChunks = DivideItems(dto.ItemPositionIds, targetWorkerIds.Count);
            var posDetails = (await _positionDetailsService.GetPositionDetailsByBranchAsync(dto.BranchId)).ToDictionary(p => p.PositionId);
            var itemPositions = (await _itemPositionRepository.GetAllAsync()).ToDictionary(ip => ip.Id);

            for (int i = 0; i < itemChunks.Count; i++)
            {
                var assignment = new InventoryAssignment(taskId, targetWorkerIds[i], dto.BranchId);
                var assignmentId = await _assignmentRepository.AddAsync(assignment);

                var lines = itemChunks[i].Select(id =>
                {
                    var ip = itemPositions[id];
                    var pd = posDetails[ip.PositionId];
                    return new InventoryAssignmentLine(assignmentId, id, ip.PositionId, PositionCode.FromString(pd.PositionCode), ip.Quantity);
                }).ToList();

                await _lineRepository.AddBatchAsync(lines);
                await _statisticsRepository.AddAsync(new InventoryStatistics(assignmentId, lines.Count));
            }

            return new CompleteInventoryDto { Message = $"Создано {targetWorkerIds.Count} назначений." };
        }

        public async Task<List<InventoryAssignmentHeaderDto>> GetAssignmentsHeaderForWorkerAsync(int userId)
        {
            var assignments = await _assignmentRepository.GetByUserIdAsync(userId);
            return assignments
                .Where(a => a.Status == AssignmentStatus.Assigned || a.Status == AssignmentStatus.InProgress)
                .Select(a => new InventoryAssignmentHeaderDto
                {
                    AssignmentId = a.Id,
                    TaskId = a.TaskId,
                    Status = a.Status,
                    AssignedAt = a.AssignedAt,
                    TotalLines = a.TotalLines,
                    CountedLines = a.CountedLines
                }).ToList();
        }

        public async Task<WorkerInventoryTaskDto> GetInventoryTaskDetailsAsync(int assignmentId)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            if (assignment == null) throw new InvalidOperationException("Назначение не найдено.");

            var baseTask = await _baseTaskService.GetById(assignment.TaskId);
            var lines = await _lineRepository.GetByAssignmentIdAsync(assignmentId);
            var items = (await _itemRepository.GetAllAsync()).ToDictionary(i => i.ItemId);
            var itemPositions = (await _itemPositionRepository.GetAllAsync()).ToDictionary(ip => ip.Id);

            var dto = new WorkerInventoryTaskDto
            {
                AssignmentId = assignment.Id,
                TaskId = assignment.TaskId,
                TaskNumber = baseTask?.Title,
                Status = assignment.Status,
                CreatedDate = assignment.AssignedAt,
                TotalLines = assignment.TotalLines,
                CountedLines = assignment.CountedLines
            };

            foreach (var group in lines.GroupBy(l => l.PositionId))
            {
                var first = group.First();
                var cell = new CellInventoryInfoDto
                {
                    PositionId = group.Key,
                    CellCode = first.PositionCode.ToString(),
                    CellDisplayName = first.PositionCode.ToString()
                };

                foreach (var line in group)
                {
                    var itemId = itemPositions[line.ItemPositionId].ItemId;
                    cell.Items.Add(new InventoryLineDto
                    {
                        LineId = line.Id,
                        ItemPositionId = line.ItemPositionId,
                        ItemId = itemId,
                        ItemName = items[itemId].Name,
                        ExpectedQuantity = line.ExpectedQuantity,
                        ActualQuantity = line.ActualQuantity
                    });
                }
                dto.CellInventories.Add(cell);
            }
            return dto;
        }

        public async Task<bool> StartInventoryAsync(int id)
        {
            var a = await _assignmentRepository.GetByIdAsync(id);
            if (a == null) return false;
            a.Start(DateTime.UtcNow);
            await _assignmentRepository.UpdateAsync(a);
            return true;
        }

        public async Task<bool> PauseInventoryAsync(int id)
        {
            var a = await _assignmentRepository.GetByIdAsync(id);
            if (a == null) return false;
            a.Pause();
            await _assignmentRepository.UpdateAsync(a);
            return true;
        }

        public async Task<bool> CancelInventoryAsync(int id)
        {
            var a = await _assignmentRepository.GetByIdAsync(id);
            if (a == null) return false;
            a.Cancel();
            await _assignmentRepository.UpdateAsync(a);
            return true;
        }

        public async Task<CompleteAssignmentResultDto> CompleteAssignmentAsync(CompleteAssignmentDto dto)
        {
            var a = await _assignmentRepository.GetByIdAsync(dto.AssignmentId);
            if (a == null) throw new InvalidOperationException("Assignment not found.");

            foreach (var lDto in dto.Lines.Where(l => l.LineId.HasValue))
            {
                var line = await _lineRepository.GetByIdAsync(lDto.LineId!.Value);
                if (line != null)
                {
                    line.SetActualQuantity(lDto.ActualQuantity ?? 0);
                    await _lineRepository.UpdateAsync(line);
                }
            }

            a.Complete(DateTime.UtcNow);
            await _assignmentRepository.UpdateAsync(a);

            var finalLines = await _lineRepository.GetByAssignmentIdAsync(a.Id);
            var discrepancies = await ProcessDiscrepanciesAsync(finalLines);

            int queueSize = await _aggregator.GetTotalActiveWorkloadAsync(a.AssignedToUserId);
            await _telemetryService.LogTaskEventAsync(a.AssignedToUserId, a.BranchId, "Inventory", finalLines.Count,
                (int)(DateTime.UtcNow - a.AssignedAt).TotalSeconds, discrepancies.Count, queueSize);

            return new CompleteAssignmentResultDto { Success = true, Message = "Завершено." };
        }

        private List<List<int>> DivideItems(List<int> ids, int count)
        {
            var res = new List<List<int>>();
            if (count <= 0) return res;
            int per = (int)Math.Ceiling((double)ids.Count / count);
            for (int i = 0; i < count; i++) res.Add(ids.Skip(i * per).Take(per).ToList());
            return res;
        }

        private async Task<List<InventoryDiscrepancy>> ProcessDiscrepanciesAsync(List<InventoryAssignmentLine> lines)
        {
            var res = new List<InventoryDiscrepancy>();
            foreach (var l in lines.Where(x => x.ActualQuantity != x.ExpectedQuantity))
            {
                var d = new InventoryDiscrepancy(l.Id, l.ItemPositionId, l.ExpectedQuantity, l.ActualQuantity ?? 0);
                await _discrepancyRepository.AddAsync(d);
                res.Add(d);
            }
            return res;
        }
    }
}