using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Interface
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByPhoneAsync(string phone);
        Task<Customer?> GetByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<int> AddAsync(Customer entity);
        Task<int> UpdateAsync(Customer entity);
        Task<int> DeleteAsync(int id);
    }
}