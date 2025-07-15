using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Interface
{
    public interface ICheckIOEmployeeRepository
    {
        /// <summary>
        /// Получает запись учета прихода/ухода сотрудника по ID
        /// </summary>
        /// <param name="id">Идентификатор записи</param>
        /// <returns>Найденная запись или null</returns>
        Task<CheckIOEmployee?> GetByIdAsync(int id);

        /// <summary>
        /// Получает все записи учета прихода/ухода сотрудников
        /// </summary>
        /// <returns>Коллекция записей</returns>
        Task<IEnumerable<CheckIOEmployee>> GetAllAsync();

        /// <summary>
        /// Добавляет новую запись учета прихода/ухода сотрудника
        /// </summary>
        /// <param name="entity">Добавляемая запись</param>
        /// <returns>Идентификатор добавленной записи</returns>
        /// <exception cref="ArgumentNullException">Если запись равна null</exception>
        Task<int> AddAsync(CheckIOEmployee entity);

        /// <summary>
        /// Обновляет существующую запись учета прихода/ухода сотрудника
        /// </summary>
        /// <param name="entity">Обновляемая запись</param>
        /// <returns>Количество измененных записей</returns>
        Task<int> UpdateAsync(CheckIOEmployee entity);

        /// <summary>
        /// Удаляет запись учета прихода/ухода сотрудника по ID
        /// </summary>
        /// <param name="id">Идентификатор записи</param>
        /// <returns>Количество удаленных записей</returns>
        Task<int> DeleteAsync(int id);
    }
}