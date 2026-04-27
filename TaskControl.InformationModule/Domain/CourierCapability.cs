using UnitsNet;
using System.ComponentModel.DataAnnotations;

namespace TaskControl.InformationModule.Domain
{
    public enum VehicleType
    {
        OnFoot = 1,
        Bicycle = 2,
        Car = 3,
        Van = 4,
        Truck = 5
    }

    /// <summary>
    /// Возможности курьера (ресурсные ограничения)
    /// </summary>
    public class CourierCapability
    {
        /// <summary>
        /// ID сотрудника (связь 1-к-1 с Employee)
        /// </summary>
        public int EmployeeId { get; set; }

        /// <summary>
        /// Тип транспортного средства
        /// </summary>
        public VehicleType Vehicle { get; set; }

        /// <summary>
        /// Максимальный вес груза
        /// </summary>
        public Mass MaxWeight { get; set; }

        /// <summary>
        /// Габариты: Максимальная длина
        /// </summary>
        public Length MaxLength { get; set; }

        /// <summary>
        /// Габариты: Максимальная ширина   
        /// </summary>
        public Length MaxWidth { get; set; }

        /// <summary>
        /// Габариты: Максимальная высота
        /// </summary>
        public Length MaxHeight { get; set; }
    }
}