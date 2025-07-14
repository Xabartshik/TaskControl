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
    public static class ActiveTaskMapper
    {
        // ActiveTask → ActiveTaskModel
        public static ActiveTaskModel ToModel(this ActiveTask entity)
        {
            if (entity == null) return null;
            var values = entity.JSONParams is null ? null : entity.JSONParams.RootElement.ToString();
            return new ActiveTaskModel
            {
                Id = entity.TaskId,
                BranchId = entity.BranchId,
                Type = entity.Type,
                CreatedAt = entity.CreatedAt,
                CompletedAt = entity.CompletedAt,
                Status = entity.Status,
                JsonParams = values
            };
        }

        // ActiveTaskModel → ActiveTask
        public static ActiveTask ToDomain(this ActiveTaskModel model)
        {
            if (model == null) return null;

            return new ActiveTask
            {
                TaskId = model.Id,
                BranchId = model.BranchId,
                Type = model.Type,
                CreatedAt = model.CreatedAt,
                CompletedAt = model.CompletedAt,
                Status = model.Status,
                JSONParams = model.JsonParams != null
                    ? JsonDocument.Parse(model.JsonParams)
                    : null // Десериализация
            };
        }
    }
}
