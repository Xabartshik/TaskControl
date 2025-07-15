using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Interface
{
    public interface IEmployeeRepository
    {
        /// <summary>
        /// Получает сотрудника по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сотрудника</param>
        /// <returns>Найденный сотрудник или null</returns>
        Task<Employee?> GetByIdAsync(int id);

        /// <summary>
        /// Получает всех сотрудников
        /// </summary>
        /// <returns>Коллекция сотрудников</returns>
        Task<IEnumerable<Employee>> GetAllAsync();

        /// <summary>
        /// Добавляет нового сотрудника
        /// </summary>
        /// <param name="entity">Добавляемый сотрудник</param>
        /// <returns>Идентификатор добавленного сотрудника</returns>
        /// <exception cref="ArgumentNullException">Если сотрудник равен null</exception>
        Task<int> AddAsync(Employee entity);

        /// <summary>
        /// Обновляет данные сотрудника
        /// </summary>
        /// <param name="entity">Обновляемый сотрудник</param>
        /// <returns>Количество измененных записей</returns>
        Task<int> UpdateAsync(Employee entity);

        /// <summary>
        /// Удаляет сотрудника по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сотрудника</param>
        /// <returns>Количество удаленных записей</returns>
        Task<int> DeleteAsync(int id);
    }
}