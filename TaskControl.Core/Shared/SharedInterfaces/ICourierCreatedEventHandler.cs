namespace TaskControl.Core.Shared.SharedInterfaces
{
    public interface ICourierCreatedEventHandler
    {
        Task HandleCourierCreatedAsync(int employeeId, int defaultBranchId, double length, double width, double height);
    }
}