using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.Core.SharedInfrastructure;
using TaskControl.ReportsModule.DataAccess.Model;

namespace TaskControl.ReportsModule.DataAccess.Interface
{
    public interface IReportDataConnection : IDataConnection
    {
        ITable<RawEventModel> RawEvents { get; }
    }
}
