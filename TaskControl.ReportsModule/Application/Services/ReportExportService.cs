using CsvHelper;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TaskControl.ReportsModule.Application.DTOs;

namespace TaskControl.ReportsModule.Application.Services
{
    public class ReportExportService
    {
        public byte[] ExportToCsv<T>(IEnumerable<T> data)
        {
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms, new System.Text.UTF8Encoding(true));
            using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);
            csv.WriteRecords(data);
            sw.Flush();
            return ms.ToArray();
        }

        public byte[] ExportGroupedReportToPdf(IEnumerable<TaskGroupReportDto> groups, string title)
        {

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); // Альбомная ориентация
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    // Заголовок документа
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(title).FontSize(22).SemiBold().FontColor(Colors.Indigo.Darken2);
                            col.Item().Text($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                    // Таблица
                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);   // Задача / Имя сотрудника
                            columns.ConstantColumn(100); // Завершено (Время)
                            columns.RelativeColumn(1);   // Товаров
                            columns.RelativeColumn(1);   // Ожидание
                            columns.RelativeColumn(1);   // Выполнение
                            columns.RelativeColumn(1);   // Ошибки
                        });

                        // Шапка таблицы
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Задача / Назначенный сотрудник");
                            header.Cell().Element(HeaderStyle).Text("Время сдачи");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Позиций");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Ожидание");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Выполнение");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Ошибки");

                            static IContainer HeaderStyle(IContainer c) => c.DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                                .Background(Colors.Indigo.Darken2)        
                                .PaddingVertical(6).PaddingHorizontal(5);
                        });

                        foreach (var group in groups)
                        {
                            // СТРОКА ЗАДАЧИ
                            table.Cell().Element(GroupStyle).Text(group.GroupName).SemiBold().FontColor(Colors.Indigo.Darken3);
                            table.Cell().Element(GroupStyle).Text($"Сотрудников: {group.TotalWorkers}").FontColor(Colors.Indigo.Medium);
                            table.Cell().Element(GroupStyle).AlignRight().Text(group.TotalItems.ToString()).SemiBold();
                            table.Cell().Element(GroupStyle).AlignRight().Text("-");
                            table.Cell().Element(GroupStyle).AlignRight().Text(FormatDuration(group.TotalDurationSeconds)).SemiBold();

                            var discCell = table.Cell().Element(GroupStyle).AlignRight();
                            if (group.TotalDiscrepancies > 0) discCell.Text(group.TotalDiscrepancies.ToString()).FontColor(Colors.Red.Medium).SemiBold();
                            else discCell.Text("-").FontColor(Colors.Grey.Medium);

                            // СТРОКИ СОТРУДНИКОВ
                            bool isEven = false;
                            foreach (var worker in group.Workers)
                            {
                                var bgColor = isEven ? Colors.White : Colors.Grey.Lighten4;

                                // СМЕЩЕНИЕ ВПРАВО 
                                table.Cell().Element(c => WorkerStyle(c, bgColor)).PaddingLeft(20).Text("↳ " + worker.WorkerFullName);

                                table.Cell().Element(c => WorkerStyle(c, bgColor)).Text(worker.CompletedAt.ToString("HH:mm:ss")).FontColor(Colors.Grey.Darken1);
                                table.Cell().Element(c => WorkerStyle(c, bgColor)).AlignRight().Text(worker.ItemsProcessed.ToString());
                                table.Cell().Element(c => WorkerStyle(c, bgColor)).AlignRight().Text(FormatDuration(worker.WaitTimeSeconds));
                                table.Cell().Element(c => WorkerStyle(c, bgColor)).AlignRight().Text(FormatDuration(worker.DurationSeconds));

                                var wDiscCell = table.Cell().Element(c => WorkerStyle(c, bgColor)).AlignRight();
                                if (worker.Discrepancies > 0) wDiscCell.Text(worker.Discrepancies.ToString()).FontColor(Colors.Red.Medium);
                                else wDiscCell.Text("-").FontColor(Colors.Grey.Lighten1);

                                isEven = !isEven;
                            }
                        }

                        // Стили ячеек
                        static IContainer GroupStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Indigo.Medium)
                            .Background(Colors.Indigo.Lighten4).PaddingVertical(6).PaddingHorizontal(5); // Светло-синий фон группы

                        static IContainer WorkerStyle(IContainer c, string bg) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                            .Background(bg).PaddingVertical(4).PaddingHorizontal(5); // Чередование цветов сотрудников
                    });

                    // Футер
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Страница ");
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }


        public byte[] ExportDetailedReportToPdf(IEnumerable<DetailedTaskReportDto> data, string title)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Альбомная ориентация для вместимости всех колонок
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(header => ComposeHeader(header, title));
                    page.Content().Element(content => ComposeContent(content, data));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        private void ComposeHeader(IContainer container, string title)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(title)
                        .FontSize(20)
                        .SemiBold()
                        .FontColor(Colors.Indigo.Darken2);

                    column.Item().Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void ComposeContent(IContainer container, IEnumerable<DetailedTaskReportDto> data)
        {
            container.PaddingVertical(1, Unit.Centimetre).Table(table =>
            {
                // Настройка ширины колонок
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100); // Дата
                    columns.RelativeColumn(2);   // ФИО
                    columns.RelativeColumn(1.5f);  // Тип
                    columns.RelativeColumn(1);   // Товаров
                    columns.RelativeColumn(1);   // Очередь
                    columns.RelativeColumn(1);   // Ожидание
                    columns.RelativeColumn(1);   // Выполнение
                    columns.RelativeColumn(1);   // Расхождения
                });

                // Шапка таблицы
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Завершено").SemiBold().FontColor(Colors.White);
                    header.Cell().Element(CellStyle).Text("Исполнитель").SemiBold().FontColor(Colors.White);
                    header.Cell().Element(CellStyle).Text("Тип задачи").SemiBold().FontColor(Colors.White);
                    header.Cell().Element(CellStyle).AlignRight().Text("Позиций").SemiBold().FontColor(Colors.White);
                    header.Cell().Element(CellStyle).AlignRight().Text("Очередь").SemiBold().FontColor(Colors.White);
                    header.Cell().Element(CellStyle).AlignRight().Text("Ожидание").SemiBold().FontColor(Colors.White);
                    header.Cell().Element(CellStyle).AlignRight().Text("Выполнение").SemiBold().FontColor(Colors.White);
                    header.Cell().Element(CellStyle).AlignRight().Text("Ошибки").SemiBold().FontColor(Colors.White);

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold())
                            .PaddingVertical(5)
                            .BorderBottom(1).BorderColor(Colors.Black)
                            .Background(Colors.Indigo.Darken2)
                            .PaddingHorizontal(5);
                    }
                });

                // Заполнение данными с чередованием цветов строк
                var rowIndex = 0;
                foreach (var item in data)
                {
                    var isEven = rowIndex % 2 == 0;
                    var backgroundColor = isEven ? Colors.White : Colors.Grey.Lighten4;

                    table.Cell().Element(c => CellStyle(c, backgroundColor)).Text(item.CompletedAt.ToString("dd.MM.yyyy HH:mm"));
                    table.Cell().Element(c => CellStyle(c, backgroundColor)).Text(item.WorkerFullName);
                    table.Cell().Element(c => CellStyle(c, backgroundColor)).Text(item.TaskCategoryDisplayName);

                    table.Cell().Element(c => CellStyle(c, backgroundColor)).AlignRight().Text(item.ItemsProcessed.ToString());
                    table.Cell().Element(c => CellStyle(c, backgroundColor)).AlignRight().Text(item.QueueSize.ToString());

                    // Форматируем время
                    table.Cell().Element(c => CellStyle(c, backgroundColor)).AlignRight().Text(FormatDuration(item.WaitTimeSeconds));
                    table.Cell().Element(c => CellStyle(c, backgroundColor)).AlignRight().Text(FormatDuration(item.DurationSeconds));

                    // Расхождения (ошибки) подсвечиваем красным
                    var discCell = table.Cell().Element(c => CellStyle(c, backgroundColor)).AlignRight();
                    if (item.Discrepancies > 0)
                        discCell.Text(item.Discrepancies.ToString()).FontColor(Colors.Red.Medium).SemiBold();
                    else
                        discCell.Text("-").FontColor(Colors.Grey.Medium);

                    rowIndex++;
                }

                static IContainer CellStyle(IContainer container, string bgColor)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Background(bgColor)
                        .PaddingVertical(5).PaddingHorizontal(5);
                }
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

        // Вспомогательный метод для перевода секунд в формат ЧЧ:ММ:СС
        private string FormatDuration(int seconds)
        {
            if (seconds == 0) return "00:00:00";
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}