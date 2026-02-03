using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.TaskModule.Application.DTOs.InventarizationDTOs;

namespace TaskControl.TaskModule.Application.Interface
{
    /// <summary>
    /// Интерфейс основного сервиса обработки инвентаризации
    /// Отвечает за создание, распределение, выполнение и завершение инвентаризации
    /// </summary>
    public interface IInventoryProcessService
    {
        /// <summary>
        /// Создать новую инвентаризацию и распределить товары между работниками
        /// </summary>
        /// <param name="dto">Данные для создания инвентаризации</param>
        /// <param name="availableWorkers">Список доступных работников для распределения</param>

        /// <returns>Результат создания с информацией о распределении</returns>
        Task<CompleteInventoryDto> CreateAndDistributeInventoryAsync(
            CreateInventoryTaskDto dto,
            List<int> availableWorkers);

        Task<List<InventoryAssignmentDetailedWithItemDto>> GetNewAssignmentsForWorkerAsync(int userId);
        Task<InventoryTaskDetailsDto> GetInventoryTaskDetailsForWorkerAsync(int userId, int inventoryTaskId);

        Task<bool> HasNewTasksForWorkerAsync(int userId, DateTime? since = null);


        /// <summary>
        /// Получить текущий прогресс выполнения инвентаризации
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>

        /// <returns>Информация о текущем прогрессе</returns>
        Task<GetInventoryProgressDto> GetInventoryProgressAsync(
            int assignmentId);

        /// <summary>
        /// Обработать сканирование товара при инвентаризации
        /// Обновляет фактическое количество и создаёт расхождение если нужно
        /// </summary>
        /// <param name="dto">Данные сканирования</param>

        /// <returns>Обновленная статистика инвентаризации</returns>
        Task<InventoryStatisticsDto> ProcessInventoryScanAsync(
            ProcessInventoryScanDto dto);

        /// <summary>
        /// Завершить инвентаризацию и получить финальный отчёт
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>

        /// <returns>Финальный отчёт с результатами</returns>
        Task<CompleteInventoryDto> CompleteInventoryAsync(
            int assignmentId);

        /// <summary>
        /// Отменить инвентаризацию
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>
        /// <param name="reason">Причина отмены</param>

        /// <returns>Результат отмены</returns>
        Task<bool> CancelInventoryAsync(
            int assignmentId,
            string? reason = null);

        /// <summary>
        /// Получить все назначения инвентаризации для конкретного пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>

        /// <returns>Список назначений пользователю</returns>
        Task<List<InventoryAssignmentDetailedDto>> GetUserAssignmentsAsync(
            int userId);

        /// <summary>
        /// Получить активные (незавершенные) инвентаризации
        /// </summary>
        /// <param name="branchId">ID филиала (опционально)</param>

        /// <returns>Список активных инвентаризаций</returns>
        Task<List<InventoryAssignmentDetailedDto>> GetActiveInventoriesAsync(
            int? branchId = null);

        /// <summary>
        /// Получить завершенные инвентаризации за период
        /// </summary>
        /// <param name="startDate">Начало периода</param>
        /// <param name="endDate">Конец периода</param>
        /// <param name="branchId">ID филиала (опционально)</param>

        /// <returns>Список завершенных инвентаризаций</returns>
        Task<List<InventoryAssignmentDetailedDto>> GetCompletedInventoriesAsync(
            DateTime startDate,
            DateTime endDate,
            int? branchId = null);

        /// <summary>
        /// Переназначить инвентаризацию другому работнику
        /// </summary>
        /// <param name="assignmentId">ID текущего назначения</param>
        /// <param name="newUserId">ID нового работника</param>
        /// <param name="reason">Причина переназначения</param>

        /// <returns>Результат переназначения</returns>
        Task<bool> ReassignInventoryAsync(
            int assignmentId,
            int newUserId,
            string? reason = null);

        /// <summary>
        /// Возобновить незавершённую инвентаризацию
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>

        /// <returns>Информация о возобновленной инвентаризации</returns>
        Task<GetInventoryProgressDto> ResumeInventoryAsync(
            int assignmentId);

        /// <summary>
        /// Получить статистику инвентаризации по конкретному назначению
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>

        /// <returns>Подробная статистика</returns>
        Task<InventoryStatisticsDto> GetStatisticsAsync(
            int assignmentId);

        /// <summary>
        /// Отменить сканирование (удалить последнее сканированное значение)
        /// </summary>
        /// <param name="lineId">ID строки назначения</param>

        /// <returns>Результат отмены сканирования</returns>
        Task<bool> UndoScanAsync(
            int lineId);

        /// <summary>
        /// Получить список товаров которые ещё не отсчитаны в назначении
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>

        /// <returns>Список неотсчитанных товаров</returns>
        Task<List<InventoryAssignmentLineDto>> GetUncountedItemsAsync(
            int assignmentId);

        /// <summary>
        /// Получить рекомендации по ускорению процесса (на основе текущего прогресса)
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>

        /// <returns>Список рекомендаций</returns>
        Task<List<string>> GetOptimizationRecommendationsAsync(
            int assignmentId);

        /// <summary>
        /// Валидировать сканирование перед применением
        /// Проверяет: существует ли товар, количество > 0 и т.д.
        /// </summary>
        /// <param name="dto">Данные сканирования</param>

        /// <returns>Результат валидации с сообщением об ошибке если есть</returns>
        Task<(bool IsValid, string? ErrorMessage)> ValidateScanAsync(
            ProcessInventoryScanDto dto);

        /// <summary>
        /// Синхронизировать данные инвентаризации (в случае рассинхронизации)
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>

        /// <returns>Синхронизированная статистика</returns>
        Task<InventoryStatisticsDto> SyncInventoryDataAsync(
            int assignmentId);

        /// <summary>
        /// Добавить примечание к инвентаризации
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>
        /// <param name="note">Содержимое примечания</param>

        /// <returns>Результат добавления примечания</returns>
        Task<bool> AddNoteAsync(
            int assignmentId,
            string note);

        /// <summary>
        /// Получить метрики производительности для инвентаризации
        /// Включает: время выполнения, скорость сканирования, процент ошибок
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>

        /// <returns>Метрики производительности</returns>
        Task<Dictionary<string, object>> GetPerformanceMetricsAsync(
            int assignmentId);

        /// <summary>
        /// Завершает задание инвентаризации для конкретного сотрудника
        /// </summary>
        Task<CompleteAssignmentResultDto> CompleteAssignmentAsync(CompleteAssignmentDto dto);

        /// <summary>
        /// Экспортировать результаты инвентаризации в различные форматы
        /// </summary>
        /// <param name="assignmentId">ID назначения инвентаризации</param>
        /// <param name="format">Формат экспорта (PDF, Excel, CSV)</param>

        /// <returns>Поток с данными в указанном формате</returns>
        Task<Stream> ExportResultsAsync(
            int assignmentId,
            ExportFormat format);
    }

    /// <summary>
    /// Перечисление для форматов экспорта результатов
    /// </summary>
    public enum ExportFormat
    {
        /// <summary>
        /// PDF формат
        /// </summary>
        Pdf = 0,

        /// <summary>
        /// Excel формат
        /// </summary>
        Excel = 1,

        /// <summary>
        /// CSV формат
        /// </summary>
        Csv = 2,

        /// <summary>
        /// JSON формат
        /// </summary>
        Json = 3
    }


}
