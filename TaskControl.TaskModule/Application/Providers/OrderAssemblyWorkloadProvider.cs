using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Application.Providers
{
    public class OrderAssemblyWorkloadProvider : ITaskWorkloadProvider
    {
        private readonly IOrderAssemblyAssignmentRepository _assemblyRepo;
        //private readonly IOrderAssemblyExecutionService _orderAssemblyExecutionService;
        private readonly IBaseTaskService _baseTaskService;

        public string TaskType => "OrderAssembly";

        public OrderAssemblyWorkloadProvider(
            IOrderAssemblyAssignmentRepository assemblyRepo,
            //IOrderAssemblyExecutionService orderAssemblyExecutionService,
            IBaseTaskService baseTaskService)
        {
            _assemblyRepo = assemblyRepo;
            //_orderAssemblyExecutionService = orderAssemblyExecutionService;
            _baseTaskService = baseTaskService;
        }

        public async Task<int> GetActiveWorkloadCountAsync(int workerId)
        {
            var tasks = await _assemblyRepo.GetByUserIdAsync(workerId);
            return tasks.Count(t => (int)t.Status == 0 || (int)t.Status == 1); 
        }

        public async Task<double> GetActiveWorkloadComplexityAsync(int workerId)
        {
            var tasks = await _assemblyRepo.GetByUserIdAsync(workerId);
            // Берем задачи со статусом Assigned (0) или InProgress (1) и суммируем их параметр Complexity
            return tasks
                .Where(t => (int)t.Status == 0 || (int)t.Status == 1)
                .Sum(t => t.Complexity);
        }



        public async Task<bool> HasNewAssignmentsAsync(int workerId)
        {
            var tasks = await _assemblyRepo.GetByUserIdAsync(workerId);
            return tasks.Any(t => (int)t.Status == 0);
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetAvailableTasksAsync(int workerId)
        {
            var assignments = await _assemblyRepo.GetByUserIdAsync(workerId);
            var pending = assignments.Where(t => t.Status == AssignmentStatus.Assigned || t.Status == AssignmentStatus.InProgress || t.Status == AssignmentStatus.Paused).ToList();

            var result = new List<MobileBaseTaskDto>();
            foreach (var a in pending)
            {
                var baseTask = await _baseTaskService.GetById(a.TaskId);
                
                result.Add(new MobileBaseTaskDto
                {
                    TaskId = a.TaskId,
                    Title = baseTask?.Title ?? $"Сборка заказа #{a.OrderId}",
                    TaskType = this.TaskType,
                    PriorityLevel = baseTask?.PriorityLevel ?? 1,
                    Status = TaskStatus.Assigned,
                    AssignmentStatus = a.Status,
                    CreatedAt = a.AssignedAt,
                    Deadline = baseTask?.Deadline,
                    TaskDetails = new
                    {
                        AssignmentId = a.Id,
                        totalLines = a.TotalLines,
                        completedLines = a.Lines?.Count(l => l.Status == Domain.OrderAssemblyLineStatus.Placed) ?? 0
                    }
                });
            }
            return result;
        }

        public async Task<IEnumerable<int>> GetAssignedEmployeeIdsAsync(int taskId)
        {
            var assignments = await _assemblyRepo.GetByTaskIdAsync(taskId);
            var completedStatus = AssignmentStatus.Completed.ToString();
            var cancelledStatus = AssignmentStatus.Cancelled.ToString();
            return assignments
                .Where(a => a.Status != AssignmentStatus.Completed && a.Status != AssignmentStatus.Cancelled)
                .Select(a => a.AssignedToUserId)
                .Distinct()
                .ToList();
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetActiveTasksAsync(int workerId)
        {
            var assignments = await _assemblyRepo.GetByUserIdAsync(workerId);
            var activeAssignments = assignments.Where(t =>
                t.Status == AssignmentStatus.InProgress).ToList();

            var result = new List<MobileBaseTaskDto>();
            foreach (var a in activeAssignments)
            {
                var baseTask = await _baseTaskService.GetById(a.TaskId);
                result.Add(new MobileBaseTaskDto
                {
                    TaskId = a.TaskId,
                    Title = baseTask?.Title ?? $"Сборка заказа #{a.OrderId}",
                    TaskType = this.TaskType,
                    PriorityLevel = baseTask?.PriorityLevel ?? 1,
                    Status = TaskStatus.Assigned,
                    AssignmentStatus = a.Status,
                    CreatedAt = a.AssignedAt,
                    Deadline = baseTask?.Deadline
                });
            }
            return result;
        }
    }
}
