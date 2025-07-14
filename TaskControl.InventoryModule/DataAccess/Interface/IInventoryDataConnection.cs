using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Model;

namespace TaskControl.InventoryModule.DataAccess.Interface
{
    public interface IInventoryDataConnection
    {
        ITable<ItemMovementModel> ItemMovements { get; }
        ITable<ItemPositionModel> ItemPositions { get; }
        ITable<ItemStatusModel> ItemStatuses { get; }
        ITable<OrderPositionModel> OrderPositions { get; }
        ITable<PositionModel> Positions { get; }

        Task<int> InsertAsync<T>(T entity) where T : class;
        Task<int> UpdateAsync<T>(T entity) where T : class;
        Task<int> DeleteAsync<T>(T entity) where T : class;
    }
}
