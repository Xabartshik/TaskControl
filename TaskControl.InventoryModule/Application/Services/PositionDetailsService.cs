using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskControl.Core.AppSettings;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Interface;

namespace TaskControl.InventoryModule.Application.Services
{
    public class PositionDetailsService
    {
        private readonly IPositionCellRepository _positionRepository;
        private readonly IItemPositionRepository _itemPositionRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ILogger<PositionDetailsService> _logger;
        private readonly AppSettings _appSettings;

        public PositionDetailsService(
            IPositionCellRepository positionRepository,
            IItemPositionRepository itemPositionRepository,
            IItemRepository itemRepository,
            ILogger<PositionDetailsService> logger,
            IOptions<AppSettings> options)
        {
            _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
            _itemPositionRepository = itemPositionRepository ?? throw new ArgumentNullException(nameof(itemPositionRepository));
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _logger = logger;
            _appSettings = options.Value;
        }

        /// <summary>
        /// Получить детальную информацию о позиции по её ID
        /// </summary>
        public async Task<PositionDetailsDto> GetPositionDetailsAsync(int positionId)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetPositionDetailsAsync");
                _logger.LogDebug("Получение детальной информации для позиции ID: {PositionId}", positionId);
            }

            _logger.LogInformation("Запрос детальной информации для позиции ID: {PositionId}", positionId);

            try
            {
                // 1. Получаем информацию о позиции
                var position = await _positionRepository.GetByIdAsync(positionId);
                if (position == null)
                {
                    _logger.LogWarning("Позиция ID: {PositionId} не найдена", positionId);
                    return null;
                }

                // 2. Получаем связь товар-позиция для данной позиции
                var itemPositions = await _itemPositionRepository.GetByPositionIdAsync(positionId);
                var itemPosition = itemPositions.FirstOrDefault();

                if (itemPosition == null)
                {
                    _logger.LogWarning("Для позиции ID: {PositionId} не найдено связей с товарами", positionId);
                    return null;
                }

                // 3. Получаем информацию о товаре
                var item = await _itemRepository.GetByIdAsync(itemPosition.ItemId);
                if (item == null)
                {
                    _logger.LogWarning("Товар ID: {ItemId} не найден для позиции ID: {PositionId}",
                        itemPosition.ItemId, positionId);
                    return null;
                }

                // 4. Формируем DTO
                var result = PositionDetailsDto.ToDto(position, itemPosition, item);

                _logger.LogInformation("Детальная информация для позиции ID: {PositionId} получена успешно", positionId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения детальной информации для позиции ID: {PositionId}", positionId);
                throw;
            }
        }

        /// <summary>
        /// Получить детальную информацию обо всех позициях в филиале
        /// </summary>
        public async Task<IEnumerable<PositionDetailsDto>> GetPositionDetailsByBranchAsync(int branchId)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetPositionDetailsByBranchAsync");
                _logger.LogDebug("Получение детальной информации для позиций филиала ID: {BranchId}", branchId);
            }

            _logger.LogInformation("Запрос детальной информации для позиций филиала ID: {BranchId}", branchId);

            try
            {
                // 1. Получаем все позиции филиала
                var positions = (await _positionRepository.GetByBranchAsync(branchId)).ToList();
                if (!positions.Any())
                {
                    _logger.LogWarning("Для филиала ID: {BranchId} не найдено позиций", branchId);
                    return Enumerable.Empty<PositionDetailsDto>();
                }

                var positionIds = positions.Select(p => p.PositionId).ToList();

                // 2. Получаем все связи товар-позиция для этих позиций
                var allItemPositions = await _itemPositionRepository.GetAllAsync();
                var itemPositionsDict = allItemPositions
                    .Where(ip => positionIds.Contains(ip.PositionId))
                    .GroupBy(ip => ip.PositionId)
                    .ToDictionary(g => g.Key, g => g.First()); // Берем первую связь для каждой позиции

                // 3. Получаем ID всех товаров
                var itemIds = itemPositionsDict.Values.Select(ip => ip.ItemId).Distinct().ToList();

                // 4. Получаем информацию о товарах
                var items = (await _itemRepository.GetByIdsAsync(itemIds)).ToList();
                var itemsDict = items.ToDictionary(i => i.ItemId);

                // 5. Формируем результат
                var result = new List<PositionDetailsDto>();

                foreach (var position in positions)
                {
                    if (!itemPositionsDict.TryGetValue(position.PositionId, out var itemPosition))
                    {
                        _logger.LogDebug("Для позиции ID: {PositionId} не найдено связей с товарами, пропускаем",
                            position.PositionId);
                        continue;
                    }

                    if (!itemsDict.TryGetValue(itemPosition.ItemId, out var item))
                    {
                        _logger.LogDebug("Товар ID: {ItemId} не найден, пропускаем позицию ID: {PositionId}",
                            itemPosition.ItemId, position.PositionId);
                        continue;
                    }

                    result.Add(PositionDetailsDto.ToDto(position, itemPosition, item));
                }

                _logger.LogInformation("Получено {Count} позиций с детальной информацией для филиала ID: {BranchId}",
                    result.Count, branchId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения детальной информации для филиала ID: {BranchId}", branchId);
                throw;
            }
        }

        /// <summary>
        /// Получить детальную информацию обо всех позициях
        /// </summary>
        public async Task<IEnumerable<PositionDetailsDto>> GetAllPositionDetailsAsync()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAllPositionDetailsAsync");
                _logger.LogDebug("Получение детальной информации для всех позиций");
            }

            _logger.LogInformation("Запрос детальной информации для всех позиций");

            try
            {
                // 1. Получаем все позиции
                var positions = (await _positionRepository.GetAllAsync()).ToList();
                if (!positions.Any())
                {
                    _logger.LogWarning("Позиции не найдены");
                    return Enumerable.Empty<PositionDetailsDto>();
                }

                var positionIds = positions.Select(p => p.PositionId).ToList();

                // 2. Получаем все связи товар-позиция
                var allItemPositions = await _itemPositionRepository.GetAllAsync();
                var itemPositionsDict = allItemPositions
                    .Where(ip => positionIds.Contains(ip.PositionId))
                    .GroupBy(ip => ip.PositionId)
                    .ToDictionary(g => g.Key, g => g.First());

                // 3. Получаем ID всех товаров
                var itemIds = itemPositionsDict.Values.Select(ip => ip.ItemId).Distinct().ToList();

                // 4. Получаем информацию о товарах
                var items = (await _itemRepository.GetByIdsAsync(itemIds)).ToList();
                var itemsDict = items.ToDictionary(i => i.ItemId);

                // 5. Формируем результат
                var result = new List<PositionDetailsDto>();

                foreach (var position in positions)
                {
                    if (!itemPositionsDict.TryGetValue(position.PositionId, out var itemPosition))
                    {
                        _logger.LogDebug("Для позиции ID: {PositionId} не найдено связей с товарами, пропускаем",
                            position.PositionId);
                        continue;
                    }

                    if (!itemsDict.TryGetValue(itemPosition.ItemId, out var item))
                    {
                        _logger.LogDebug("Товар ID: {ItemId} не найден, пропускаем позицию ID: {PositionId}",
                            itemPosition.ItemId, position.PositionId);
                        continue;
                    }

                    result.Add(PositionDetailsDto.ToDto(position, itemPosition, item));
                }

                _logger.LogInformation("Получено {Count} позиций с детальной информацией", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения детальной информации для всех позиций");
                throw;
            }
        }
    }
}
