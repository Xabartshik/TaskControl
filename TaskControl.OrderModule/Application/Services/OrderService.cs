using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.DAL.Repositories;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.Domain;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.Domain;
using TaskControl.OrderModule.Application.DTOs;
using TaskControl.OrderModule.Application.Interface;
using TaskControl.OrderModule.DataAccess.Interface;
using TaskControl.OrderModule.Domain;


namespace TaskControl.OrderModule.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly ILogger<OrderService> _logger;
        private readonly IOrderPositionRepository _positionRepository;
        private readonly IEnumerable<IOrderCreatedEventHandler> _orderCreatedHandlers;
        private readonly IPostamatAllocationService _postamatAllocationService;
        private readonly IItemAllocationService _itemAllocationService;
        private readonly IItemRepository _itemRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly AppSettings _appSettings;

        public OrderService(
            IOrderRepository repository,
            ILogger<OrderService> logger,
            IOptions<AppSettings> options,
            IOrderPositionRepository positionRepository,
            IPostamatAllocationService postamatAllocationService,
            IItemRepository itemRepository,
            IBranchRepository branchRepository,
            IItemAllocationService itemAllocationService,
            IEnumerable<IOrderCreatedEventHandler> orderCreatedHandlers)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
            _positionRepository = positionRepository;
            _orderCreatedHandlers = orderCreatedHandlers;
            _postamatAllocationService = postamatAllocationService;
            _itemRepository = itemRepository;
            _branchRepository = branchRepository;
            _itemAllocationService = itemAllocationService;
        }

        public async Task<IEnumerable<OrderDto>> GetByCustomerAsync(int customerId)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetByCustomerAsync для клиента {CustomerId}", customerId);
            }

            try
            {
                // Вызываем уже готовый метод из репозитория
                var orders = await _repository.GetByCustomerAsync(customerId);

                // Маппим доменные модели в DTO для отправки на клиент
                var result = orders.Select(OrderDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} заказов для клиента {CustomerId}", result.Count, customerId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка заказов для клиента {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<int> Add(OrderDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для заказа клиента {CustomerId}", dto.CustomerId);
                _logger.LogDebug("Параметры заказа: Тип доставки={DeliveryType}, Постамат={PostamatId}, Количество позиций={PosCount}",
                    dto.DeliveryType, dto.PostamatId, dto.Positions?.Count() ?? 0);
            }

            _logger.LogInformation("Начало процесса создания заказа для клиента {CustomerId}, тип: {Type}", dto.CustomerId, dto.DeliveryType);

            try
            {
                if (dto.Positions == null || !dto.Positions.Any())
                {
                    _logger.LogWarning("Попытка создания пустого заказа для клиента {CustomerId}", dto.CustomerId);
                    throw new ArgumentException("Заказ не может быть пустым.");
                }

                // 1. ПОДМЕНА АДРЕСА ДЛЯ САМОВЫВОЗА И ЭКСПРЕССА
                if (dto.DestinationAddress is null || dto.DeliveryType == DeliveryType.Pickup || dto.DeliveryType == DeliveryType.Express)
                {
                    var branch = await _branchRepository.GetByIdAsync(dto.BranchId);
                    dto.DestinationAddress = branch?.Address ?? "Адрес филиала не указан";
                }

                if (dto.DeliveryType != DeliveryType.Delivery && dto.DeliveryType != DeliveryType.Postamat)
                {
                    dto.DestinationAddress = null;
                }


                // ====================================================================
                // 1. ВАЛИДАЦИЯ ТОВАРОВ И РАСЧЕТ СТОИМОСТИ (ФИКСАЦИЯ ЦЕНЫ)
                // ====================================================================
                decimal totalOrderPrice = 0;
                var validPositions = new List<(OrderPositionDto dto, Item item)>();

                foreach (var posDto in dto.Positions)
                {
                    // Подтягиваем товар из актуального справочника (с актуальной ценой)
                    var item = await _itemRepository.GetByIdAsync(posDto.ItemId);
                    if (item == null)
                    {
                        _logger.LogError("Товар {ItemId} не найден в справочнике при оформлении заказа", posDto.ItemId);
                        throw new Exception($"Товар {posDto.ItemId} не найден.");
                    }

                    validPositions.Add((posDto, item));

                    // Рассчитываем итоговую сумму (Цена товара * Количество)
                    totalOrderPrice += item.Price * posDto.Quantity;
                }

                var entity = OrderDto.FromDto(dto);
                entity.Status = OrderStatus.Created;
                entity.CreatedAt = DateTime.UtcNow;
                entity.TotalPrice = totalOrderPrice; // <-- ФИКСИРУЕМ ОБЩУЮ СТОИМОСТЬ ЗАКАЗА

                // ====================================================================
                // 2. УМНОЕ РАСПРЕДЕЛЕНИЕ ДЛЯ ПОСТАМАТОВ
                // ====================================================================
                if (dto.DeliveryType == DeliveryType.Postamat)
                {
                    if (!dto.PostamatId.HasValue)
                    {
                        _logger.LogWarning("Для типа доставки Postamat не указан ID постамата. Клиент: {CustomerId}", dto.CustomerId);
                        throw new ArgumentException("Не выбран постамат для доставки.");
                    }

                    // Используем уже загруженные товары (validPositions), чтобы не обращаться к БД повторно
                    var itemsToPack = validPositions.Select(vp => new ItemToPack
                    {
                        ItemId = vp.item.ItemId,
                        Length = vp.item.Length.Millimeters,
                        Width = vp.item.Width.Millimeters,
                        Height = vp.item.Height.Millimeters,
                        Weight = vp.item.Weight.Grams,
                        Quantity = vp.dto.Quantity
                    }).ToList();

                    _logger.LogDebug("Запуск алгоритма упаковки для постамата {PostamatId}", dto.PostamatId);
                    int cellId = await _postamatAllocationService.ReservePostamatCellAsync(dto.PostamatId.Value, itemsToPack);

                    if (cellId == -1)
                    {
                        _logger.LogWarning("Отказ в создании заказа: в постамате {Id} нет места под габариты заказа клиента {CustomerId}",
                            dto.PostamatId, dto.CustomerId);
                        throw new InvalidOperationException("В выбранном постамате нет свободных ячеек, подходящих под габариты заказа.");
                    }

                    _logger.LogInformation("Зарезервирована ячейка {CellId} в постамате {PostamatId}", cellId, dto.PostamatId);
                    entity.PostamatId = dto.PostamatId;
                    entity.PostamatCellId = cellId;
                }

                // ====================================================================
                // 3. СОХРАНЕНИЕ ЗАКАЗА И ПОЗИЦИЙ
                // ====================================================================
                int orderId = await _repository.AddAsync(entity);

                foreach (var vp in validPositions)
                {
                    var pos = new OrderPosition
                    {
                        OrderId = orderId,
                        ItemId = vp.dto.ItemId,
                        Quantity = vp.dto.Quantity,
                        Price = vp.item.Price // <-- ФИКСИРУЕМ ЦЕНУ ЗА 1 ШТ. НА МОМЕНТ ПОКУПКИ
                    };

                    int orderPositionId = await _positionRepository.AddAsync(pos);

                    bool isAllocated = await _itemAllocationService.HardAllocateOrderItemsAsync(
                        dto.BranchId,
                        orderPositionId,
                        vp.dto.ItemId,
                        vp.dto.Quantity
                    );

                    if (!isAllocated)
                    {
                        throw new InvalidOperationException($"Критическая ошибка: Товар {vp.dto.ItemId} закончился на складе в процессе оформления.");
                    }
                }

                // ====================================================================
                // 4. ВЫЗОВ СОБЫТИЙ
                // ====================================================================
                if (_orderCreatedHandlers != null)
                {
                    foreach (var handler in _orderCreatedHandlers)
                    {
                        await handler.HandleOrderCreatedAsync(orderId, dto.BranchId);
                    }
                }

                _logger.LogInformation("Заказ успешно создан. ID: {OrderId}, Сумма: {TotalPrice}, Клиент: {CustomerId}",
                    orderId, totalOrderPrice, dto.CustomerId);

                return orderId;
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Критическая ошибка при создании заказа для клиента {CustomerId}", dto.CustomerId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Delete для заказа");
                _logger.LogDebug("Удаление заказа ID: {OrderId}", id);
            }
            _logger.LogInformation("Удаление заказа ID: {OrderId}", id);

            try
            {
                var result = await _repository.DeleteAsync(id) == 1;
                if (result)
                {
                    _logger.LogInformation("Заказ ID: {OrderId} удален", id);
                }
                else
                {
                    _logger.LogWarning("Заказ ID: {OrderId} не найден", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления заказа ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetAll()
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetAll для заказов");
                _logger.LogDebug("Получение всех заказов");
            }
            _logger.LogInformation("Запрос всех заказов");

            try
            {
                var orders = await _repository.GetAllAsync();
                var result = orders.Select(OrderDto.ToDto).ToList();

                _logger.LogInformation("Получено {Count} заказов", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка заказов");
                throw;
            }
        }

        public async Task<OrderDto?> GetById(int id)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры GetById для заказа");
                _logger.LogDebug("Получение заказа ID: {OrderId}", id);
            }
            _logger.LogInformation("Запрос заказа ID: {OrderId}", id);

            try
            {
                // 1. Получаем доменную модель заказа
                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Заказ ID: {OrderId} не найден", id);
                    return null;
                }

                // 2. Маппим базовые свойства в DTO
                var dto = OrderDto.ToDto(order);

                // 3. Получаем позиции заказа через репозиторий позиций
                var positions = await _positionRepository.GetByOrderIdAsync(id);

                // 4. Обогащаем DTO позициями
                if (positions != null && positions.Any())
                {
                    var enrichedPositions = new List<OrderPositionDto>();

                    foreach (var pos in positions)
                    {
                        var posDto = OrderPositionDto.ToDto(pos);

                        // Запрашиваем товар из справочника через уже готовый репозиторий
                        var item = await _itemRepository.GetByIdAsync(pos.ItemId);

                        // Прописываем читаемое имя
                        posDto.ItemName = item?.Name ?? "Неизвестный товар";

                        enrichedPositions.Add(posDto);
                    }

                    dto.Positions = enrichedPositions;
                }
                else
                {
                    dto.Positions = new List<OrderPositionDto>();
                }

                _logger.LogInformation("Заказ ID: {OrderId} успешно получен с позициями", id);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения заказа ID: {OrderId}", id);
                throw;
            }
        }
        public async Task<bool> Update(OrderDto dto)
        {
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Update для заказа");
                _logger.LogDebug("Обновление заказа ID: {OrderId}", dto.OrderId);
            }
            _logger.LogInformation("Обновление заказа ID: {OrderId}", dto.OrderId);

            try
            {
                var entity = OrderDto.FromDto(dto);
                var result = await _repository.UpdateAsync(entity) == 1;

                if (result)
                {
                    _logger.LogInformation("Заказ ID: {OrderId} обновлен", dto.OrderId);
                }
                else
                {
                    _logger.LogWarning("Заказ ID: {OrderId} не найден", dto.OrderId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления заказа ID: {OrderId}", dto.OrderId);
                throw;
            }
        }
    }
}