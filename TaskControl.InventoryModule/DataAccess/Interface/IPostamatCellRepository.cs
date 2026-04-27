using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.DataAccess.Interface
{
    public interface IPostamatCellRepository : IRepository<PostamatCell>
    {
        Task<IEnumerable<PostamatCell>> GetAvailableCellsAsync(int postamatId);
        Task<IEnumerable<PostamatCell>> GetCellsByPostamatIdAsync(int postamatId);
    }
}