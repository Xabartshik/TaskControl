using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.DataAccess.Interface;

namespace TaskControl.InformationModule.Application.Services
{
    /// <summary>
    /// Сервис для активных сотрудников
    /// </summary>
    public class ActiveEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICheckIOEmployeeRepository _checkIORepository;
        private readonly ILogger<ActiveEmployeeService> _logger;

        public ActiveEmployeeService(
            IEmployeeRepository employeeRepository,
            ICheckIOEmployeeRepository checkIORepository,
            ILogger<ActiveEmployeeService> logger)
        {
            _employeeRepository = employeeRepository;
            _checkIORepository = checkIORepository;
            _logger = logger;
        }


        /// <summary>
        ///  Получить активных сотрудников филиала (те, кто отметился как "в работе")
        /// </summary>
        public async Task<IEnumerable<ActiveEmployeeDto>> GetWorkingEmployeesByBranchAsync(int branchId)
        {
            // Получаем все чеки из репозитория
            // TODO: в идеале нужно добавить в метод GetRecentByBranchAsync, чтобы не тянуть всю таблицу из БД)
            var allChecks = await _checkIORepository.GetAllAsync();
            var threshold = DateTime.UtcNow.AddHours(-24);

            var recentChecks = allChecks
                .Where(c => c.BranchId == branchId && c.CheckTimeStamp >= threshold)
                .GroupBy(c => c.EmployeeId)
                .Select(g => g.OrderBy(c => c.CheckTimeStamp).Last()) // Берем самую последнюю отметку сотрудника
                .ToList();
            var activeChecks = recentChecks
                .Where(c => c.IsCheckIn())
                .ToList();

            var workingEmployeeIds = activeChecks.Select(c => c.EmployeeId).ToList();

            if (!workingEmployeeIds.Any())
            {
                _logger.LogWarning("Нет работающих сотрудников в филиале {BranchId} (активных check-in не найдено)", branchId);
                return Enumerable.Empty<ActiveEmployeeDto>();
            }

            var allEmployees = await _employeeRepository.GetAllAsync();
            var workingEmployees = allEmployees
                .Where(e => workingEmployeeIds.Contains(e.EmployeesId))
                .ToList();

            return workingEmployees.Select(e => new ActiveEmployeeDto
            {
                EmployeeId = e.EmployeesId,
                Surname = e.Surname,
                Name = e.Name,
                MiddleName = e.MiddleName,
                Role = e.Role,
                CheckInTime = activeChecks.First(c => c.EmployeeId == e.EmployeesId).CheckTimeStamp,
                IsCheckedOut = false
            });
        }

        /// <summary>
        /// Получает сотрудника по идентификатору
        /// </summary>
        public async Task<TaskControl.InformationModule.Domain.Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            return await _employeeRepository.GetByIdAsync(employeeId);
        }
    }
}
