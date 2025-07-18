using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Mapper
{
    public static class BaseTaskMapper
    {
        // ActiveTask → ActiveTaskModel
        public static BaseTaskModel ToModel(this BaseTask entity)
        {
            if (entity == null) return null;
            return new BaseTaskModel
            {
                TaskId = entity.TaskId,
                BranchId = entity.BranchId,
                Type = entity.Type,
                CreatedAt = entity.CreatedAt,
                CompletedAt = entity.CompletedAt,
                Status = entity.Status,
            };
        }

        // ActiveTaskModel → ActiveTask
        public static BaseTask ToDomain(this BaseTaskModel model)
        {
            if (model == null) return null;

            return new BaseTask
            {
                TaskId = model.TaskId,
                BranchId = model.BranchId,
                Type = model.Type,
                CreatedAt = model.CreatedAt,
                CompletedAt = model.CompletedAt,
                Status = model.Status
            };
        }
    }
}
