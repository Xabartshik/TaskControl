using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.DTOs;

namespace TaskControl.TaskModule.Application.Interface;

public interface IWorkerBreakService
{
    Task<BreakStatusDto> GetBreakStatusAsync(int employeeId);
    Task StartBreakAsync(int employeeId);
    Task EndBreakAsync(int employeeId);
}