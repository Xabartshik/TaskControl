using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;

namespace TaskControl.TaskModule.Application.Interface
{
    public interface IInventoryProcessService
    {
        /// <summary>
        /// Создает задачу и распределяет её. 
        /// Если availableWorkers пуст, сервис подберет исполнителей автоматически.
        /// </summary>
        Task<CompleteInventoryDto> CreateAndDistributeInventoryAsync(
            CreateInventoryTaskDto dto,
            List<int> availableWorkers);

        Task<List<InventoryAssignmentHeaderDto>> GetAssignmentsHeaderForWorkerAsync(int userId);
        Task<WorkerInventoryTaskDto> GetInventoryTaskDetailsAsync(int assignmentId);
        Task<bool> StartInventoryAsync(int assignmentId);
        Task<bool> PauseInventoryAsync(int assignmentId);
        Task<bool> CancelInventoryAsync(int assignmentId);
        Task<CompleteAssignmentResultDto> CompleteAssignmentAsync(CompleteAssignmentDto dto);
    }
}