using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using TaskControl.Core.Shared.SharedInfrastructure;

namespace TaskControl.Core.Shared.Shared.SharedInfrastructure
{
    public abstract class BaseDataConnection<Type> : DataConnection where Type : DataConnection
    {
        protected BaseDataConnection(IConfiguration configuration, string connectionStringName)
            : base(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v95),
                  configuration.GetConnectionString(connectionStringName))
        {
        }

        public async Task<int> InsertAsync<T>(T entity) where T : class
        {
            return await DataExtensions.InsertAsync(this, entity);
        }

        public async Task<int> UpdateAsync<T>(T entity) where T : class
        {
            return await DataExtensions.UpdateAsync(this, entity);
        }

        public async Task<int> DeleteAsync<T>(T entity) where T : class
        {
            return await DataExtensions.DeleteAsync(this, entity);
        }
    }
}