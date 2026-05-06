using System.Threading.Tasks;

namespace TaskControl.Core.Shared.SharedInterfaces
{
    public interface IEmployeeCheckInObserver
    {
        /// <summary>
        /// Вызывается, когда сотрудник делает отметку на проходной
        /// </summary>
        Task OnEmployeeCheckedAsync(int employeeId, int branchId, string checkType);
    }
}