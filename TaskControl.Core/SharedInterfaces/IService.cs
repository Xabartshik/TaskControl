using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.Core.SharedInterfaces
{
    public interface IService<T> where T : class
    {
        /// <summary>
        /// Возврат объекта по его идентификатору. Если объект не найден, возвращает null
        /// </summary>
        /// <param name="id">Идентификатор объекта, значение которого необходимо вернуть</param>
        /// <returns>Объект, полученный из БД</returns>
        Task<T?> GetById(int id);

        /// <summary>
        ///  Возвращает список всех полученных значений в виде коллекции элементов
        /// </summary>
        /// <returns>Экземпляр коллекции, который содержит (или нет) все записи из таблицы</returns>
        Task<IEnumerable<T>> GetAll();

        /// <summary>
        /// Добавление объекта в таблицу
        /// </summary>
        /// <param name="dto">DTO для добавления</param>
        /// <returns>ID новой записи</returns>
        Task<int> Add(T dto);

        /// <summary>
        /// Удаление сущности из таблицы
        /// </summary>
        /// <param name="id">ID записи для удаления</param>
        /// <returns>False -- не найдено, True -- удалено</returns>
        Task<bool> Delete(int id);

        /// <summary>
        /// Обновить сущность в таблице
        /// </summary>
        /// <param name="dto">Сущность, которая заменит старую</param>
        /// <returns>False -- не найдено, True -- обновлено</returns>
        Task<bool> Update(T dto);


    }
}
