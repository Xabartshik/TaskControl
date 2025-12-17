using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.DTOs.InventorizationDTOs
{
    public class DiscrepancyDto
    {
        public int Id { get; set; }
        public int InventoryAssignmentLineId { get; set; }
        public int ItemPositionId { get; set; }
        public int ExpectedQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public int Variance { get; set; }
        public DiscrepancyType Type { get; set; }
        public string? Note { get; set; }
        public DateTime IdentifiedAt { get; set; }
        public DiscrepancyResolutionStatus ResolutionStatus { get; set; } = DiscrepancyResolutionStatus.Pending;


        public static DiscrepancyDto ToDto(InventoryDiscrepancy d)
        {
            return new DiscrepancyDto
            {
                Id = d.Id,
                InventoryAssignmentLineId = d.InventoryAssignmentLineId,
                ItemPositionId = d.ItemPositionId,
                ExpectedQuantity = d.ExpectedQuantity,
                ActualQuantity = d.ActualQuantity,
                Variance = d.Variance,
                Type = d.Type,
                Note = d.Note,
                ResolutionStatus = d.ResolutionStatus,
                IdentifiedAt = d.IdentifiedAt
            };
        }
    }

}

