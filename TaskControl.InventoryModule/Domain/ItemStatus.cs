using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.OrderModule.Domain
{
    /// <summary>
    /// Статус товарной позиции
    /// </summary>
    public class ItemStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ItemPositionId { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression("^(Available|Reserved|Shipped|Defective)$")]
        public string Status { get; set; } = "Available";

        [Required]
        public DateTime StatusDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int Quantity { get; set; }



    }
}
