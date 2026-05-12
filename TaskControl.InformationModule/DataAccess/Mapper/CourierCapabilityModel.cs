using LinqToDB.Mapping;

namespace TaskControl.InformationModule.DataAccess.Model
{
    [Table("courier_capabilities")]
    public class CourierCapabilityModel
    {
        [Column("employee_id")][PrimaryKey] public int EmployeeId { get; set; }

        [Column("vehicle_type_id")][NotNull] public int VehicleTypeId { get; set; }

        [Column("max_weight_grams")][NotNull] public double MaxWeightGrams { get; set; }

        [Column("max_length_mm")][NotNull] public double MaxLengthMm { get; set; }

        [Column("max_width_mm")][NotNull] public double MaxWidthMm { get; set; }

        [Column("max_height_mm")][NotNull] public double MaxHeightMm { get; set; }

        [Column("updated_at"), NotNull] public DateTime UpdatedAt { get; set; }
    }
}