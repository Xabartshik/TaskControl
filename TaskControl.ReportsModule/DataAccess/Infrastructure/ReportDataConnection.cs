using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.ReportsModule.DataAccess.Interface;
using TaskControl.ReportsModule.DataAccess.Model;

namespace TaskControl.ReportsModule.DataAccess.Infrastructure
{
    public class ReportDataConnection : DataConnection, IReportDataConnection
    {
        public ReportDataConnection(IConfiguration configuration)
            : base(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v95),
                  configuration.GetConnectionString("ReportConnection"))
        {
        }

        public ITable<RawEventModel> RawEvents => this.GetTable<RawEventModel>();

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
