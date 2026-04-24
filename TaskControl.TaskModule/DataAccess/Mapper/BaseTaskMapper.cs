using System;
using TaskControl.TaskModule.Domain;
using TaskControl.TaskModule.DataAccess.Model;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.DataAccess.Mapper
{
    public static class BaseTaskMapper
    {
        /// <summary>
        /// BaseTask (Domain) → BaseTaskModel (Data)
        /// </summary>
        public static BaseTaskModel ToModel(this BaseTask entity)
        {
            if (entity == null)
                return null;

            return new BaseTaskModel
            {
                TaskId = entity.TaskId,
                Title = entity.Title,
                Description = entity.Description,
                BranchId = entity.BranchId,
                Type = entity.Type,
                CreatedAt = entity.CreatedAt,
                CompletedAt = entity.CompletedAt,
                Status = entity.Status.ToString(),  // Enum → INT для БД
                Priority = entity.Priority
            };
        }

        /// <summary>
        /// BaseTaskModel (Data) → BaseTask (Domain)
        /// </summary>
        public static BaseTask ToDomain(this BaseTaskModel model)
        {
            if (model == null)
                return null;

            return new BaseTask
            {
                TaskId = model.TaskId,
                Title = model.Title,
                Description = model.Description,
                BranchId = model.BranchId,
                Type = model.Type,
                CreatedAt = model.CreatedAt,
                CompletedAt = model.CompletedAt,
                Status = Enum.Parse<TaskStatus>(model.Status),
                Priority = model.Priority
            };
        }

        /// <summary>
        /// Валидация статуса задачи
        /// </summary>
        public static bool IsValidStatus(TaskStatus status)
        {
            return Enum.IsDefined(typeof(TaskStatus), status);
        }

        /// <summary>
        /// Валидация приоритета задачи
        /// </summary>
        public static bool IsValidPriority(int priority)
        {
            return priority >= 0 && priority <= 10;
        }
    }
}
