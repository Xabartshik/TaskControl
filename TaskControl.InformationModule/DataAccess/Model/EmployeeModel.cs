using System;
using LinqToDB.Mapping; // Оставляем только LinqToDB

namespace TaskControl.InformationModule.DataAccess.Model
{
    [Table("employees")]
    public class EmployeeModel
    {
        [Column("employees_id"), PrimaryKey, Identity]
        public int EmployeesId { get; set; }

        [Column("surname"), NotNull]
        public string Surname { get; set; }

        [Column("name"), NotNull]
        public string Name { get; set; }

        [Column("middle_name")]
        public string? MiddleName { get; set; }

        [Column("role_id"), NotNull]
        public int RoleId { get; set; }

        [Column("created_at"), NotNull]
        public DateTime CreatedAt { get; set; }

        // Используем атрибут LinqToDB, чтобы ORM игнорировала это вычисляемое свойство
        [NotColumn]
        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{Surname} {Name}"
            : $"{Surname} {Name} {MiddleName}";
    }
}