using System;
using System.Collections.Generic;
using TaskControl.InventoryModule.Domain;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.DTOs.InventarizationDTOs
{
    /// <summary>
    /// DTO для строки назначения инвентаризации (базовый - для хранения в БД, репозиториях)
    /// </summary>
    public class InventoryAssignmentLineDto
    {
        public int Id { get; set; }

        public int InventoryAssignmentId { get; set; }

        public int ItemPositionId { get; set; }

        public int PositionId { get; set; }

        public PositionCode? PositionCode { get; set; }

        public int ExpectedQuantity { get; set; }

        public int? ActualQuantity { get; set; }

        public int Variance => ActualQuantity.HasValue ? ActualQuantity.Value - ExpectedQuantity : 0;
    }

    public static class InventoryAssignmentLineMapper
    {
        public static InventoryAssignmentLine ToDomain(this InventoryAssignmentLineDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.PositionCode == null)
                throw new ArgumentNullException(nameof(dto.PositionCode), "PositionCode обязателен для доменной сущности.");

            var line = new InventoryAssignmentLine(
                id: dto.Id,
                inventoryAssignmentId: dto.InventoryAssignmentId,
                itemPositionId: dto.ItemPositionId,
                positionId: dto.PositionId,
                positionCode: dto.PositionCode,
                expectedQuantity: dto.ExpectedQuantity
            );

            if (dto.ActualQuantity.HasValue)
                line.SetActualQuantity(dto.ActualQuantity.Value);

            return line;
        }

        public static InventoryAssignmentLineDto ToDto(this InventoryAssignmentLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));

            return new InventoryAssignmentLineDto
            {
                Id = line.Id,
                InventoryAssignmentId = line.InventoryAssignmentId,
                ItemPositionId = line.ItemPositionId,
                PositionId = line.PositionId,
                PositionCode = line.PositionCode,
                ExpectedQuantity = line.ExpectedQuantity,
                ActualQuantity = line.ActualQuantity
            };
        }
    }

}
