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

            // Клонируем список, чтобы безопасно вычитать упакованное количество и преобразуем товары во внутренний формат для работы с остатками
            var remainingItems = items.Select(i => new ItemToPack
            {
                OrderPositionId = i.OrderPositionId,
                ItemId = i.ItemId,
                Length = i.Length,
                Width = i.Width,
                Height = i.Height,
                Quantity = i.Quantity
            }).ToList();

            foreach (var cell in cells)
            {
                // Если больше нечего паковать — прерываем цикл
                if (!remainingItems.Any(r => r.Quantity > 0)) break;

                // Размеры ячейки (контейнера). Если у ячейки нет размеров — пропускаем.
                if (cell.Length <= 0 || cell.Width <= 0 || cell.Height <= 0) continue;

                var container = new Container(
                    cell.PositionId,
                    (decimal)cell.Length,
                    (decimal)cell.Width,
                    (decimal)cell.Height
                );

                // Товары, которые ещё не распределены (передаем библиотеке актуальные остатки)
                var packingItems = remainingItems
                    .Where(i => i.Quantity > 0)
                    .Select(i => new Item(
                        i.OrderPositionId,
                        (decimal)i.Length,
                        (decimal)i.Width,
                        (decimal)i.Height,
                        i.Quantity))
                    .ToList();

                var algorithms = new List<int> { (int)AlgorithmType.EB_AFIT };

                // Выполняем расчет упаковки для текущей ячейки
                var packingResults = PackingService.Pack(new List<Container> { container }, packingItems, algorithms);

                // Обрабатываем результат для текущей ячейки
                var containerResult = packingResults.FirstOrDefault();
                if (containerResult == null) continue;

                var algoResult = containerResult.AlgorithmPackingResults.FirstOrDefault();
                if (algoResult == null) continue;

                // Библиотека возвращает упакованные предметы поштучно. Группируем их по OrderPositionId.
                var packedGroups = algoResult.PackedItems.GroupBy(p => p.ID);

                foreach (var group in packedGroups)
                {
                    int packedQty = group.Count(); // Количество штук, влезших в эту ячейку
                    int orderPosId = group.Key;

                    // Записываем результат: часть позиции упакована в эту конкретную ячейку
                    result.PackedItems.Add(new PackedItemResult
                    {
                        OrderPositionId = orderPosId,
                        TargetPositionId = cell.PositionId,
                        Quantity = packedQty
                    });

                    // Уменьшаем количество товаров, которые осталось распределить
                    var remItem = remainingItems.First(r => r.OrderPositionId == orderPosId);
                    remItem.Quantity -= packedQty;
                }

                // Убираем из списка те позиции, которые упакованы полностью
                remainingItems.RemoveAll(r => r.Quantity <= 0);
            }

            // Заказ считается успешно распределенным, если не осталось неупакованных товаров
            result.IsFullyPacked = !remainingItems.Any(r => r.Quantity > 0);

            return result;
        }
    }
}