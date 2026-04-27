using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Interface
{
    public interface IPostamatRepository : IRepository<Postamat>
    {
        /// <summary>
        /// Получает список всех активных постаматов для отображения в мобильном приложении.
        /// </summary>
        Task<IEnumerable<Postamat>> GetActivePostamatsAsync();
    }
}