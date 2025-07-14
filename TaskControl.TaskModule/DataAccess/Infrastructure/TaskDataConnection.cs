using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;

namespace TaskControl.TaskModule.DataAccess.Infrastructure
{
    public class TaskDataConnection : DataConnection, ITaskDataConnection
    {
        public TaskDataConnection(IConfiguration configuration)
            : base(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v95),
                  configuration.GetConnectionString("TaskConnection"))
        {
        }

        public ITable<ActiveTaskModel> ActiveTasks => this.GetTable<ActiveTaskModel>();
        public ITable<TaskAssignationModel> TaskAssignations => this.GetTable<TaskAssignationModel>();

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
