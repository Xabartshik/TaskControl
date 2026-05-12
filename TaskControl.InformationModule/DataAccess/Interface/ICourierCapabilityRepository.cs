using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Interface
{
    public interface ICourierCapabilityRepository
    {
        Task<CourierCapability> GetByEmployeeIdAsync(int employeeId);
        Task<List<CourierCapability>> GetAllAsync();
        Task<List<CourierCapability>> GetByVehicleTypeAsync(VehicleType vehicleType);
        Task<int> AddAsync(CourierCapability capability);
        Task<int> UpdateAsync(CourierCapability capability);
        Task<int> DeleteAsync(int employeeId);
    }
}