using LinqToDB;
using Microsoft.Extensions.Logging;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.InventoryModule.DataAccess.Model;

public class CourierInventoryHandler : ICourierCreatedEventHandler
{
    private readonly IInventoryDataConnection _db;
    private readonly ILogger<CourierInventoryHandler> _logger;

    public CourierInventoryHandler(IInventoryDataConnection db, ILogger<CourierInventoryHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task HandleCourierCreatedAsync(int employeeId, int defaultBranchId, double length, double width, double height)
    {
        // 1. Проверяем, нет ли уже ячейки
        var existingCell = await _db.GetTable<PositionModel>()
            .FirstOrDefaultAsync(p => p.ZoneCode == "COURIER" && p.FLSNumber == employeeId.ToString());

        if (existingCell == null)
        {
            // 2. Создаем виртуальную ячейку
            var courierCell = new PositionModel
            {
                BranchId = defaultBranchId,
                Status = "Active",
                ZoneCode = "COURIER",
                FirstLevelStorageType = "Vehicle",
                FLSNumber = employeeId.ToString(),
                Length = length,
                Width = width,
                Height = height,
                CreatedAt = DateTime.UtcNow
            };

            await _db.InsertAsync(courierCell);
            _logger.LogInformation("Создана виртуальная складская ячейка для курьера ID: {Id}", employeeId);
        }
    }
}