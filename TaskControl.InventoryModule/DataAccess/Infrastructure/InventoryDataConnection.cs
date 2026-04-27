using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using TaskControl.Core.Shared.Shared.SharedInfrastructure;
using TaskControl.Core.Shared.SharedInfrastructure;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Model;

namespace TaskControl.InventoryModule.DataAccess.Infrastructure
{
    public class InventoryDataConnection : BaseDataConnection<InventoryDataConnection>, IInventoryDataConnection
    {
        public InventoryDataConnection(IConfiguration configuration)
            : base(configuration, "DefaultConnection")
        {
        }

        public ITable<ItemMovementModel> ItemMovements => this.GetTable<ItemMovementModel>();
        public ITable<ItemPositionModel> ItemPositions => this.GetTable<ItemPositionModel>();
        public ITable<ItemStatusModel> ItemStatuses => this.GetTable<ItemStatusModel>();
        public ITable<PositionModel> PositionCells => this.GetTable<PositionModel>();
        public ITable<OrderReservationModel> OrderReservations => this.GetTable<OrderReservationModel>();
        public ITable<PostamatModel> Postamats => this.GetTable<PostamatModel>();
        public ITable<PostamatCellModel> PostamatCells => this.GetTable<PostamatCellModel>();
    }
}
