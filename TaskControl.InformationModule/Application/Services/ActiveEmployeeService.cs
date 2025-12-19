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
            var allChecks = (await _checkIORepository.GetAllAsync()).ToList();
            var today = DateTime.UtcNow.Date;

            // Группируем по сотруднику, берем все записи за день
            var employeeChecks = allChecks
                .Where(c => c.BranchId == branchId && c.CheckTimeStamp.Date == today)
                .GroupBy(c => c.EmployeeId)
                .Select(g => g.OrderBy(c => c.CheckTimeStamp).ToList())
                .ToList();

            var workingEmployeeIds = new List<int>();

            // Проверяем: если последняя запись - это check-in, то сотрудник работает
            foreach (var checks in employeeChecks)
            {
                if (checks.Last().IsCheckIn())
                    workingEmployeeIds.Add(checks.First().EmployeeId);
            }

            var allEmployees = await _employeeRepository.GetAllAsync();
            var employees = allEmployees
                .Where(e => workingEmployeeIds.Contains(e.EmployeesId))
                .ToList();

            return employees.Select(e => new ActiveEmployeeDto
            {
                EmployeeId = e.EmployeesId,
                Surname = e.Surname,
                Name = e.Name,
                MiddleName = e.MiddleName,
                Role = e.Role,
                CheckInTime = allChecks
                    .Where(c => c.EmployeeId == e.EmployeesId && c.IsCheckIn())
                    .OrderByDescending(c => c.CheckTimeStamp)
                    .First().CheckTimeStamp,
                IsCheckedOut = false
            });
        }
    }

}
