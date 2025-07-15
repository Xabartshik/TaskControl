using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain; // Предполагается, что есть доменная модель Item

namespace TaskControl.InformationModule.DataAccess.Interface
{
    public interface IItemRepository
    {
        /// <summary>
        /// Получает груз по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор груза</param>
        /// <returns>Найденный груз или null</returns>
        Task<Item?> GetByIdAsync(int id);

        /// <summary>
        /// Получает все грузы
        /// </summary>
        /// <returns>Коллекция грузов</returns>
        Task<IEnumerable<Item>> GetAllAsync();

        /// <summary>
        /// Добавляет новый груз
        /// </summary>
        /// <param name="entity">Добавляемый груз</param>
        /// <returns>Идентификатор добавленного груза</returns>
        /// <exception cref="ArgumentNullException">Если груз равен null</exception>
        Task<int> AddAsync(Item entity);

        /// <summary>
        /// Обновляет данные груза
        /// </summary>
        /// <param name="entity">Обновляемый груз</param>
        /// <returns>Количество измененных записей</returns>
        Task<int> UpdateAsync(Item entity);

        /// <summary>
        /// Удаляет груз по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор груза</param>
        /// <returns>Количество удаленных записей</returns>
        Task<int> DeleteAsync(int id);
   }
}