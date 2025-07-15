using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using TaskControl.Core.SharedInfrastructure;

namespace TaskControl.InventoryModule.DataAccess.Infrastructure
{
    public abstract class BaseDataConnection<T> : DataConnection where T : DataConnection
    {
        protected BaseDataConnection(IConfiguration configuration, string connectionStringName)
            : base(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v95),
                  configuration.GetConnectionString(connectionStringName))
        {
        }

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