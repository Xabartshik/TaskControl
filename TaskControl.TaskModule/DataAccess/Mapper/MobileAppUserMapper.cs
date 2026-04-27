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
                Login = entity.Login,
                EmployeeId = entity.EmployeeId,
                CustomerId = entity.CustomerId,
                PasswordHash = entity.PasswordHash,
                Role = (int)entity.Role,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                BranchId = entity.BranchId
            };
        }

        public static MobileAppUser ToDomain(this MobileAppUserModel model)
        {
            if (model == null) return null;

            return new MobileAppUser
            {
                Id = model.Id,
                Login = model.Login,
                EmployeeId = model.EmployeeId,
                CustomerId = model.CustomerId,
                PasswordHash = model.PasswordHash,
                Role = (MobileUserRole)model.Role,
                IsActive = model.IsActive,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                BranchId = model.BranchId
            };
        }
    }
}