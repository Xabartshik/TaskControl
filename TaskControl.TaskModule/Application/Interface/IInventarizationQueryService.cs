using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface IInventarizationQueryRepository
    {
        Task<InventoryTaskDetailsDto?> GetInventoryTaskDetailsAsync(int taskId);
        Task<IReadOnlyList<int>> GetTaskIdsAssignedToEmployeeAsync(
            int employeeId,
            IEnumerable<string> excludedStatuses);
    }
}
