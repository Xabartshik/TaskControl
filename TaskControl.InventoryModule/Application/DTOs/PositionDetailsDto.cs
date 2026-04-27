using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.Domain;
using TaskControl.InventoryModule.Domain;

namespace TaskControl.InventoryModule.Application.DTOs
{
    public class PositionDetailsDto
    {
        /// <summary>
        /// ID позиции (PositionId из ItemPosition)
        /// </summary>
        public int PositionId { get; set; }

        /// <summary>
        /// Полный код позиции (BranchId-ZoneCode-FirstLevelStorageType-FLSNumber-...)
        /// </summary>
        public string PositionCode { get; set; }

        /// <summary>
        /// Название предмета в данной позиции
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// Ожидаемое количество товара на этой позиции
        /// </summary>
        public int ExpectedQuantity { get; set; }

        /// <summary>
        /// Статус позиции (Active, Inactive, Maintenance)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Доступна ли позиция для инвентаризации
        /// </summary>
        public bool IsAvailable { get; set; }

        public static PositionDetailsDto ToDto(PositionCell position, ItemPosition itemPosition, Item item)
        {
            return new PositionDetailsDto
            {
                PositionId = position.PositionId,
                ItemName = item.Name,
                PositionCode = position.Code.ToString(), // "1-ZA-RACK-A1-S1-C3"
                ExpectedQuantity = itemPosition.Quantity, 
                Status = position.Status,
                IsAvailable = position.Status == "Active" 
            };
        }
    }

}
