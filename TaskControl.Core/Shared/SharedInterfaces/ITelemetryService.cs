namespace TaskControl.Core.Shared.SharedInterfaces
{
    /// <summary>
    /// Универсальный интерфейс для сбора метрик производительности сотрудников.
    /// Позволяет фиксировать результаты узкоспециализированных задач.
    /// </summary>
    public interface ITelemetryService
    {
        /// <summary>
        /// Логирует факт завершения задачи для последующего анализа.
        /// </summary>
        /// <param name="workerId">ID сотрудника</param>
        /// <param name="branchId">ID филиала</param>
        /// <param name="taskCategory">Тип задачи (напр. "OrderAssembly")</param>
        /// <param name="itemsProcessed">Кол-во обработанных единиц (integer)</param>
        /// <param name="durationSeconds">Затраченное время в секундах (integer)</param>
        /// <param name="discrepanciesFound">Кол-во ошибок/расхождений (integer)</param>
        /// <param name="queueSize"> Кол-во задач на момент задачи</param>
        /// <param name="waitTimeSeconds"> Время перед началом выполнения задачи</param>
        Task LogTaskEventAsync(
            int workerId,
            int branchId,
            string taskCategory,
            int itemsProcessed,
            int durationSeconds,
            int discrepanciesFound = 0,
            int waitTimeSeconds = 0, 
            int queueSize = 0);
    }
}