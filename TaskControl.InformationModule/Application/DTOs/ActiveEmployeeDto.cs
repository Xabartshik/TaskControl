using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InformationModule.Application.DTOs
{
    /// <summary>
    /// Класс активного сотрудника (на работе)
    /// </summary>
    public class ActiveEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string? MiddleName { get; set; }
        public string Role { get; set; }
        public DateTime CheckInTime { get; set; }
        public bool IsCheckedOut { get; set; }
    }
}
