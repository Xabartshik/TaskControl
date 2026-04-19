using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Application.Providers
{
    public class InventoryWorkloadProvider : ITaskWorkloadProvider
    {
        private readonly IInventoryAssignmentRepository _inventoryRepo;
        private readonly IBaseTaskService _baseTaskService;

        public string TaskType => "Inventory";

        public InventoryWorkloadProvider(
            IInventoryAssignmentRepository inventoryRepo, 
            IBaseTaskService baseTaskService)
        {
            _inventoryRepo = inventoryRepo;
            _baseTaskService = baseTaskService;
        }

        public async Task<int> GetActiveWorkloadCountAsync(int workerId)
        {
            var tasks = await _inventoryRepo.GetByUserIdAsync(workerId);
            return tasks.Count(t => t.Status == InventoryAssignmentStatus.InProgress);
        }

        public async Task<bool> HasNewAssignmentsAsync(int workerId)
        {
            var tasks = await _inventoryRepo.GetByUserIdAsync(workerId);
            return tasks.Any(t => t.Status == 0); 
        }

        public async Task<IEnumerable<MobileBaseTaskDto>> GetAvailableTasksAsync(int workerId)
        {
            var assignments = await _inventoryRepo.GetByUserIdAsync(workerId);
            var pending = assignments.Where(t => t.Status == InventoryAssignmentStatus.Assigned || t.Status == InventoryAssignmentStatus.InProgress).ToList();

            var result = new List<MobileBaseTaskDto>();
            foreach (var a in pending)
            {
                var baseTask = await _baseTaskService.GetById(a.TaskId); 
                
                result.Add(new MobileBaseTaskDto(null)
                {
                    TaskId = a.TaskId,
                    Title = baseTask?.Title ?? "Инвентаризация ячеек",
                    TaskType = this.TaskType, 
                    Priority = baseTask?.Priority ?? 5,
                    Status = a.Status == InventoryAssignmentStatus.InProgress ? TaskStatus.InProgress : TaskStatus.Assigned,
                    CreatedAt = a.AssignedAt
                });
            }
            return result;
        }

        public async Task<bool> TryStartTaskAsync(int taskId, int workerId)
        {
            var assignment = await _inventoryRepo.GetByIdAsync(taskId);
            if (assignment == null || assignment.AssignedToUserId != workerId) 
                return false;

            assignment.Status = InventoryAssignmentStatus.InProgress; 
            await _inventoryRepo.UpdateAsync(assignment);
            return true;
        }
    }
}
