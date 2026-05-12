using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.Helpers;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Jobs
{
    public class PriorityEscalationJob
    {
        private readonly IActiveTaskRepository _baseTaskRepository;
        private readonly ILogger<PriorityEscalationJob> _logger;
        private readonly INotificationService _notificationService;
        private readonly TaskWorkloadAggregator _workloadAggregator;

        public PriorityEscalationJob(
            IActiveTaskRepository baseTaskRepository,
            INotificationService notificationService,
            TaskWorkloadAggregator workloadAggregator,
            ILogger<PriorityEscalationJob> logger)
        {
            _baseTaskRepository = baseTaskRepository;
            _workloadAggregator = workloadAggregator;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Запуск проверки дедлайнов и пересчета приоритетов...");

            try
            {
                // Получаем все активные задачи с дедлайнами
                var activeTasks = await _baseTaskRepository.GetPendingTasksWithDeadlinesAsync();

                int updatedCount = 0;

                foreach (var task in activeTasks)
                {
                    var currentPriority = task.PriorityLevel;
                    var calculatedPriority = PriorityCalculator.CalculatePriority(task.Deadline, currentPriority);

                    // Если приоритет изменился (как в большую, так и в меньшую сторону)
                    if (calculatedPriority != currentPriority)
                    {
                        task.PriorityLevel = calculatedPriority;
                        await _baseTaskRepository.UpdateAsync(task);

                        updatedCount++;
                        _logger.LogInformation(
                            "Приоритет задачи #{TaskId} изменен с {OldPriority} на {NewPriority}. Дедлайн: {Deadline}",
                            task.TaskId, currentPriority, calculatedPriority, task.Deadline);

                        // --- ЛОГИКА УВЕДОМЛЕНИЙ ---
                        // Отправляем уведомления только если приоритет повысился (эскалация)
                        // --- ЛОГИКА УВЕДОМЛЕНИЙ ---
                        if (calculatedPriority > currentPriority)
                        {

                            var assignedUserIds = await _workloadAggregator.GetAssignedEmployeeIdsAsync(task.TaskId);
                            _logger.LogInformation("Отправка пушей для задачи #{TaskId} сотрудникам: {UserIds}",
    task.TaskId, string.Join(", ", assignedUserIds));
                            foreach (var userId in assignedUserIds)
                            {
                                string notifType = calculatedPriority switch
                                {
                                    TaskPriority.Critical => "priority_escalated_3",
                                    TaskPriority.High => "priority_escalated_2",
                                    TaskPriority.Normal => "priority_escalated_1",
                                    _ => "info"
                                };

                                if (calculatedPriority == TaskPriority.Critical)
                                {
                                    await _notificationService.SendNotificationAsync(
                                        userId,
                                        "🔥 СЕЙЧАС ЖЕ! 🔥",
                                        $"Срочно: {task.Title ?? "Выполнение задачи"} (#{task.TaskId})",
                                        notifType
                                    );
                                }
                                else
                                {
                                    await _notificationService.SendNotificationAsync(
                                        userId,
                                        "Приоритет повышен",
                                        $"У задачи #{task.TaskId} истекает время.",
                                        notifType
                                    );
                                }
                            }

                            if (assignedUserIds.Any())
                            {
                                _logger.LogInformation("Уведомления о повышении приоритета задачи #{TaskId} отправлены {Count} сотрудникам.",
                                    task.TaskId, assignedUserIds.Count());
                            }
                        }
                    }
                }

                _logger.LogInformation("Пересчет приоритетов завершен. Обновлено задач: {Count}", updatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении джоба пересчета приоритетов.");
                throw;
            }
        }
    }
}