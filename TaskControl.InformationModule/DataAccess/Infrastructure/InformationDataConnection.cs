using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Model;

namespace TaskControl.InformationModule.DataAccess.Infrastructure
{
    public class InformationDataConnection : DataConnection, IInformationDataConnection
    {
        public InformationDataConnection(IConfiguration configuration)
            : base(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v95),
                  configuration.GetConnectionString("OrganizationConnection"))
        {
        }

        public ITable<BranchModel> Branches => this.GetTable<BranchModel>();
        public ITable<CheckIOEmployeeModel> CheckIoEmployees => this.GetTable<CheckIOEmployeeModel>();
        public ITable<EmployeeModel> Employees => this.GetTable<EmployeeModel>();
        public ITable<ItemModel> Items => this.GetTable<ItemModel>();

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
