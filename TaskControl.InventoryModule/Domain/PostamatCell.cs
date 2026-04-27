using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace TaskControl.InventoryModule.Domain
{
    public class PostamatCell
    {
        [Required]
        public int CellId { get; set; }

        [Required]
        public int PostamatId { get; set; }

        [Required]
        [StringLength(20)]
        public string CellNumber { get; set; }

        [StringLength(50)]
        public string? SizeLabel { get; set; }

        public Length Length { get; set; }
        public Length Width { get; set; }
        public Length Height { get; set; }

        /// <summary>
        /// Автоматическое вычисление объема ячейки
        /// </summary>
        public Volume Capacity => Length * Width * Height;

        [Required]
        public PostamatCellStatus Status { get; set; } = PostamatCellStatus.Available;
    }
}