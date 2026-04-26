using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Application.Providers
{
    public class OrderAssemblyWorkloadProvider : ITaskWorkloadProvider
    {
        private readonly IOrderAssemblyAssignmentRepository _assemblyRepo;
        private readonly IBaseTaskService _baseTaskService;

        public string TaskType => "OrderAssembly";

        public OrderAssemblyWorkloadProvider(
            IOrderAssemblyAssignmentRepository assemblyRepo,
            IBaseTaskService baseTaskService)
        {
            _assemblyRepo = assemblyRepo;
            _baseTaskService = baseTaskService;
        }

        public async Task<int> GetActiveWorkloadCountAsync(int workerId)
        {
            var tasks = await _assemblyRepo.GetByUserIdAsync(workerId);
            return tasks.Count(t => (int)t.Status == 0 || (int)t.Status == 1); 
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

        public async Task<bool> TryStartTaskAsync(int taskId, int workerId)
        {
            var assignment = await _assemblyRepo.GetByIdAsync(taskId);
            if (assignment == null || assignment.AssignedToUserId != workerId) 
                return false;

            assignment.Status = Domain.AssignmentStatus.InProgress;
            await _assemblyRepo.UpdateAsync(assignment);
            return true;
        }
    }
}
