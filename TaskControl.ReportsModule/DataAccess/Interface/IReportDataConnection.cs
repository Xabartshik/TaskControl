using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.ReportsModule.DataAccess.Model;

namespace TaskControl.ReportsModule.DataAccess.Interface
{
    public interface IReportDataConnection
    {
        ITable<RawEventModel> RawEvents { get; }

        Task<int> InsertAsync<T>(T entity) where T : class;
        Task<int> UpdateAsync<T>(T entity) where T : class;
        Task<int> DeleteAsync<T>(T entity) where T : class;
    }
}
