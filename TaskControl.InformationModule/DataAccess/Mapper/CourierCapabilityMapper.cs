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
                MaxWeightGrams = entity.MaxWeight.Grams,
                MaxLengthMm = entity.MaxLength.Millimeters,
                MaxWidthMm = entity.MaxWidth.Millimeters,
                MaxHeightMm = entity.MaxHeight.Millimeters
            };
        }

        public static CourierCapability ToDomain(this CourierCapabilityModel model)
        {
            if (model == null) return null;

            return new CourierCapability
            {
                EmployeeId = model.EmployeeId,
                Vehicle = (VehicleType)model.VehicleTypeId,
                MaxWeight = Mass.FromGrams(model.MaxWeightGrams),
                MaxLength = Length.FromMillimeters(model.MaxLengthMm),
                MaxWidth = Length.FromMillimeters(model.MaxWidthMm),
                MaxHeight = Length.FromMillimeters(model.MaxHeightMm)
            };
        }
    }
}