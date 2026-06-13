using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.Services;

namespace TaskControl.TaskModule.Application.Observers
{
    public class ReturnTaskGeneratorObserver : IEmployeeCheckInObserver
    {
        private readonly ICourierReturnService _courierReturnService;
        private readonly ILogger<ReturnTaskGeneratorObserver> _logger;

        public ReturnTaskGeneratorObserver(
            ICourierReturnService courierReturnService,
            ILogger<ReturnTaskGeneratorObserver> logger)
        {
            _courierReturnService = courierReturnService;
            _logger = logger;
        }

        public async Task OnEmployeeCheckedAsync(int employeeId, int branchId, string checkType)
        {
            // Нас интересует только конец смены или прибытие на пандус
            if (checkType != "out" && checkType != "dock") return;

            _logger.LogInformation("Обработка чекина '{CheckType}' для курьера {CourierId}. Запуск автоматической генерации возвратов.", checkType, employeeId);
            try
            {
                await _courierReturnService.GenerateReturnsIfAnyAsync(employeeId, branchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке возвратов курьера {CourierId}", employeeId);
            }
        }
    }
}