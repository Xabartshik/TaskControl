using LinqToDB.Mapping;

namespace TaskControl.InventoryModule.DataAccess.Model
{
    [Table("postamat_cells")]
    public class PostamatCellModel
    {
        [Column("cell_id"), PrimaryKey, Identity] public int CellId { get; set; }
        [Column("postamat_id"), NotNull] public int PostamatId { get; set; }
        [Column("cell_number"), NotNull] public string CellNumber { get; set; }
        [Column("size_label")] public string? SizeLabel { get; set; }

        // Значения хранятся в миллиметрах
        [Column("length"), NotNull] public double Length { get; set; }
        [Column("width"), NotNull] public double Width { get; set; }
        [Column("height"), NotNull] public double Height { get; set; }

        [Column("status"), NotNull] public string Status { get; set; }
    }
}