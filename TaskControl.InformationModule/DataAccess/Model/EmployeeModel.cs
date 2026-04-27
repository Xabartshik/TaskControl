using LinqToDB.Mapping;
using System;

namespace TaskControl.InformationModule.DataAccess.Model
{
    [Table("employees")]
    public class EmployeeModel
    {
        [Column("employees_id")][PrimaryKey][Identity] public int EmployeesId { get; set; }

        [Column("surname")][NotNull] public string Surname { get; set; }

        [Column("name")][NotNull] public string Name { get; set; }

        [Column("middle_name")] public string? MiddleName { get; set; }

        [Column("role_id")][NotNull] public int RoleId { get; set; }

        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}