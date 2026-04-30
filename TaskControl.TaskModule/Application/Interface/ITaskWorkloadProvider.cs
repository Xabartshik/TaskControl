using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;

namespace TaskControl.TaskModule.Application.Interface
{
    public interface ITaskWorkloadProvider
    {
        string TaskType { get; }
        Task<int> GetActiveWorkloadCountAsync(int workerId);
        Task<double> GetActiveWorkloadComplexityAsync(int workerId);
        Task<bool> HasNewAssignmentsAsync(int workerId);
        Task<IEnumerable<MobileBaseTaskDto>> GetAvailableTasksAsync(int workerId);
        Task<bool> TryStartTaskAsync(int taskId, int workerId);
    }
}
