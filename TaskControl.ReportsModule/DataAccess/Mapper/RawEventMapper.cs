using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskControl.ReportsModule.DataAccess.Model;
using TaskControl.ReportsModule.Domain;

namespace TaskControl.ReportsModule.DataAccess.Mapper
{
    public static class RawEventMapper
    {
        // Маппинг из бизнес-сущности (RawEvent) в модель (RawEventModel)
        public static RawEventModel ToModel(this RawEvent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new RawEventModel
            {
                ReportId = entity.ReportId,
                Type = entity.Type,
                // Сериализация JsonDocument в строку
                JsonParams = entity.JSONParams.RootElement.ToString(),
                EventTime = entity.EventTime,
                SourceService = entity.SourceService,
                // Установка текущего времени для CreatedAt
                CreatedAt = DateTime.UtcNow
            };
        }

        // Маппинг из модели (RawEventModel) в бизнес-сущность (RawEvent)
        public static RawEvent ToDomain(this RawEventModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return new RawEvent
            {
                ReportId = model.ReportId,
                Type = model.Type,
                // Парсинг строки в JsonDocument
                JSONParams = JsonDocument.Parse(model.JsonParams),
                EventTime = model.EventTime,
                SourceService = model.SourceService
            };
        }
    }
}