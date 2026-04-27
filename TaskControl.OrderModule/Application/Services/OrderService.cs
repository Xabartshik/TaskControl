using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.Domain;
using TaskControl.OrderModule.Application.DTOs;
using TaskControl.OrderModule.Application.Interface;
using TaskControl.OrderModule.DataAccess.Interface;
using TaskControl.OrderModule.Domain;
using TaskControl.OrderModule.Domain.TaskControl.OrderModule.Domain.Enums;

namespace TaskControl.OrderModule.Application.Services
{
    public class OrderService : IService<OrderDto>
    {
        private readonly IOrderRepository _repository;
        private readonly ILogger<OrderService> _logger;
        private readonly IOrderPositionRepository _positionRepository;
        private readonly IEnumerable<IOrderCreatedEventHandler> _orderCreatedHandlers;
        private readonly IInventoryAllocationService _allocationService;
        private readonly IItemRepository _itemRepository;
        private readonly AppSettings _appSettings;

        public OrderService(
            IOrderRepository repository,
            ILogger<OrderService> logger,
            IOptions<AppSettings> options,
            IOrderPositionRepository positionRepository,
            IInventoryAllocationService allocationService,
            IEnumerable<IOrderCreatedEventHandler> orderCreatedHandlers)
        {
            _repository = repository;
            _logger = logger;
            _appSettings = options.Value;
            _positionRepository = positionRepository;
            _orderCreatedHandlers = orderCreatedHandlers;
            _allocationService = allocationService;
        }

        public async Task<int> Add(OrderDto dto)
        {
            // 1. Детальное логгирование (Trace/Debug) на входе
            if (_appSettings.EnableDetailedLogging)
            {
                _logger.LogTrace("Вызов процедуры Add для заказа клиента {CustomerId}", dto.CustomerId);
                _logger.LogDebug("Параметры заказа: Тип доставки={DeliveryType}, Постамат={PostamatId}, Количество позиций={PosCount}",
                    dto.DeliveryType, dto.PostamatId, dto.Positions?.Count() ?? 0);
            }

            // 2. Информационное логгирование начала процесса
            _logger.LogInformation("Начало процесса создания заказа для клиента {CustomerId}, тип: {Type}", dto.CustomerId, dto.DeliveryType);

            try
            {
                // 1. Валидация состава
                if (dto.Positions == null || !dto.Positions.Any())
                {
                    _logger.LogWarning("Попытка создания пустого заказа для клиента {CustomerId}", dto.CustomerId);
                    throw new ArgumentException("Заказ не может быть пустым.");
                }

                // 2. Обработка адреса: если не доставка (курьер/постамат), адрес = null (филиал)
                if (dto.DeliveryType != DeliveryType.Delivery && dto.DeliveryType != DeliveryType.Postamat)
                {
                    dto.DestinationAddress = null;
                }

                var entity = OrderDto.FromDto(dto);
                entity.Status = OrderStatus.Created;
                entity.CreatedAt = DateTime.UtcNow;

                // 3. Умное распределение для постаматов
                if (dto.DeliveryType == DeliveryType.Postamat)
                {
                    if (!dto.PostamatId.HasValue)
                    {
                        _logger.LogWarning("Для типа доставки Postamat не указан ID постамата. Клиент: {CustomerId}", dto.CustomerId);
                        throw new ArgumentException("Не выбран постамат для доставки.");
                    }

                    // Подготовка данных для алгоритма упаковки
                    var itemsToPack = new List<ItemToPack>();
                    foreach (var posDto in dto.Positions)
                    {
                        var item = await _itemRepository.GetByIdAsync(posDto.ItemId);
                        if (item == null)
                        {
                            _logger.LogError("Товар {ItemId} не найден в справочнике при оформлении заказа", posDto.ItemId);
                            throw new Exception($"Товар {posDto.ItemId} не найден.");
                        }

                        itemsToPack.Add(new ItemToPack
                        {
                            ItemId = item.ItemId,
                            Length = item.Length.Millimeters,
                            Width = item.Width.Millimeters,
                            Height = item.Height.Millimeters,
                            Quantity = posDto.Quantity
                        });
                    }

                    // Поиск ячейки через Bin Packing
                    _logger.LogDebug("Запуск алгоритма упаковки для постамата {PostamatId}", dto.PostamatId);
                    int cellId = await _allocationService.ReservePostamatCellAsync(dto.PostamatId.Value, itemsToPack);

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

                // 4. Сохранение заказа
                int orderId = await _repository.AddAsync(entity);

                // 5. Сохранение позиций
                foreach (var pDto in dto.Positions)
                {
                    var pos = new OrderPosition
                    {
                        OrderId = orderId,
                        ItemId = pDto.ItemId,
                        Quantity = pDto.Quantity
                    };

                    // Сохраняем позицию заказа и получаем ее ID
                    int orderPositionId = await _positionRepository.AddAsync(pos);

                    // Сразу же выполняем ЖЕСТКУЮ АЛЛОКАЦИЮ через инвентарный сервис
                    bool isAllocated = await _allocationService.HardAllocateOrderItemsAsync(
                        dto.BranchId,
                        orderPositionId,
                        pDto.ItemId,
                        pDto.Quantity
                    );

                    if (!isAllocated)
                    {
                        // Если кто-то успел купить товар за те 2 секунды, пока клиент оформлял заказ
                        throw new InvalidOperationException($"Критическая ошибка: Товар {pDto.ItemId} закончился на складе в процессе оформления.");
                    }
                }

                // 6. Вызов событий (обработчиков)
                if (_orderCreatedHandlers != null)
                {
                    foreach (var handler in _orderCreatedHandlers)
                    {
                        // Используем HandleOrderCreatedAsync согласно структуре проекта
                        await handler.HandleOrderCreatedAsync(orderId, dto.BranchId);
                    }
                }

                _logger.LogInformation("Заказ успешно создан. ID: {OrderId}, Клиент: {CustomerId}", orderId, dto.CustomerId);
                return orderId;
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not InvalidOperationException)
            {
                // Логгируем только непредвиденные системные ошибки (критические)
                _logger.LogError(ex, "Критическая ошибка при создании заказа для клиента {CustomerId}", dto.CustomerId);
                throw;
            }
            catch (Exception)
            {
                // Бизнес-исключения пробрасываем дальше (они уже могли быть частично логированы как Warning)
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
                var order = await _repository.GetByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Заказ ID: {OrderId} не найден", id);
                    return null;
                }

                _logger.LogInformation("Заказ ID: {OrderId} получен", id);
                return OrderDto.ToDto(order);
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