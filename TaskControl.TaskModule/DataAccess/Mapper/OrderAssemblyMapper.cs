using System;
using System.Collections.Generic;
using System.Linq;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Mapper
{
    internal static class OrderAssemblyMapper
    {
        public static OrderAssemblyAssignment ToDomainWithLines(this OrderAssemblyAssignmentModel model, IEnumerable<OrderAssemblyLine> lines)
        {
            if (model == null) return null;

            return new OrderAssemblyAssignment(
                model.Id,
                model.TaskId,
                model.OrderId,
                model.AssignedToUserId,
                model.BranchId,
                (OrderAssemblyAssignmentStatus)model.Status,
                model.AssignedAt,
                lines
            )
            {
                CompletedAt = model.CompletedAt
            };
        }
        
        public static OrderAssemblyAssignment ToDomain(this OrderAssemblyAssignmentModel model)
        {
            if (model == null) return null;

            var assignment = new OrderAssemblyAssignment
            {
                Id = model.Id,
                TaskId = model.TaskId,
                OrderId = model.OrderId,
                AssignedToUserId = model.AssignedToUserId,
                BranchId = model.BranchId,
                Status = (OrderAssemblyAssignmentStatus)model.Status,
                AssignedAt = model.AssignedAt,
                CompletedAt = model.CompletedAt
            };
            return assignment;
        }

        public static OrderAssemblyAssignmentModel ToModel(this OrderAssemblyAssignment domain)
        {
            if (domain == null) return null;

            return new OrderAssemblyAssignmentModel
            {
                Id = domain.Id,
                TaskId = domain.TaskId,
                OrderId = domain.OrderId,
                AssignedToUserId = domain.AssignedToUserId,
                BranchId = domain.BranchId,
                Status = (int)domain.Status,
                AssignedAt = domain.AssignedAt,
                CompletedAt = domain.CompletedAt
            };
        }

        public static OrderAssemblyLine ToDomain(this OrderAssemblyLineModel model)
        {
            if (model == null) return null;

            return new OrderAssemblyLine(
                model.Id,
                model.OrderAssemblyAssignmentId,
                model.ItemPositionId,
                model.SourcePositionId,
                model.TargetPositionId,
                model.Quantity,
                (OrderAssemblyLineStatus)model.Status
            );
        }

        public static OrderAssemblyLineModel ToModel(this OrderAssemblyLine domain)
        {
            if (domain == null) return null;

            return new OrderAssemblyLineModel
            {
                Id = domain.Id,
                OrderAssemblyAssignmentId = domain.OrderAssemblyAssignmentId,
                ItemPositionId = domain.ItemPositionId,
                SourcePositionId = domain.SourcePositionId,
                TargetPositionId = domain.TargetPositionId,
                Quantity = domain.Quantity,
                Status = (int)domain.Status
            };
        }
    }
}
