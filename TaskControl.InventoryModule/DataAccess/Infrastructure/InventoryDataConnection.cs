using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Model;

namespace TaskControl.InventoryModule.DataAccess.Infrastructure
{
    public class InventoryDataConnection : BaseDataConnection<InventoryDataConnection>, IInventoryDataConnection
    {
        public InventoryDataConnection(IConfiguration configuration)
            : base(configuration, "InventoryConnection")
        {
        }

        public ITable<ItemMovementModel> ItemMovements => this.GetTable<ItemMovementModel>();
        public ITable<ItemPositionModel> ItemPositions => this.GetTable<ItemPositionModel>();
        public ITable<ItemStatusModel> ItemStatuses => this.GetTable<ItemStatusModel>();
        public ITable<OrderPositionModel> OrderPositions => this.GetTable<OrderPositionModel>();
        public ITable<PositionModel> PositionCells => this.GetTable<PositionModel>();
    }
}
