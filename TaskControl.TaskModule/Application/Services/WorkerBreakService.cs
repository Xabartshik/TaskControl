using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;
using TaskControl.InformationModule.Services;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;
using TaskControl.TaskModule.Domain;

namespace TaskControl.TaskModule.Application.Services;

public class WorkerBreakService : IWorkerBreakService
{
    private readonly IMobileAppUserRepository _userRepository;
    private readonly IInventoryAssignmentRepository _inventoryRepository;
    private readonly IOrderAssemblyAssignmentRepository _assemblyRepository;
    private readonly AppSettings _appSettings;
    private readonly ILogger<WorkerBreakService> _logger;
    private readonly CheckIOEmployeeService _checkIOService;
    private readonly TaskWorkloadAggregator _workloadAggregator;

    public WorkerBreakService(
        IMobileAppUserRepository userRepository,
        IInventoryAssignmentRepository inventoryRepository,
        IOrderAssemblyAssignmentRepository assemblyRepository,
        CheckIOEmployeeService checkIOService,
        IOptions<AppSettings> appSettings,

        TaskWorkloadAggregator workloadAggregator,
        ILogger<WorkerBreakService> logger)
    {
        _userRepository = userRepository;
        _inventoryRepository = inventoryRepository;
        _assemblyRepository = assemblyRepository;
        _workloadAggregator = workloadAggregator;
        _checkIOService = checkIOService;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public async Task<BreakStatusDto> GetBreakStatusAsync(int employeeId)
    {
        var user = await _userRepository.GetByEmployeeIdAsync(employeeId);
        if (user == null) throw new Exception("Пользователь не найден");

        // 1. Проверяем активные задачи через агрегатор
        var activeTasks = await _workloadAggregator.GetAllActiveTasksAsync(employeeId);
        bool hasActiveTasks = activeTasks.Any();

        // 2. Считаем лимит 20% отдыхающих
        var allUsers = await _userRepository.GetAllAsync();
        var activeWorkers = allUsers.Where(u => u.IsActive).ToList();

        int totalWorkers = activeWorkers.Count;
        int workersOnBreak = activeWorkers.Count(u => u.IsOnBreak);

        double breakPercentage = totalWorkers > 0
            ? ((double)workersOnBreak / totalWorkers) * 100
            : 0;

        bool isLimitReached = breakPercentage >= _appSettings.MaxConcurrentBreaksPercentage;

        // 3. Высчитываем накопленные минуты от отметки заступления на смену
        int accumulatedMinutes = 0;
        var lastCheck = await _checkIOService.GetLastByEmployeeId(employeeId);

        if (lastCheck != null)
        {
            var shiftStartTime = lastCheck.CheckTimeStamp;

            // Если перерыв уже был во время текущей смены, отсчитываем от его конца.
            // Иначе — от начала смены (отметки CheckIO).
            var referenceTime = (user.LastBreakEndTime.HasValue && user.LastBreakEndTime.Value > shiftStartTime)
                ? user.LastBreakEndTime.Value
                : shiftStartTime;

            int rawAccumulatedMinutes = (int)(DateTime.UtcNow - referenceTime).TotalMinutes;

            // Ограничиваем накопленные минуты (не меньше 0 и не больше лимита)
            accumulatedMinutes = Math.Max(0, Math.Min(rawAccumulatedMinutes, _appSettings.WorkMinutesRequiredForBreak));
        }

        bool hasEnoughTime = accumulatedMinutes >= _appSettings.WorkMinutesRequiredForBreak;

        return new BreakStatusDto
        {
            IsOnBreak = user.IsOnBreak,
            AccumulatedMinutes = accumulatedMinutes,
            HasActiveTasks = hasActiveTasks,
            IsLimitReached = isLimitReached,
            CanStartBreak = !user.IsOnBreak && !hasActiveTasks && !isLimitReached && hasEnoughTime
        };
    }



    public async Task StartBreakAsync(int employeeId)
    {
        var status = await GetBreakStatusAsync(employeeId);

        if (!status.CanStartBreak)
        {
            _logger.LogWarning("Сотрудник {EmployeeId} попытался уйти на перерыв, но условия не выполнены.", employeeId);
            throw new InvalidOperationException("Невозможно начать перерыв: есть активные задачи, лимит превышен или не накоплено время.");
        }

        var user = await _userRepository.GetByIdAsync(employeeId);
        user.IsOnBreak = true;
        user.CurrentBreakStartTime = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("Сотрудник {EmployeeId} ушел на перерыв.", employeeId);
    }

    public async Task EndBreakAsync(int employeeId)
    {
        var user = await _userRepository.GetByIdAsync(employeeId);

        if (!user.IsOnBreak) return;

        user.IsOnBreak = false;
        user.LastBreakEndTime = DateTime.UtcNow;
        user.CurrentBreakStartTime = null;

        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("Сотрудник {EmployeeId} завершил перерыв.", employeeId);
    }
}