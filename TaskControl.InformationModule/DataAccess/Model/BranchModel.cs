using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InformationModule.DataAccess.Model
{
    [Table("branches")]
    public class BranchModel
    {
        [Column("branch_id")][PrimaryKey][Identity] public int BranchId { get; set; }
        [Column("branch_name")][NotNull] public string BranchName { get; set; }
        [Column("branch_type")][NotNull] public string BranchType { get; set; }
        [Column("address")][NotNull] public string Address { get; set; }
        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}
