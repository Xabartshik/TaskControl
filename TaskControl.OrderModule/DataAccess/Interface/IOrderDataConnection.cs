using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.OrderModule.DataAccess.Model;

namespace TaskControl.OrderModule.DataAccess.Interface
{
    public interface IOrderDataConnection
    {
        ITable<OrderModel> Orders { get; }

        Task<int> InsertAsync<T>(T entity) where T : class;
        Task<int> UpdateAsync<T>(T entity) where T : class;
        Task<int> DeleteAsync<T>(T entity) where T : class;
    }
}
