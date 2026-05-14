using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.DataAccess.Interface;

namespace TaskControl.InventoryModule.Application.Handlers
{
    public class BranchCreatedPositionHandler : IBranchCreatedHandler
    {
        private readonly IPositionCellRepository _positionRepository;
        private readonly ILogger<BranchCreatedPositionHandler> _logger;

        public BranchCreatedPositionHandler(
            IPositionCellRepository positionRepository,
            ILogger<BranchCreatedPositionHandler> logger)
        {
            _positionRepository = positionRepository;
            _logger = logger;
        }

        public async Task OnBranchCreatedAsync(int branchId)
        {
            _logger.LogInformation("Создание системных ячеек для нового филиала {BranchId}", branchId);

            // Список системных позиций для создания
            var systemPositions = new List<PositionCellDto>
            {
                // 1. EXPRESS — зона прямой выдачи
                new() {
                    BranchId = branchId,
                    ZoneCode = "EXPRESS",
                    FirstLevelStorageType = "Зона выдачи",
                    FLSNumber = "1",
                    Status = "Active",
                    Width = 999999,  // Добавляем размеры, чтобы пройти Check Constraint
                    Length = 999999,
                    Height = 999999
                },
                // 2. BULK — куча для крупногабарита
                new() {
                    BranchId = branchId,
                    ZoneCode = "BULK",
                    FirstLevelStorageType = "Куча",
                    FLSNumber = "1",
                    Status = "Active",
                    Width = 999999, 
                    Length = 999999,
                    Height = 999999
                }
            };

            foreach (var pos in systemPositions)
            {
                await _positionRepository.AddAsync(PositionCellDto.FromDto(pos)); // Используй свой маппер или метод репозитория
            }

            _logger.LogInformation("Системные ячейки для филиала {BranchId} успешно созданы", branchId);
        }
    }
}