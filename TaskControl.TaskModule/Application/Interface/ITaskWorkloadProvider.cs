using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;

namespace TaskControl.TaskModule.Application.Interface
{
    public interface ITaskWorkloadProvider
    {
        Task<int> GetActiveWorkloadCountAsync(int workerId);
        Task<double> GetActiveWorkloadComplexityAsync(int workerId);
        Task<bool> HasNewAssignmentsAsync(int workerId);
        Task<IEnumerable<MobileBaseTaskDto>> GetAvailableTasksAsync(int workerId);
        Task<IEnumerable<MobileBaseTaskDto>> GetActiveTasksAsync(int workerId);
        Task<IEnumerable<int>> GetAssignedEmployeeIdsAsync(int taskId);
        Task<MobileBaseTaskDto?> GetTaskDetailsAsync(int taskId, int workerId);
    }
}
