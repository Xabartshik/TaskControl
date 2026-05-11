using CsvHelper;
using CsvHelper.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TaskControl.ReportsModule.Application.DTOs;

namespace TaskControl.ReportsModule.Application.Services
{
    public class ReportExportService
    {
        public ReportExportService()
        {
            // Для использования QuestPDF необходимо указать тип лицензии
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] ExportToCsv<T>(IEnumerable<T> data)
        {
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms, new System.Text.UTF8Encoding(true));
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };
            using var csv = new CsvWriter(sw, config);

            csv.WriteRecords(data);
            sw.Flush();
            return ms.ToArray();
        }

        // 1. ОТЧЕТ ПО KPI СОТРУДНИКОВ (С графиками нагрузки и тоннажом)
        public byte[] GenerateEmployeeKpiPdf(IEnumerable<EmployeeKpiDto> data, string title = "Глубокая аналитика: Эффективность и нагрузка сотрудников")
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); // Широкий формат для максимума инфы
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, title));
                    page.Content().Element(c => ComposeKpiTable(c, data));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        // 2. ОТЧЕТ ПО ЗАКАЗАМ (С детализацией состава, весом и кубатурой)
        public byte[] GenerateDetailedOrdersPdf(IEnumerable<OrderDetailedDto> data, string title = "Детализированный отчет по управлению заказами")
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, title));
                    page.Content().Element(c => ComposeDetailedOrdersTable(c, data));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        // 3. ОТЧЕТ ПО ТОВАРАМ (С графиками популярности)
        public byte[] GenerateTopItemsPdf(IEnumerable<TopItemDto> data, string title = "Аналитика: Топ востребованных товаров")
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Portrait());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, title));
                    page.Content().Element(c => ComposeTopItemsChart(c, data));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        #region Вспомогательные методы верстки (QuestPDF)

        private void ComposeHeader(IContainer container, string title)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(title).FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Сгенерировано системой управления: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });
        }

        // Верстка таблицы KPI
        private void ComposeKpiTable(IContainer container, IEnumerable<EmployeeKpiDto> data)
        {
            var dataList = data.ToList();
            float maxComplexity = dataList.Any() ? dataList.Max(x => x.TotalComplexity) : 1;

            container.PaddingVertical(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // #
                    columns.RelativeColumn(3);  // ФИО
                    columns.RelativeColumn(3);  // График Нагрузки
                    columns.RelativeColumn(2);  // Задач закрыто
                    columns.RelativeColumn(2);  // Ср. время
                    columns.RelativeColumn(2);  // Вес (кг)
                    columns.RelativeColumn(2);  // Объем (м3)
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#").SemiBold();
                    header.Cell().Element(CellStyle).Text("Сотрудник").SemiBold();
                    header.Cell().Element(CellStyle).Text("Уровень нагрузки (Complexity)").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Выполнено").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Ср. время").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Перенесено (кг)").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Объем (м³)").SemiBold();

                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                });

                int index = 1;
                foreach (var item in dataList)
                {
                    var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                    float fillWeight = maxComplexity > 0 ? ((float)item.TotalComplexity / maxComplexity) : 0;
                    float emptyWeight = 1f - fillWeight;

                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text(index.ToString());
                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text($"{item.FullName}\n({item.BranchName})").FontSize(9);

                    // ГРАФИК НАГРУЗКИ
                    table.Cell().Element(c => ItemStyle(c, bgColor)).AlignMiddle().Row(row =>
                    {
                        row.RelativeItem().AlignMiddle().Row(barRow =>
                        {
                            if (fillWeight > 0)
                                barRow.RelativeItem(fillWeight).Height(10)
                                      .Background(fillWeight > 0.8f ? Colors.Red.Lighten1 : Colors.Green.Medium); // Красный, если перегружен

                            if (emptyWeight > 0)
                                barRow.RelativeItem(emptyWeight).Height(10).Background(Colors.Grey.Lighten3);
                        });
                        row.ConstantItem(30).AlignRight().Text(item.TotalComplexity.ToString()).SemiBold().FontSize(9);
                    });

                    table.Cell().Element(c => ItemStyle(c, bgColor)).AlignRight().Text(item.TasksCompleted.ToString());
                    table.Cell().Element(c => ItemStyle(c, bgColor)).AlignRight().Text(item.AverageTaskDuration);

                    // Выделяем вес > 1 тонны
                    var weightCell = table.Cell().Element(c => ItemStyle(c, bgColor)).AlignRight();
                    if (item.TotalWeightMovedKg > 1000)
                        weightCell.Text(item.TotalWeightMovedKg.ToString("F2")).FontColor(Colors.Red.Medium).SemiBold();
                    else
                        weightCell.Text(item.TotalWeightMovedKg.ToString("F2"));

                    table.Cell().Element(c => ItemStyle(c, bgColor)).AlignRight().Text(item.TotalVolumeMovedCubicMeters.ToString("F4"));
                    index++;
                }

                static IContainer ItemStyle(IContainer container, string bgColor) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Background(bgColor).PaddingVertical(6).PaddingHorizontal(2);
            });
        }

        // Верстка детализации заказов
        private void ComposeDetailedOrdersTable(IContainer container, IEnumerable<OrderDetailedDto> data)
        {
            container.PaddingVertical(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);   // ID
                    columns.RelativeColumn(1.5f); // Дата
                    columns.RelativeColumn(5);    // Состав заказа
                    columns.RelativeColumn(1.5f); // Статус
                    columns.RelativeColumn(1.2f); // Вес
                    columns.RelativeColumn(1.2f); // Объем
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("ID").SemiBold();
                    header.Cell().Element(CellStyle).Text("Дата создания").SemiBold();
                    header.Cell().Element(CellStyle).Text("Состав заказа (Номенклатура и кол-во)").SemiBold();
                    header.Cell().Element(CellStyle).Text("Статус").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Вес (кг)").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Объем (м³)").SemiBold();

                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                });

                int index = 1;
                foreach (var item in data)
                {
                    var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text($"#{item.OrderId}");
                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text(item.CreatedAt.ToString("dd.MM.yyyy HH:mm")).FontSize(9);

                    // Вывод списка товаров
                    var itemsText = string.IsNullOrEmpty(item.ItemsList) ? "Корзина пуста" : item.ItemsList;
                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text(itemsText).FontSize(9).Italic().FontColor(Colors.Grey.Darken3);

                    var statusColor = item.StatusRaw == "Completed" ? Colors.Green.Darken1 :
                                      item.StatusRaw == "Cancelled" || item.StatusRaw == "Rejected" ? Colors.Red.Darken1 : Colors.Black;
                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text(item.Status).FontColor(statusColor).SemiBold().FontSize(9);

                    table.Cell().Element(c => ItemStyle(c, bgColor)).AlignRight().Text(item.TotalWeightKg.ToString("F2")).FontSize(9);
                    table.Cell().Element(c => ItemStyle(c, bgColor)).AlignRight().Text(item.TotalVolumeCubicMeters.ToString("F4")).FontSize(9);

                    index++;
                }

                static IContainer ItemStyle(IContainer container, string bgColor) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Background(bgColor).PaddingVertical(6).PaddingHorizontal(4);
            });
        }

        // Добавьте этот метод в ReportExportService.cs для исправления ошибки компиляции
        public byte[] GenerateOrderLeadTimePdf(IEnumerable<OrderLeadTimeDto> data, string title = "Аналитика времени выполнения заказов (Lead Time)")
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, title));

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // ID
                            columns.RelativeColumn(2);   // Филиал
                            columns.RelativeColumn(2);   // Создан
                            columns.RelativeColumn(2);   // Выдан
                            columns.RelativeColumn(1.5f); // Тип
                            columns.RelativeColumn(1.5f); // Статус
                            columns.RelativeColumn(2);   // Lead Time (График)
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("ID");
                            header.Cell().Element(CellStyle).Text("Филиал");
                            header.Cell().Element(CellStyle).Text("Создан");
                            header.Cell().Element(CellStyle).Text("Завершен");
                            header.Cell().Element(CellStyle).Text("Тип");
                            header.Cell().Element(CellStyle).Text("Статус");
                            header.Cell().Element(CellStyle).AlignRight().Text("Lead Time");
                            static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1);
                        });

                        foreach (var item in data)
                        {
                            table.Cell().PaddingVertical(5).Text($"#{item.OrderId}");
                            table.Cell().PaddingVertical(5).Text(item.BranchName);
                            table.Cell().PaddingVertical(5).Text(item.CreatedAt.ToString("g"));
                            table.Cell().PaddingVertical(5).Text(item.DeliveryDate?.ToString("g") ?? "---");
                            table.Cell().PaddingVertical(5).Text(item.DeliveryType);
                            table.Cell().PaddingVertical(5).Text(item.Status);

                            // ВИЗУАЛИЗАЦИЯ ВРЕМЕНИ (Если заказ шел дольше 2 часов - подсвечиваем риск)
                            table.Cell().PaddingVertical(5).AlignRight().Column(col => {
                                col.Item().Text(item.LeadTimeFormatted).SemiBold();
                                if (item.LeadTimeMinutes > 120) // Пример порога в 2 часа
                                    col.Item().Text("Задержка").FontSize(8).FontColor(Colors.Red.Medium);
                            });
                        }
                    });
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        // Верстка популярных товаров
        private void ComposeTopItemsChart(IContainer container, IEnumerable<TopItemDto> data)
        {
            var dataList = data.ToList();
            if (!dataList.Any()) return;

            float maxQuantity = dataList.Max(x => x.QuantitySold);

            container.PaddingVertical(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // #
                    columns.RelativeColumn(3);   // Товар
                    columns.RelativeColumn(4);   // График продаж
                    columns.RelativeColumn(1);   // Кол-во
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#").SemiBold();
                    header.Cell().Element(CellStyle).Text("Наименование товара").SemiBold();
                    header.Cell().Element(CellStyle).Text("Объем продаж (Популярность)").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Шт.").SemiBold();

                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                });

                int index = 1;
                foreach (var item in dataList)
                {
                    var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                    float fillWeight = maxQuantity > 0 ? ((float)item.QuantitySold / maxQuantity) : 0;
                    float emptyWeight = 1f - fillWeight;

                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text(index.ToString());
                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text(item.ItemName);

                    // ГРАФИК ПРОДАЖ
                    table.Cell().Element(c => ItemStyle(c, bgColor)).AlignMiddle().Row(row =>
                    {
                        if (fillWeight > 0)
                            row.RelativeItem(fillWeight).Height(12).Background(Colors.Blue.Medium);

                        if (emptyWeight > 0)
                            row.RelativeItem(emptyWeight).Height(12).Background(Colors.Grey.Lighten3);
                    });

                    table.Cell().Element(c => ItemStyle(c, bgColor)).AlignRight().Text(item.QuantitySold.ToString()).SemiBold();
                    index++;
                }

                static IContainer ItemStyle(IContainer container, string bgColor) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Background(bgColor).PaddingVertical(6).PaddingHorizontal(2);
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Страница ");
                x.CurrentPageNumber();
                x.Span(" из ");
                x.TotalPages();
            });
        }
        #endregion
    }
}