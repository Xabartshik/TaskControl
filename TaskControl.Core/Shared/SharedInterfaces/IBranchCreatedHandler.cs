namespace TaskControl.Core.Shared.SharedInterfaces
{
    /// <summary>
    /// Интерфейс для действий, выполняемых после создания филиала
    /// </summary>
    public interface IBranchCreatedHandler
    {
        Task OnBranchCreatedAsync(int branchId);
    }
}