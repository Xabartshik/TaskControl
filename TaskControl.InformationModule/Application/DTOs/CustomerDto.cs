using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.Application.DTOs
{
    public record CustomerDto
    {
        public int CustomerId { get; init; }
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Phone { get; init; } = null!;
        public string? Email { get; init; }

        public static CustomerDto ToDto(Customer entity) => new()
        {
            CustomerId = entity.CustomerId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Phone = entity.Phone,
            Email = entity.Email
        };
    }

    public record RegisterCustomerRequestDto
    {
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Login { get; init; } = null!;
        public string? Phone { get; init; } = null!;
        public string Password { get; init; } = null!;
        public string? Email { get; init; }
    }
}