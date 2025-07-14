using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.OrderModule.DataAccess.Interface;
using TaskControl.OrderModule.DataAccess.Model;

namespace TaskControl.OrderModule.DataAccess.Infrastructure
{
    public class OrderDataConnection : DataConnection, IOrderDataConnection
    {
        public OrderDataConnection(IConfiguration configuration)
            : base(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v95),
                  configuration.GetConnectionString("OrderConnection"))
        {
        }

        public ITable<OrderModel> Orders => this.GetTable<OrderModel>();

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
