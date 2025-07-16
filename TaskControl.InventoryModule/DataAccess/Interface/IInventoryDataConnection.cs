using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInfrastructure;
using TaskControl.InventoryModule.DataAccess.Model;

namespace TaskControl.InventoryModule.DataAccess.Interface
{
    public interface IInventoryDataConnection : IDataConnection
    {
        ITable<ItemMovementModel> ItemMovements { get; }
        ITable<ItemPositionModel> ItemPositions { get; }
        ITable<ItemStatusModel> ItemStatuses { get; }
        ITable<OrderPositionModel> OrderPositions { get; }
        ITable<PositionModel> PositionCells { get; }

    }
}
