using System.Threading.Tasks;

namespace TaskControl.Core.Shared.SharedInterfaces
{
    /// <summary>
    /// Интерфейс наблюдателя для событий блокировки и разблокировки сотрудников
    /// </summary>
    public interface IEmployeeBlockObserver
    {
        /// <summary>
        /// Вызывается при блокировке сотрудника
        /// </summary>
        Task OnEmployeeBlockedAsync(int employeeId);

        /// <summary>
        /// Вызывается при разблокировке сотрудника
        /// </summary>
        Task OnEmployeeUnblockedAsync(int employeeId);
    }
}
