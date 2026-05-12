// TaskControl.InformationModule/Application/Services/CourierCapabilityService.cs
using LinqToDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Mapper;
using TaskControl.InformationModule.DataAccess.Model;

namespace TaskControl.InformationModule.Application.Services
{
    public class CourierCapabilityService
    {
        private readonly IInformationDataConnection _db;
        private readonly IEnumerable<ICourierCreatedEventHandler> _eventHandlers;
        private readonly ILogger<CourierCapabilityService> _logger;

        public CourierCapabilityService(
            IInformationDataConnection db,
            IEnumerable<ICourierCreatedEventHandler> eventHandlers,
            ILogger<CourierCapabilityService> logger)
        {
            _db = db;
            _eventHandlers = eventHandlers;
            _logger = logger;
        }

        public async Task AddOrUpdateCourierCapabilityAsync(CourierCapabilityModel capability, int defaultBranchId = -1)
        {
            _logger.LogInformation("Сохранение характеристик курьера ID: {Id}", capability.EmployeeId);

            try
            {
                // Используем InsertOrReplace (Upsert), чтобы обновить, если уже есть
                await _db.GetTable<CourierCapabilityModel>()
                    .InsertOrUpdateAsync(
                        () => new CourierCapabilityModel
                        {
                            EmployeeId = capability.EmployeeId,
                            VehicleTypeId = capability.VehicleTypeId,
                            MaxWeightGrams = capability.MaxWeightGrams,
                            MaxLengthMm = capability.MaxLengthMm,
                            MaxWidthMm = capability.MaxWidthMm,
                            MaxHeightMm = capability.MaxHeightMm,
                            UpdatedAt = DateTime.UtcNow
                        },
                        p => new CourierCapabilityModel
                        {
                            VehicleTypeId = capability.VehicleTypeId,
                            MaxWeightGrams = capability.MaxWeightGrams,
                            MaxLengthMm = capability.MaxLengthMm,
                            MaxWidthMm = capability.MaxWidthMm,
                            MaxHeightMm = capability.MaxHeightMm,
                            UpdatedAt = DateTime.UtcNow
                        });

                // ВЫЗЫВАЕМ СОБЫТИЯ
                if (_eventHandlers != null)
                {
                    foreach (var handler in _eventHandlers)
                    {
                        await handler.HandleCourierCreatedAsync(
                            capability.EmployeeId,
                            defaultBranchId,
                            capability.MaxLengthMm,
                            capability.MaxWidthMm,
                            capability.MaxHeightMm);
                    }
                }

                _logger.LogInformation("Характеристики курьера ID: {Id} успешно сохранены", capability.EmployeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении характеристик курьера ID: {Id}", capability.EmployeeId);
                throw;
            }
        }
    }
}