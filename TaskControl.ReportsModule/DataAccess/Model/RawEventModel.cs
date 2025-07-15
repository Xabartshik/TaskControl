using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.ReportsModule.DataAccess.Model
{
    [Table("raw_events")]
    public class RawEventModel
    {
        [Column("report_id")][PrimaryKey] public int ReportId { get; set; }
        [Column("type")][NotNull] public string Type { get; set; }
        [Column("json_params")][NotNull] public string JsonParams { get; set; } // Используется string для JSONB
        [Column("event_time")][NotNull] public DateTime EventTime { get; set; }
        [Column("source_service")][NotNull] public string SourceService { get; set; }
        [Column("created_at")][NotNull] public DateTime CreatedAt { get; set; }
    }
}
