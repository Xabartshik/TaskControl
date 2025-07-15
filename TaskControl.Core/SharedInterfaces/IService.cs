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
        /// Получение всех элементов из таблицы
        /// </summary>
        /// <returns>Экземпляр коллекции, который содержит (или нет) все записи из таблицы</returns>
        Task<IEnumerable<T>> GetAll();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        Task Add(T dto);

        Task<int> Delete(int id);

        Task<int> Update(T dto);


    }
}
