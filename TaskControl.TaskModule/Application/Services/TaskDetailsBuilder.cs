using System.Linq;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;
using TaskControl.TaskModule.DataAccess.Mapper;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services
{
    public interface ITaskDetailsBuilder
    {
        TaskDetailsDto BuildOrderAssemblyDetails(OrderAssemblyAssignment assignment);
    }

    public sealed class TaskDetailsBuilder : ITaskDetailsBuilder
    {
        public TaskDetailsDto BuildOrderAssemblyDetails(OrderAssemblyAssignment assignment)
        {
            var completedLines = assignment.Lines?.Count(l => l.Status == OrderAssemblyLineStatus.Placed) ?? 0;
            return new TaskDetailsDto
            {
                AssignmentId = assignment.Id,
                Progress = new TaskProgressDto
                {
                    Total = assignment.TotalLines,
                    Completed = completedLines
                },
                BusinessIdentifiers = new TaskBusinessIdentifiersDto
                {
                    OrderId = assignment.OrderId,
                    TaskId = assignment.TaskId
                }
            };
        }
    }
}
