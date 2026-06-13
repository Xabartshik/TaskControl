using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;

namespace TaskControl.InformationModule.Services
{
    /// <summary>
    /// Интерфейс для сервиса управления сотрудниками
    /// </summary>
    public interface IEmployeeService : IService<EmployeeDto>
    {
        /// <summary>
        /// Блокировка сотрудника
        /// </summary>
        Task<bool> BlockEmployeeAsync(int id);

        /// <summary>
        /// Разблокировка сотрудника
        /// </summary>
        Task<bool> UnblockEmployeeAsync(int id);
    }
}
