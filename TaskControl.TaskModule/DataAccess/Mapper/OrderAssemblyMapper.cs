using System.Collections.Generic;
using TaskControl.TaskModule.DataAccess.Models;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.DataAccess.Mapper
{
    internal static class OrderAssemblyMapper
    {
        public static OrderAssemblyAssignment ToDomainWithLines(
            this OrderAssemblyAssignmentModel model,
            IEnumerable<OrderAssemblyLine> lines)
        {
            if (model == null) return null;

            return new OrderAssemblyAssignment(
                id: model.Id,
                taskId: model.TaskId,
                orderId: model.OrderId,
                assignedToUserId: model.AssignedToUserId,
                branchId: model.BranchId,
                status: (AssignmentStatus)model.Status,
                assignedAtUtc: model.AssignedAt,
                complexity: model.Complexity,
                assignedToRole: (AssignmentRole)model.Role,
                lines: lines
            )
            {
                CompletedAt = model.CompletedAt,
                StartedAt = model.StartedAt
            };
        }

        public static OrderAssemblyAssignment ToDomain(this OrderAssemblyAssignmentModel model)
        {
            if (model == null) return null;

            return new OrderAssemblyAssignment(
                id: model.Id,
                taskId: model.TaskId,
                orderId: model.OrderId,
                assignedToUserId: model.AssignedToUserId,
                branchId: model.BranchId,
                status: (AssignmentStatus)model.Status,
                assignedAtUtc: model.AssignedAt,
                complexity: model.Complexity,
                assignedToRole: (AssignmentRole)model.Role,
                lines: new List<OrderAssemblyLine>()
            )
            {
                CompletedAt = model.CompletedAt,
                StartedAt = model.StartedAt
            };
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
                StartedAt = domain.StartedAt,
                Complexity = domain.Complexity,
                Role = (int)domain.Role,
                CompletedAt = domain.CompletedAt
            };
        }

        public static OrderAssemblyLine ToDomain(this OrderAssemblyLineModel model)
        {
            if (model == null) return null;

            var line = new OrderAssemblyLine(
                id: model.Id,
                orderAssemblyAssignmentId: model.OrderAssemblyAssignmentId,
                itemPositionId: model.ItemPositionId,
                sourcePositionId: model.SourcePositionId,
                targetPositionId: model.TargetPositionId,
                quantity: model.Quantity,
                status: (OrderAssemblyLineStatus)model.Status
            );
            line.SetPickedQuantity(model.PickedQuantity);
            return line;
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
                PickedQuantity = domain.PickedQuantity,
                Status = (int)domain.Status
            };
        }
    }
}