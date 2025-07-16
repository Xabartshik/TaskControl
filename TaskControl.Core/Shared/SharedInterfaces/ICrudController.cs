using Microsoft.AspNetCore.Mvc;

namespace TaskControl.Core.Shared.SharedInterfaces
{
    /// <summary>
    /// Интерфейс для управления ячейками позиций
    /// </summary>
    public interface ICrudController<TDto, TKey> where TDto : class
    {
        /// <summary>
        /// Получить все сущности
        /// </summary>
        /// <returns>Список всех сущностей</returns>
        Task<ActionResult<IEnumerable<TDto>>> GetAll();

        /// <summary>
        /// Получить сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность</returns>
        Task<ActionResult<TDto>> GetById(TKey id);

        /// <summary>
        /// Добавить новую сущность
        /// </summary>
        /// <param name="dto">DTO сущности</param>
        /// <returns>Идентификатор созданной сущности</returns>
        Task<ActionResult<TKey>> Add(TDto dto);

        /// <summary>
        /// Обновить существующую сущность
        /// </summary>
        /// <param name="dto">DTO сущности</param>
        /// <returns>Результат операции</returns>
        Task<IActionResult> Update(TDto dto);

        /// <summary>
        /// Удалить сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Результат операции</returns>
        Task<IActionResult> Delete(TKey id);
    }
}