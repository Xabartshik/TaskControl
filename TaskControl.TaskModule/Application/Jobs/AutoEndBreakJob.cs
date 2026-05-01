using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;

namespace TaskControl.TaskModule.Application.Jobs
{
    public class AutoEndBreakJob
    {
        private readonly IMobileAppUserRepository _userRepository;
        private readonly IWorkerBreakService _workerBreakService;
        private readonly ILogger<AutoEndBreakJob> _logger;

        public AutoEndBreakJob(
            IMobileAppUserRepository userRepository,
            IWorkerBreakService workerBreakService,
            ILogger<AutoEndBreakJob> logger)
        {
            _userRepository = userRepository;
            _workerBreakService = workerBreakService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Запуск проверки просроченных перерывов...");

            // В идеале добавить в IMobileAppUserRepository метод GetUsersOnBreakAsync(),
            // чтобы не тянуть всех пользователей из БД. Здесь для примера фильтруем в памяти:
            var usersOnBreak = await _userRepository.GetUsersOnBreakAsync();

            var limitTime = DateTime.UtcNow.AddMinutes(-10); // 10 минут назад

            // 2. Безопасно фильтруем коллекцию
            var expiredBreaks = usersOnBreak
                .Where(u => u.CurrentBreakStartTime.HasValue && u.CurrentBreakStartTime.Value < limitTime)
                .ToList();

            foreach (var user in expiredBreaks)
            {
                try
                {
                    // Используем существующий сервис для консистентности логики (логирование, обновление статусов)
                    await _workerBreakService.EndBreakAsync(user.Id);
                    _logger.LogInformation("Перерыв сотрудника {EmployeeId} завершен автоматически по истечению 10 минут.", user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при автоматическом завершении перерыва для сотрудника {EmployeeId}", user.Id);
                }
            }
        }
    }
}