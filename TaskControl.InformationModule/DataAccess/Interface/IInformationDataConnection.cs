using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InformationModule.DataAccess.Model;

namespace TaskControl.InformationModule.DataAccess.Interface
{
    public interface IInformationDataConnection
    {
        ITable<BranchModel> Branches { get; }
        ITable<CheckIOEmployeeModel> CheckIOEmployees { get; }
        ITable<EmployeeModel> Employees { get; }
        ITable<ItemModel> Items { get; }

        Task<int> InsertAsync<T>(T entity) where T : class;
        Task<int> UpdateAsync<T>(T entity) where T : class;
        Task<int> DeleteAsync<T>(T entity) where T : class;
    }
}
