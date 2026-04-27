using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface IMobileAppUserRepository
    {
        Task<MobileAppUser?> GetByIdAsync(int id);
        Task<IEnumerable<MobileAppUser>> GetAllAsync();
        Task<int> AddAsync(MobileAppUser entity);
        Task<int> UpdateAsync(MobileAppUser entity);
        Task<int> DeleteAsync(int id);
        Task<MobileAppUser?> GetByEmployeeIdAsync(int employeeId);
        Task<int> DeleteByEmployeeIdAsync(int employeeId);
        /// <summary>
        /// Найти пользователя по универсальному логину (ID, Email или Телефон)
        /// </summary>
        Task<MobileAppUser?> GetByLoginAsync(string login);

        /// <summary>
        /// Найти пользователя, привязанного к конкретному покупателю
        /// </summary>
        Task<MobileAppUser?> GetByCustomerIdAsync(int customerId);
    }
}