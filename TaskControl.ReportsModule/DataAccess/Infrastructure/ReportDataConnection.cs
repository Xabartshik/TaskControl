﻿using LinqToDB;
using Microsoft.Extensions.Configuration;
using TaskControl.Core.Shared.Shared.SharedInfrastructure;
using TaskControl.Core.Shared.SharedInfrastructure;
using TaskControl.ReportsModule.DataAccess.Interface;
using TaskControl.ReportsModule.DataAccess.Model;

namespace TaskControl.ReportsModule.DataAccess.Infrastructure
{
    public class ReportDataConnection : BaseDataConnection<ReportDataConnection>, IReportDataConnection
    {
        public ReportDataConnection(IConfiguration configuration)
            : base(configuration, "DefaultConnection")
        {
        }

        public ITable<RawEventModel> RawEvents => this.GetTable<RawEventModel>();
    }
}