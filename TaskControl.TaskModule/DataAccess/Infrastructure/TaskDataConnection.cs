using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Infrastructure;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;

namespace TaskControl.TaskModule.DataAccess.Infrastructure
{
    public class TaskDataConnection : BaseDataConnection<TaskDataConnection>, ITaskDataConnection
    {
        public TaskDataConnection(IConfiguration configuration)
            : base(configuration, "TaskConnection")
        {
        }

        public ITable<BaseTaskModel> ActiveTasks => this.GetTable<BaseTaskModel>();
        public ITable<TaskAssignationModel> TaskAssignations => this.GetTable<TaskAssignationModel>();

    }
}
