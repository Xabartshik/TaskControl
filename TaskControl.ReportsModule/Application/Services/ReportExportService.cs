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
        public byte[] GenerateEmployeeKpiPdf(IEnumerable<EmployeeKpiDto> data, string title = "Глубокая аналитика: Эффективность и нагрузка сотрудников", DateTime? startDate = null, DateTime? endDate = null)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); // Широкий формат для максимума инфы
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, title, startDate, endDate));
                    page.Content().Element(c => ComposeKpiTable(c, data));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        // 2. ОТЧЕТ ПО ЗАКАЗАМ (С детализацией состава, весом и кубатурой)
        public byte[] GenerateDetailedOrdersPdf(IEnumerable<OrderDetailedDto> data, string title = "Детализированный отчет по управлению заказами", DateTime? startDate = null, DateTime? endDate = null)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, title, startDate, endDate));
                    page.Content().Element(c => ComposeDetailedOrdersTable(c, data));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        // 3. ОТЧЕТ ПО ТОВАРАМ (С графиками популярности)
        public byte[] GenerateTopItemsPdf(IEnumerable<TopItemDto> data, string title = "Аналитика: Топ востребованных товаров", DateTime? startDate = null, DateTime? endDate = null)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Portrait());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, title, startDate, endDate));
                    page.Content().Element(c => ComposeTopItemsChart(c, data));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        #region Вспомогательные методы верстки (QuestPDF)

        private void ComposeHeader(IContainer container, string title, DateTime? startDate = null, DateTime? endDate = null)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(title).FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);

                    // Вывод охватываемого периода
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        column.Item().Text($"Отчетный период: с {startDate.Value:dd.MM.yyyy} по {endDate.Value:dd.MM.yyyy}")
                            .FontSize(10).Italic().FontColor(Colors.Grey.Darken2);
                    }

                    column.Item().Text($"Дата генерации: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
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

        public byte[] GenerateOrderLeadTimePdf(IEnumerable<OrderLeadTimeDto> data, string title = "Аналитика времени выполнения заказов (Lead Time)", DateTime? startDate = null, DateTime? endDate = null)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, title, startDate, endDate));

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

        public byte[] GenerateEmployeeFullReportPdf(List<EmployeeFullReportDto> data, string title, DateTime startDate, DateTime endDate)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    // Заголовок с периодом
                    page.Header().Element(c => ComposeHeader(c, title, startDate, endDate));

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        // ОБЩИЙ ГРАФИК ЗАГРУЖЕННОСТИ - перед таблицами
                        column.Item().PaddingBottom(15).Element(c => ComposeOverallKpiChart(c, data));

                        column.Item().PaddingBottom(10).Text("Детальные карточки сотрудников").FontSize(12).SemiBold();

                        foreach (var employee in data)
                        {
                            column.Item().Element(c => ComposeEmployeeCard(c, employee));
                            column.Item().PaddingBottom(20);
                        }
                    });

                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        private void ComposeOverallKpiChart(IContainer container, List<EmployeeFullReportDto> data)
        {
            var sortedData = data.OrderByDescending(x => x.TotalComplexity).ToList();
            int maxComplexity = sortedData.Any() ? sortedData.Max(x => x.TotalComplexity) : 1;
            if (maxComplexity <= 0) maxComplexity = 1;

            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
            {
                col.Item().PaddingBottom(5).Text("Сводный график нагрузки сотрудников (Total Complexity)").SemiBold();

                foreach (var emp in sortedData)
                {
                    float fillWeight = (float)emp.TotalComplexity / maxComplexity;
                    float emptyWeight = 1f - fillWeight;

                    col.Item().PaddingVertical(2).Row(row =>
                    {
                        row.ConstantItem(150).Text(emp.FullName).FontSize(8);

                        if (fillWeight > 0)
                            row.RelativeItem(fillWeight).Height(12).Background(fillWeight > 0.8f ? Colors.Red.Medium : Colors.Blue.Medium);

                        if (emptyWeight > 0)
                            row.RelativeItem(emptyWeight).Height(12).Background(Colors.Grey.Lighten4);

                        row.ConstantItem(40).AlignRight().Text(emp.TotalComplexity.ToString()).SemiBold().FontSize(8);
                    });
                }
            });
        }

        private void ComposeEmployeeCard(IContainer container, EmployeeFullReportDto employee)
        {
            container.PaddingBottom(20).Border(1).BorderColor(Colors.Grey.Lighten1).Column(card =>
            {
                // Заголовок с ФИО
                card.Item().Background(Colors.Blue.Darken2).Padding(8).Row(row =>
                {
                    row.RelativeItem().Text($"{employee.FullName}").FontSize(14).SemiBold().FontColor(Colors.White);
                    row.ConstantItem(150).AlignRight().Text(employee.BranchName).FontColor(Colors.White).Italic();
                });

                // ГРАФИК ЗАГРУЖЕННОСТИ
                card.Item().Padding(10).Column(col =>
                {
                    col.Item().Text("Общий показатель нагрузки (Complexity)").SemiBold().FontSize(10);
                    col.Item().PaddingVertical(5).Height(20).Row(row =>
                    {
                        float fillWeight = employee.TotalComplexity > 0 ? (float)employee.TotalComplexity / 500f : 0;
                        if (fillWeight > 1) fillWeight = 1;
                        float emptyWeight = 1f - fillWeight;

                        if (fillWeight > 0)
                            row.RelativeItem(fillWeight).Background(fillWeight > 0.8f ? Colors.Red.Medium : Colors.Green.Medium);

                        if (emptyWeight > 0)
                            row.RelativeItem(emptyWeight).Background(Colors.Grey.Lighten3);
                    });
                    col.Item().AlignRight().Text($"Итоговая сложность: {employee.TotalComplexity}").FontSize(8).Italic();
                });

                // KPI Сводка
                card.Item().PaddingHorizontal(10).PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().Element(c => KpiValue(c, "Задач", employee.TotalTasksCompleted.ToString()));
                    row.RelativeItem().Element(c => KpiValue(c, "Вес (кг)", employee.TotalWeightKg.ToString()));
                    row.RelativeItem().Element(c => KpiValue(c, "Реакция (ср)", employee.AverageReactionTime));
                    row.RelativeItem().Element(c => KpiValue(c, "Работа (ср)", employee.AverageExecutionTime));
                });

                // Таблица задач
                card.Item().Padding(10).Element(c => ComposeEmployeeTasksTable(c, employee.Tasks));
            });

            static void KpiValue(IContainer container, string label, string value) =>
                container.Column(c => { c.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Medium); c.Item().Text(value).SemiBold(); });
        }

        private void ComposeEmployeeTasksTable(IContainer container, List<EmployeeTaskExtendedDto> tasks)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // ID
                    columns.RelativeColumn(2);   // Тип
                    columns.RelativeColumn(4);   // Хронология (Дата + Время)
                    columns.RelativeColumn(1.5f); // Реакция
                    columns.RelativeColumn(1.5f); // Выполнение
                    columns.ConstantColumn(40);  // Сложн.
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("ID");
                    header.Cell().Element(CellStyle).Text("Тип");
                    header.Cell().Element(CellStyle).Text("Назначено -> Принято (Локальное время)");
                    header.Cell().Element(CellStyle).AlignRight().Text("Реакция");
                    header.Cell().Element(CellStyle).AlignRight().Text("Работа");
                    header.Cell().Element(CellStyle).AlignRight().Text("Сложн.");
                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).BorderBottom(1).PaddingBottom(5);
                });

                foreach (var task in tasks)
                {
                    table.Cell().Element(ItemStyle).Text($"#{task.TaskId}");
                    table.Cell().Element(ItemStyle).Text(task.TaskType);

                    var assignedLocal = task.AssignedAt.ToLocalTime().ToString("dd.MM HH:mm:ss");
                    var startedLocal = task.StartedAt?.ToLocalTime().ToString("dd.MM HH:mm:ss") ?? "--:--";

                    table.Cell().Element(ItemStyle).Text($"{assignedLocal} ➔ {startedLocal}").FontSize(8);

                    table.Cell().Element(ItemStyle).AlignRight().Text(task.ReactionTimeFormatted);
                    table.Cell().Element(ItemStyle).AlignRight().Text(task.ExecutionTimeFormatted);
                    table.Cell().Element(ItemStyle).AlignRight().Text(task.Complexity.ToString());
                }

                static IContainer ItemStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(3);
            });
        }

        // 2. ОТЧЕТ: УПРАВЛЕНЧЕСКИЙ ДАШБОРД ЗАКАЗОВ
        public byte[] GenerateOrderDashboardPdf(OrderDashboardReportDto data, string title = "Управленческий дашборд заказов", DateTime? startDate = null, DateTime? endDate = null)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, title, startDate, endDate));

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Item().Element(c => ComposeOrderRecordsCards(c, data));
                        column.Item().PaddingVertical(15).Element(c => ComposeTopItemsSection(c, data.TopItems));
                        column.Item().PaddingBottom(5).Text("Реестр заказов").FontSize(14).SemiBold();
                        column.Item().Element(c => ComposeOrderRegistryTable(c, data.Orders));
                    });

                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        private void ComposeOrderRecordsCards(IContainer container, OrderDashboardReportDto data)
        {
            container.Row(row =>
            {
                // Финансовая сводка
                row.RelativeItem().PaddingRight(5).Background(Colors.Green.Lighten4).Border(1).BorderColor(Colors.Green.Lighten2).Padding(10).Column(col =>
                {
                    col.Item().Text("Выручка").FontSize(10).FontColor(Colors.Green.Darken3);
                    col.Item().Text($"{data.TotalRevenue:N2} ₽").FontSize(16).SemiBold();
                    col.Item().Text($"Заказов: {data.TotalOrders} | Средний чек: {data.AverageCheck:N2} ₽").FontSize(8);
                });

                // Самый тяжелый заказ
                row.RelativeItem().PaddingRight(5).Background(Colors.Blue.Lighten4).Border(1).BorderColor(Colors.Blue.Lighten2).Padding(10).Column(col =>
                {
                    col.Item().Text("Максимальный вес").FontSize(10).FontColor(Colors.Blue.Darken3);
                    if (data.HeaviestOrder != null)
                    {
                        col.Item().Text($"{data.HeaviestOrder.TotalWeightKg} кг").FontSize(16).SemiBold();
                        col.Item().Text($"Заказ #{data.HeaviestOrder.OrderId} ({data.HeaviestOrder.CustomerName})").FontSize(8);
                    }
                    else { col.Item().Text("Нет данных").Italic(); }
                });

                // Самый быстрый заказ
                row.RelativeItem().Background(Colors.Purple.Lighten4).Border(1).BorderColor(Colors.Purple.Lighten2).Padding(10).Column(col =>
                {
                    col.Item().Text("Рекорд скорости (Lead Time)").FontSize(10).FontColor(Colors.Purple.Darken3);
                    if (data.FastestOrder != null)
                    {
                        col.Item().Text($"{data.FastestOrder.LeadTimeFormatted}").FontSize(16).SemiBold();
                        col.Item().Text($"Заказ #{data.FastestOrder.OrderId}").FontSize(8);
                    }
                    else { col.Item().Text("Нет данных").Italic(); }
                });
            });
        }

        private void ComposeTopItemsSection(IContainer container, List<TopItemDto> topItems)
        {
            var top5 = topItems.Take(5).ToList();
            if (!top5.Any()) return;

            float maxQuantity = top5.Max(x => x.QuantitySold);

            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
            {
                col.Item().PaddingBottom(5).Text("ТОП-5 продаваемых товаров").FontSize(12).SemiBold();

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Название
                        columns.RelativeColumn(4); // График
                        columns.RelativeColumn(1); // Кол-во
                    });

                    foreach (var item in top5)
                    {
                        float fillWeight = maxQuantity > 0 ? ((float)item.QuantitySold / maxQuantity) : 0;
                        float emptyWeight = 1f - fillWeight;

                        table.Cell().PaddingVertical(2).Text(item.ItemName).FontSize(9);

                        table.Cell().PaddingVertical(2).AlignMiddle().Row(row =>
                        {
                            if (fillWeight > 0)
                                row.RelativeItem(fillWeight).Height(10).Background(Colors.Blue.Medium);
                            if (emptyWeight > 0)
                                row.RelativeItem(emptyWeight).Height(10).Background(Colors.Grey.Lighten4);
                        });

                        table.Cell().PaddingVertical(2).AlignRight().Text($"{item.QuantitySold} шт").FontSize(9).SemiBold();
                    }
                });
            });
        }

        private void ComposeOrderRegistryTable(IContainer container, List<OrderRegistryFlatDto> orders)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);   // ID
                    columns.RelativeColumn(1.5f); // Клиент
                    columns.RelativeColumn(3);    // Состав
                    columns.RelativeColumn(1.5f); // Тип и Оплата
                    columns.RelativeColumn(1);    // Сумма
                    columns.RelativeColumn(1);    // Вес
                    columns.RelativeColumn(2);    // Таймлайн (Создан / Ожидался / Выдан)
                    columns.RelativeColumn(1);    // Статус
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("ID");
                    header.Cell().Element(CellStyle).Text("Клиент");
                    header.Cell().Element(CellStyle).Text("Состав заказа");
                    header.Cell().Element(CellStyle).Text("Доставка/Оплата");
                    header.Cell().Element(CellStyle).AlignRight().Text("Сумма");
                    header.Cell().Element(CellStyle).AlignRight().Text("Вес");
                    header.Cell().Element(CellStyle).Text("Таймлайн (Созд/Ожид/Выдан)");
                    header.Cell().Element(CellStyle).Text("Статус");
                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                });

                int index = 0;
                foreach (var order in orders)
                {
                    var bgColor = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text($"#{order.OrderId}");
                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text(order.CustomerName).FontSize(8);

                    var itemsText = string.IsNullOrEmpty(order.ItemsList) ? "Нет позиций" : order.ItemsList;
                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text(itemsText).FontSize(8).Italic().FontColor(Colors.Grey.Darken3);

                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text($"{order.DeliveryType}\n{order.PaymentType}").FontSize(8);
                    table.Cell().Element(c => ItemStyle(c, bgColor)).AlignRight().Text($"{order.TotalPrice:N0} ₽").SemiBold().FontSize(8);
                    table.Cell().Element(c => ItemStyle(c, bgColor)).AlignRight().Text($"{order.TotalWeightKg} кг").FontSize(8);

                    // Блок детализации дат
                    var createdLocal = order.CreatedAt.ToLocalTime().ToString("dd.MM HH:mm");
                    var expectedLocal = order.ExpectedDeliveryDate?.ToLocalTime().ToString("dd.MM HH:mm") ?? "---";
                    var actualLocal = order.ActualHandoverAt?.ToLocalTime().ToString("dd.MM HH:mm") ?? "---";

                    table.Cell().Element(c => ItemStyle(c, bgColor)).Column(col =>
                    {
                        col.Item().Text($"С: {createdLocal}").FontSize(8);
                        col.Item().Text($"О: {expectedLocal}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text($"В: {actualLocal}").FontSize(8).SemiBold();
                        col.Item().Text($"Lead: {order.LeadTimeFormatted}").FontSize(8).FontColor(Colors.Blue.Medium);
                    });

                    var statusColor = order.StatusRaw == "Completed" ? Colors.Green.Darken1 : (order.StatusRaw == "Cancelled" ? Colors.Red.Medium : Colors.Black);
                    table.Cell().Element(c => ItemStyle(c, bgColor)).Text(order.Status).FontColor(statusColor).SemiBold().FontSize(8);

                    index++;
                }

                static IContainer ItemStyle(IContainer container, string bgColor) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Background(bgColor).PaddingVertical(6).PaddingHorizontal(2);
            });
        }
    }
}