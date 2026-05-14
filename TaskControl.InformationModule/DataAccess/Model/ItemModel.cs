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
        [Column("item_id")][PrimaryKey] public int ItemId { get; set; }
        [Column("name")][NotNull] public string Name { get; set; }
        [Column("weight")][NotNull] public double Weight { get; set; }
        [Column("length")][NotNull] public double Length { get; set; }
        [Column("width")][NotNull] public double Width { get; set; }
        [Column("height")][NotNull] public double Height { get; set; }
        [Column("price")][NotNull] public decimal Price { get; set; }
        [Column("barcode")] public string Barcode { get; init; }
        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}
