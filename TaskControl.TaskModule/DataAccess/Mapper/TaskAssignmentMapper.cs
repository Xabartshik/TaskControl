using TaskControl.TaskModule.DataAccess.Model;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Mapper
{
    public static class TaskAssignmentMapper
    {
        // Domain -> Model
        public static TaskAssignationModel ToModel(this TaskAssignation entity)
        {
            if (entity == null) return null;

            return new TaskAssignationModel
            {
                Id = entity.Id,
                TaskId = entity.TaskId,
                UserId = entity.UserId,
                AssignedAt = entity.AssignedAt,
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt
            };
        }

        // Model -> Domain
        public static TaskAssignation ToDomain(this TaskAssignationModel model)
        {
            if (model == null) return null;

            return new TaskAssignation
            {
                Id = model.Id,
                TaskId = model.TaskId,
                UserId = model.UserId,
                AssignedAt = model.AssignedAt,
                StartedAt = model.StartedAt,
                CompletedAt = model.CompletedAt
            };
        }
    }
}
