using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.DTOs.BossPanelDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskControl.TaskModule.Application.Interface
{
    public interface IBaseTaskService : IService<BaseTaskDto>
    {
        Task<IEnumerable<int>> GetAutoSelectedEmployeesAsync(int branchId, int requiredCount);
    }
}
