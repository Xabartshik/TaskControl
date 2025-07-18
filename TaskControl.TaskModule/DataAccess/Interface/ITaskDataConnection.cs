﻿using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInfrastructure;
using TaskControl.TaskModule.DataAccess.Model;

namespace TaskControl.TaskModule.DataAccess.Interface
{
    public interface ITaskDataConnection : IDataConnection
    {
        ITable<BaseTaskModel> ActiveTasks { get; }
        ITable<TaskAssignationModel> TaskAssignations { get; }

    }
}
