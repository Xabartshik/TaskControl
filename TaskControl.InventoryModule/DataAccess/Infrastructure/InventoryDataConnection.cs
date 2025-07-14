using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Model;

namespace TaskControl.InventoryModule.DataAccess.Infrastructure
{
    public class InventoryDataConnection : DataConnection, IInventoryDataConnection
    {
        public InventoryDataConnection(IConfiguration configuration)
            : base(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v95),
                  configuration.GetConnectionString("InventoryConnection"))
        {
        }

        public ITable<ItemMovementModel> ItemMovements => this.GetTable<ItemMovementModel>();

        public ITable<ItemPositionModel> ItemPositions => this.GetTable<ItemPositionModel>();
        public ITable<ItemStatusModel> ItemStatuses => this.GetTable<ItemStatusModel>();
        public ITable<OrderPositionModel> OrderPositions => this.GetTable<OrderPositionModel>();
        public ITable<PositionModel> Positions => this.GetTable<PositionModel>();

        public async Task<int> InsertAsync<T>(T entity) where T : class
        {
            return await this.InsertAsync(entity);
        }

        public async Task<int> UpdateAsync<T>(T entity) where T : class
        {
            return await this.UpdateAsync(entity);
        }

        public async Task<int> DeleteAsync<T>(T entity) where T : class
        {
            return await this.DeleteAsync(entity);
        }
    }
}
