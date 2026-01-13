using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInfrastructure;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.DataAccess.Models;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface ITaskDataConnection : IDataConnection
    {
        ITable<BaseTaskModel> ActiveTasks { get; }
        ITable<TaskAssignationModel> TaskAssignations { get; }
        ITable<InventoryStatisticsModel> InventoryStatistics { get; }
        ITable<InventoryDiscrepancyModel> InventoryDiscrepancies { get; }
        ITable<InventoryAssignmentLineModel> InventoryAssignmentLines { get; }
        ITable<InventoryAssignmentModel> InventoryAssignments { get; }
        ITable<MobileAppUserModel> MobileAppUsers { get; }
    }
}
