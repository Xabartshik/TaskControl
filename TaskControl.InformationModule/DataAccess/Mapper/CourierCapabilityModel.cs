using LinqToDB.Mapping;

namespace TaskControl.InformationModule.DataAccess.Model
{
    [Table("courier_capabilities")]
    public class CourierCapabilityModel
    {
        [Column("employee_id")][PrimaryKey] public int EmployeeId { get; set; }

        [Column("vehicle_type_id")][NotNull] public int VehicleTypeId { get; set; }

        [Column("max_weight_kg")][NotNull] public double MaxWeightKg { get; set; }

        [Column("max_length_cm")][NotNull] public double MaxLengthCm { get; set; }

        [Column("max_width_cm")][NotNull] public double MaxWidthCm { get; set; }

        [Column("max_height_cm")][NotNull] public double MaxHeightCm { get; set; }
    }
}