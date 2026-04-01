using System;
using System.Collections.Generic;
using System.Linq;
using CromulentBisgetti.ContainerPacking;
using CromulentBisgetti.ContainerPacking.Entities;
using CromulentBisgetti.ContainerPacking.Algorithms;

namespace TaskControl.TaskModule.Application.Services
{
    /// <summary>
    /// Реализация 3D-упаковки товаров в ячейки PICKUP с использованием алгоритма EB-AFIT.
    /// Для каждой ячейки проверяется, какие товары в неё помещаются.
    /// Если одной ячейки недостаточно — товары распределяются по нескольким ячейкам.
    /// </summary>
    public class BoxPackingService : IBoxPackingService
    {
        public PackingResult AssignItemsToPickupCells(List<ItemToPack> items, List<CellToPackInto> cells)
        {
            var result = new PackingResult();
            if (cells.Count == 0 || items.Count == 0) return result;

            // Преобразуем наши товары в формат библиотеки
            var remainingItems = items.ToList();

            foreach (var cell in cells)
            {
                if (remainingItems.Count == 0) break;

                // Размеры ячейки (контейнера)
                // Если у ячейки нет размеров — пропускаем
                if (cell.Length <= 0 || cell.Width <= 0 || cell.Height <= 0) continue;

                var container = new Container(
                    cell.PositionId,
                    (decimal)cell.Length,
                    (decimal)cell.Width,
                    (decimal)cell.Height
                );

                // Товары, которые ещё не распределены
                var packingItems = remainingItems.Select(i => new Item(
                    i.OrderPositionId,
                    (decimal)i.Length,
                    (decimal)i.Width,
                    (decimal)i.Height,
                    1 // количество = 1, каждый отдельный экземпляр товара
                )).ToList();

                var algorithms = new List<int> { (int)AlgorithmType.EB_AFIT };

                var packingResults = PackingService.Pack(
                    new List<Container> { container },
                    packingItems,
                    algorithms
                );

                // Обрабатываем результат для текущей ячейки
                var containerResult = packingResults.FirstOrDefault();
                if (containerResult == null) continue;

                var algoResult = containerResult.AlgorithmPackingResults.FirstOrDefault();
                if (algoResult == null) continue;

                // Товары, успешно помещённые в эту ячейку
                foreach (var packed in algoResult.PackedItems)
                {
                    result.ItemToCellMap[packed.ID] = cell.PositionId;
                    remainingItems.RemoveAll(r => r.OrderPositionId == packed.ID);
                }
            }

            return result;
        }
    }
}
