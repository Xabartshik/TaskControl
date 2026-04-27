using LinqToDB.Mapping;

namespace TaskControl.InventoryModule.DataAccess.Model
{
    [Table("postamats")]
    public class PostamatModel
    {
        [Column("postamat_id"), PrimaryKey, Identity]
        public int PostamatId { get; set; }

        [Column("address"), NotNull]
        public string Address { get; set; }

        [Column("status"), NotNull]
        public string Status { get; set; }

        [Column("created_at"), NotNull]
        public DateTime CreatedAt { get; set; }
    }
}