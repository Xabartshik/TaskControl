using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace TaskControl.OrderModule.Domain
{
    /// <summary>
    /// Физическое расположение товара на складе
    /// </summary>
    public class ItemPosition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ItemId { get; set; } // Ссылка на номер предмета

        [Required]
        [Range(1, int.MaxValue)]
        public int PositionId { get; set; } // Ссылка на складскую позицию


        [Required]
        public int Quantity { get; set; }

        // Использование UnitsNet для размеров
        public Length Length { get; set; }
        public Length Width { get; set; }
        public Length Height { get; set; }

    }
}
