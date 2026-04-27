using System.ComponentModel.DataAnnotations;


namespace TaskControl.InventoryModule.Domain
{
    public enum PostamatStatus
    {
        Active,
        Inactive,
        Maintenance
    }

    public enum PostamatCellStatus
    {
        Available,
        Reserved,   // Забронирована под новый заказ
        Occupied,   // Заказ физически внутри
        Maintenance
    }

    public class Postamat
    {
        [Required]
        public int PostamatId { get; set; }

        [Required(ErrorMessage = "Адрес постамата обязателен")]
        [StringLength(500)]
        public string Address { get; set; }

        [Required]
        public PostamatStatus Status { get; set; } = PostamatStatus.Active;

        /// <summary>
        /// Список всех ячеек данного постамата.
        /// </summary>
        public List<PostamatCell> Cells { get; set; } = new();
    }
}