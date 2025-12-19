using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.DTOs;

namespace TaskControl.InformationModule.Application.Interface
{
    public interface IEmployeesApi
    {
        Task<IReadOnlyList<EmployeeDto>> GetEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    }
}
