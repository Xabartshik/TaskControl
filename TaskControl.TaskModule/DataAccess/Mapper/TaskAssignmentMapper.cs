using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Mapper
{
    public static class TaskAssignmentMapper
    {
        // TaskAssignation → ActiveAssignedTaskModel
        public static TaskAssignationModel ToModel(this TaskAssignation entity)
        {
            if (entity == null) return null;

            return new TaskAssignationModel
            {
                Id = entity.Id,
                TaskId = entity.TaskId,
                UserId = entity.UserId,
                AssignedAt = entity.AssignedAt
            };
        }

        // ActiveAssignedTaskModel → TaskAssignation
        public static TaskAssignation ToDomain(this TaskAssignationModel model)
        {
            if (model == null) return null;

            return new TaskAssignation
            {
                Id = model.Id,
                TaskId = model.TaskId,
                UserId = model.UserId,
                AssignedAt = model.AssignedAt
            };
        }
    }
}
