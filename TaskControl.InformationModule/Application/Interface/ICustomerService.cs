using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InformationModule.Application.DTOs;

namespace TaskControl.InformationModule.Application.Services
{
    public interface ICustomerService
    {
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto?> GetByPhoneAsync(string phone);
        Task<CustomerDto?> GetByEmailAsync(string email);
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto> CreateCustomerAsync(CustomerDto dto);
        Task<CustomerDto> UpdateCustomerAsync(int id, CustomerDto dto);
        Task<bool> DeleteCustomerAsync(int id);
    }
}