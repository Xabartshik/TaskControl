using LinqToDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Mapper;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Repositories
{
    public class CourierCapabilityRepository : ICourierCapabilityRepository
    {
        private readonly IInformationDataConnection _db;
        private readonly ILogger<CourierCapabilityRepository> _logger;

        public CourierCapabilityRepository(
            IInformationDataConnection db,
            ILogger<CourierCapabilityRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
        }

        public async Task<CourierCapability> GetByEmployeeIdAsync(int employeeId)
        {
            var capability = await _db.GetTable<CourierCapabilityModel>()
                .FirstOrDefaultAsync(c => c.EmployeeId == employeeId);

            return capability?.ToDomain();
        }

        public async Task<List<CourierCapability>> GetAllAsync()
        {
            var capabilities = await _db.GetTable<CourierCapabilityModel>()
                .ToListAsync();

            return capabilities.Select(c => c.ToDomain()).ToList();
        }

        public async Task<List<CourierCapability>> GetByVehicleTypeAsync(VehicleType vehicleType)
        {
            int vehicleTypeId = (int)vehicleType;

            var capabilities = await _db.GetTable<CourierCapabilityModel>()
                .Where(c => c.VehicleTypeId == vehicleTypeId)
                .ToListAsync();

            return capabilities.Select(c => c.ToDomain()).ToList();
        }

        public async Task<int> AddAsync(CourierCapability capability)
        {
            var model = capability.ToModel();

            // Здесь используется InsertAsync, так как EmployeeId уже существует 
            // в таблице employees и не является автоинкрементным в courier_capabilities
            return await _db.InsertAsync(model);
        }

        public async Task<int> UpdateAsync(CourierCapability capability)
        {
            var model = capability.ToModel();
            return await _db.UpdateAsync(model);
        }

        public async Task<int> DeleteAsync(int employeeId)
        {
            return await _db.GetTable<CourierCapabilityModel>()
                .Where(c => c.EmployeeId == employeeId)
                .DeleteAsync();
        }
    }
}