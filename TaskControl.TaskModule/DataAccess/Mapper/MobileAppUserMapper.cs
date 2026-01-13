using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Mapper
{
    public static class MobileAppUserMapper
    {
        public static MobileAppUserModel ToModel(this MobileAppUser entity)
        {
            if (entity == null) return null;

            return new MobileAppUserModel
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                PasswordHash = entity.PasswordHash,
                Role = entity.Role.ToString(),
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static MobileAppUser ToDomain(this MobileAppUserModel model)
        {
            if (model == null) return null;

            var role = Enum.TryParse<MobileUserRole>(model.Role, ignoreCase: true, out var parsed)
                ? parsed
                : MobileUserRole.Worker;

            var entity = new MobileAppUser
            {
                Id = model.Id,
                EmployeeId = model.EmployeeId,
                PasswordHash = model.PasswordHash,
                Role = role,
                IsActive = model.IsActive,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt
            };

            return entity;
        }
    }
}