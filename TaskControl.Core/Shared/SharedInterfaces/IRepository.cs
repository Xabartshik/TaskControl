using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.Core.Shared.SharedInterfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        /// <summary>
        /// Получает все сущности из БД в виде коллекции
        /// </summary>
        /// <returns>Коллекция элементов из таблицы -- пустая, если элементов нет</returns>
        Task<IEnumerable<T>> GetAllAsync();
        /// <summary>
        /// Добавляет сущность в БД
        /// </summary>
        /// <param name="entity">Сущность для добавления</param>
        /// <returns>ID новой записи</returns>
        Task<int> AddAsync(T entity);
        /// <summary>
        /// Обновляет сущность в БД
        /// </summary>
        /// <param name="entity">Новая сущность</param>
        /// <returns>0 -- не найдено, 1 -- обновлено</returns>
        Task<int> UpdateAsync(T entity);
        /// <summary>
        /// Удаление сущности из таблицы по его ID. 
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>0 -- не найдено, 1 -- удалено</returns>
        Task<int> DeleteAsync(int id);
    }
}
