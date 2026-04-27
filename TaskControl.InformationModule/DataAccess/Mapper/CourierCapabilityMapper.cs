using UnitsNet;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.DataAccess.Mapper
{
    public static class CourierCapabilityMapper
    {
        public static CourierCapabilityModel ToModel(this CourierCapability entity)
        {
            if (entity == null) return null;

            return new CourierCapabilityModel
            {
                EmployeeId = entity.EmployeeId,
                VehicleTypeId = (int)entity.Vehicle,
                MaxWeightKg = entity.MaxWeight.Grams,
                MaxLengthCm = entity.MaxLength.Millimeters,
                MaxWidthCm = entity.MaxWidth.Millimeters,
                MaxHeightCm = entity.MaxHeight.Millimeters
            };
        }

        public static CourierCapability ToDomain(this CourierCapabilityModel model)
        {
            if (model == null) return null;

            return new CourierCapability
            {
                EmployeeId = model.EmployeeId,
                Vehicle = (VehicleType)model.VehicleTypeId,
                MaxWeight = Mass.FromGrams(model.MaxWeightKg),
                MaxLength = Length.FromMillimeters(model.MaxLengthCm),
                MaxWidth = Length.FromMillimeters(model.MaxWidthCm),
                MaxHeight = Length.FromMillimeters(model.MaxHeightCm)
            };
        }
    }
}