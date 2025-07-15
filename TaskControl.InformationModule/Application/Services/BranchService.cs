using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.Domain;

namespace TaskControl.InformationModule.Services
{
    public class BranchService : IService<BranchDto>
    {
        private readonly IBranchRepository _repository;
        private readonly ILogger<BranchService> _logger;
        private readonly AppSettings _appSettings;

        public BranchService(
            IBranchRepository repository,
            ILogger<BranchService> logger,
            IOptions<AppSettings> options)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<int> Add(BranchDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для филиала");
                _logger.LogDebug("Добавление нового филиала: {BranchName}", dto.BranchName);
            }
            _logger.LogInformation("Добавление филиала '{BranchName}'", dto.BranchName);

            try
            {
                var entity = BranchDto.FromDto(dto);
                var newId = await _repository.AddAsync(entity);

                _logger.LogInformation("Филиал успешно добавлен. ID: {BranchId}", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении филиала '{BranchName}'", dto.BranchName);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для филиала");
                _logger.LogDebug("Удаление филиала с ID: {BranchId}", id);
            }
            _logger.LogInformation("Удаление филиала ID: {BranchId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Филиал ID: {BranchId} успешно удален", id);
                }
                else
                {
                    _logger.LogWarning("Филиал ID: {BranchId} не найден", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления филиала ID: {BranchId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<BranchDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для филиалов");
                _logger.LogDebug("Получение всех филиалов");
            }
            _logger.LogInformation("Запрос всех филиалов");

            try
            {
                var branches = await _repository.GetAllAsync();
                var result = branches.Select(BranchDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} филиалов", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка филиалов");
                throw;
            }
        }

        public async Task<BranchDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для филиала");
                _logger.LogDebug("Получение филиала по ID: {BranchId}", id);
            }
            _logger.LogInformation("Запрос филиала ID: {BranchId}", id);

            try
            {
                var branch = await _repository.GetByIdAsync(id);
                if (branch == null)
                {
                    _logger.LogWarning("Филиал ID: {BranchId} не найден", id);
                    return null;
                }

                _logger.LogInformation("Филиал ID: {BranchId} успешно получен", id);
                return BranchDto.ToDto(branch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения филиала ID: {BranchId}", id);
                throw;
            }
        }

        public async Task<bool> Update(BranchDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для филиала");
                _logger.LogDebug("Обновление филиала ID: {BranchId}", dto.BranchId);
            }
            _logger.LogInformation("Обновление филиала ID: {BranchId}", dto.BranchId);

            try
            {
                var entity = BranchDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Филиал ID: {BranchId} успешно обновлен", dto.BranchId);
                }
                else
                {
                    _logger.LogWarning("Филиал ID: {BranchId} не найден", dto.BranchId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления филиала ID: {BranchId}", dto.BranchId);
                throw;
            }
        }
    }
}