using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.DTOs.BossPanelDTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;
using TaskControl.TaskModule.Domain;
using TaskStatus = TaskControl.TaskModule.Domain.TaskStatus;

namespace TaskControl.TaskModule.Application.Services
{
    public class BaseTaskService : IBaseTaskService
    {
        private readonly IActiveTaskRepository _repository;
        private readonly ActiveEmployeeService _employeeService;
        private readonly IInventoryAssignmentRepository _assignmentRepository;
        private readonly IOrderAssemblyAssignmentRepository _assemblyAssignmentRepository;
        private readonly ILogger<BaseTaskService> _logger;
        private readonly AppSettings _appSettings;

        public BaseTaskService(
            IActiveTaskRepository repository,
            ActiveEmployeeService employeeService,
            IInventoryAssignmentRepository assignmentRepository,
            IOrderAssemblyAssignmentRepository assemblyAssignmentRepository,
            ILogger<BaseTaskService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
            _assemblyAssignmentRepository = assemblyAssignmentRepository ?? throw new ArgumentNullException(nameof(assemblyAssignmentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = options?.Value ?? new AppSettings();
        }



        public async Task<int> Add(BaseTaskDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для активной задачи");
                _logger.LogDebug("Добавление задачи: Тип={Type}, Филиал={BranchId}", dto.Type, dto.BranchId);
            }
            _logger.LogInformation("Добавление новой активной задачи типа {Type} для филиала {BranchId}",
                dto.Type, dto.BranchId);

            try
            {
                var entity = BaseTaskDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Активная задача добавлена. ID: {TaskId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления активной задачи типа {Type} для филиала {BranchId}",
                    dto.Type, dto.BranchId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для активной задачи");
                _logger.LogDebug("Удаление задачи ID: {TaskId}", id);
            }
            _logger.LogInformation("Удаление активной задачи ID: {TaskId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Активная задача ID: {TaskId} удалена", id);
                }
                else
                {
                    _logger.LogWarning("Активная задача ID: {TaskId} не найдена", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления активной задачи ID: {TaskId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<BaseTaskDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для активных задач");
                _logger.LogDebug("Получение всех активных задач");
            }
            _logger.LogInformation("Запрос всех активных задач");

            try
            {
                var tasks = await _repository.GetAllAsync();
                var result = tasks.Select(BaseTaskDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} активных задач", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка активных задач");
                throw;
            }
        }


        public async Task<BaseTaskDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для активной задачи");
                _logger.LogDebug("Получение задачи ID: {TaskId}", id);
            }
            _logger.LogInformation("Запрос активной задачи ID: {TaskId}", id);

            try
            {
                var task = await _repository.GetByIdAsync(id);
                if (task == null)
                {
                    _logger.LogWarning("Активная задача ID: {TaskId} не найдена", id);
                    return null;
                }

                _logger.LogInformation("Активная задача ID: {TaskId} получена", id);
                return BaseTaskDto.ToDto(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения активной задачи ID: {TaskId}", id);
                throw;
            }
        }

        public async Task<bool> Update(BaseTaskDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для активной задачи");
                _logger.LogDebug("Обновление задачи ID: {TaskId}", dto.TaskId);
            }
            _logger.LogInformation("Обновление активной задачи ID: {TaskId}", dto.TaskId);

            try
            {
                var entity = BaseTaskDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Активная задача ID: {TaskId} обновлена", dto.TaskId);
                }
                else
                {
                    _logger.LogWarning("Активная задача ID: {TaskId} не найдена", dto.TaskId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления активной задачи ID: {TaskId}", dto.TaskId);
                throw;
            }
        }

        public async Task<IEnumerable<int>> GetAutoSelectedEmployeesAsync(int branchId, int requiredCount)
        {
            _logger.LogInformation("|   [Автоподбор] ищем {Count} сотрудников для филиала {BranchId}", requiredCount, branchId);
            
            var employees = await _employeeService.GetWorkingEmployeesByBranchAsync(branchId);
            if (!employees.Any())
            {
                _logger.LogWarning("|   ! нет работающих сотрудников в филиале {BranchId}", branchId);
                return new List<int>();
            }

            _logger.LogDebug("|     найдено {Count} работающих сотрудников",
                employees.Count());

            var workLoads = new List<(int EmployeeId, int ActiveCount)>();

            foreach (var emp in employees)
            {
                var invAssignments = await _assignmentRepository.GetByUserIdAsync(emp.EmployeeId);
                var invActiveCount = invAssignments.Count(a =>
                    a.Status != Domain.AssignmentStatus.Completed &&
                    a.Status != Domain.AssignmentStatus.Cancelled);

                var oaAssignments = await _assemblyAssignmentRepository.GetByUserIdAsync(emp.EmployeeId);
                var oaActiveCount = oaAssignments.Count(a =>
                    a.Status != AssignmentStatus.Completed &&
                    a.Status != AssignmentStatus.Cancelled);

                workLoads.Add((emp.EmployeeId, invActiveCount + oaActiveCount));
            }

            // Логируем нагрузку каждого сотрудника
            foreach (var wl in workLoads.OrderBy(w => w.ActiveCount))
            {
                _logger.LogDebug("|     UserId={EmployeeId}: {ActiveCount} активных задач", wl.EmployeeId, wl.ActiveCount);
            }

            // Сортируем: сначала те, у кого меньше всего активных задач, и берем нужное количество
            var selectedIds = workLoads
                .OrderBy(w => w.ActiveCount)
                .Take(requiredCount)
                .Select(w => w.EmployeeId)
                .ToList();

            _logger.LogInformation("|   [Автоподбор] выбраны: [{SelectedIds}] (из {Total} сотрудников)",
                string.Join(", ", selectedIds), workLoads.Count);

            return selectedIds;
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, TaskStatus newStatus)
        {
            // 1. Достаем задачу напрямую из базы данных (Модель базы данных, а не DTO)
            var taskModel = await _repository.GetByIdAsync(taskId);

            if (taskModel == null)
            {
                _logger.LogWarning("Попытка обновить статус для несуществующей задачи с ID {TaskId}", taskId);
                return false;
            }

            // 2. Если статус уже такой, какой нужно — ничего не делаем (экономим запрос к БД)
            if (taskModel.Status == newStatus)
            {
                return true;
            }

            // 3. Обновляем статус
            var oldStatus = taskModel.Status;
            taskModel.Status = newStatus;

            // 4. Сохраняем изменения в базу данных
            await _repository.UpdateAsync(taskModel);

            _logger.LogInformation("Статус базовой задачи {TaskId} изменен с {OldStatus} на {NewStatus}",
                taskId, oldStatus, newStatus);

            return true;
        }
    }
}