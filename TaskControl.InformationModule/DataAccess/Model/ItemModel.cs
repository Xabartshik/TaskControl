using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.InformationModule.DataAccess.Model
{
    [Table("items")]
    public class ItemModel
    {
        [Column("item_id")][PrimaryKey][Identity] public int ItemId { get; set; }
        [Column("weight")][NotNull] public decimal Weight { get; set; }
        [Column("length")][NotNull] public decimal Length { get; set; }
        [Column("width")][NotNull] public decimal Width { get; set; }
        [Column("height")][NotNull] public decimal Height { get; set; }
        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}
