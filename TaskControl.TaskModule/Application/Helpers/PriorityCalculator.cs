using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Helpers
{
    public static class PriorityCalculator
    {
        public static TaskPriority CalculatePriority(DateTime? deadline, TaskPriority currentPriority)
        {
            if (!deadline.HasValue)
                return currentPriority;

            var timeRemaining = deadline.Value - DateTime.UtcNow;

            if (timeRemaining.TotalHours <= 1)
                return TaskPriority.Critical; // Менее 1 часа

            if (timeRemaining.TotalHours <= 3)
                return TaskPriority.High; // 1-2 часа

            if (timeRemaining.TotalDays < 1) // От 2 до 48 часов
                return TaskPriority.Normal; // Норма 

            return TaskPriority.Background; // 2 дня и более
        }
    }
}