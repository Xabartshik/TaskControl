using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.Core.Shared.Shared.SharedInfrastructure;
using TaskControl.Core.Shared.SharedInfrastructure;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.DataAccess.Infrastructure
{
    public class TaskDataConnection : BaseDataConnection<TaskDataConnection>, ITaskDataConnection
    {
        public TaskDataConnection(IConfiguration configuration)
            : base(configuration, "DefaultConnection")
        {
        }

        public ITable<BaseTaskModel> ActiveTasks => this.GetTable<BaseTaskModel>();
        public ITable<TaskAssignationModel> TaskAssignations => this.GetTable<TaskAssignationModel>();

        public ITable<InventoryStatisticsModel> InventoryStatistics => this.GetTable<InventoryStatisticsModel>();

        public ITable<InventoryDiscrepancyModel> InventoryDiscrepancies => this.GetTable<InventoryDiscrepancyModel>();

        public ITable<InventoryAssignmentLineModel> InventoryAssignmentLines => this.GetTable<InventoryAssignmentLineModel>();

        public ITable<InventoryAssignmentModel> InventoryAssignments => this.GetTable<InventoryAssignmentModel>();
    }
}
