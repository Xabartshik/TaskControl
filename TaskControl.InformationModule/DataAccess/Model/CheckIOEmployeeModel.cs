using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InformationModule.DataAccess.Model
{
    [Table("check_io_employees")]
    public class CheckIOEmployeeModel
    {
        [Column("id")][PrimaryKey][Identity] public int Id { get; set; }
        [Column("employee_id")][NotNull] public int EmployeeId { get; set; }
        [Column("branch_id")][NotNull] public int BranchId { get; set; }
        [Column("check_type")][NotNull] public string CheckType { get; set; }
        [Column("check_timestamp")][NotNull] public DateTime CheckTimeStamp { get; set; }
    }
}
