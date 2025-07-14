using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Mapper
{
    public static class EmployeeMapper
    {
        public static EmployeeModel ToModel(this Employee entity)
        {
            if (entity == null) return null;

            return new EmployeeModel
            {
                EmployeesId = entity.EmployeesId,
                Surname = entity.Surname,
                Name = entity.Name,
                MiddleName = entity.MiddleName,
                Role = entity.Role,
                CreatedAt = DateTime.UtcNow // Устанавливается при создании
            };
        }

        public static Employee ToDomain(this EmployeeModel model)
        {
            if (model == null) return null;

            return new Employee
            {
                EmployeesId = model.EmployeesId,
                Surname = model.Surname,
                Name = model.Name,
                MiddleName = model.MiddleName,
                Role = model.Role
            };
        }
    }
}
