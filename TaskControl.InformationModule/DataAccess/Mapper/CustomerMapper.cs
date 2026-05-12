using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Mapper
{
    public static class CustomerMapper
    {
        public static CustomerModel ToModel(this Customer entity) => new()
        {
            CustomerId = entity.CustomerId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Phone = entity.Phone,
            Email = entity.Email,
            CreatedAt = entity.CreatedAt == default ? DateTime.UtcNow : entity.CreatedAt
        };

        public static Customer ToDomain(this CustomerModel model) => new()
        {
            CustomerId = model.CustomerId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.Phone,
            Email = model.Email,
            CreatedAt = model.CreatedAt
        };
    }
}