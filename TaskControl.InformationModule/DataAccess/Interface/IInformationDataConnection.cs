using LinqToDB;
using TaskControl.Core.Shared.SharedInfrastructure;
using TaskControl.InformationModule.DataAccess.Model;

namespace TaskControl.InformationModule.DataAccess.Interface
{
    public interface IInformationDataConnection : IDataConnection
    {
        ITable<BranchModel> Branches { get; }
        ITable<CheckIOEmployeeModel> CheckIOEmployees { get; }
        ITable<EmployeeModel> Employees { get; }
        ITable<ItemModel> Items { get; }
        ITable<CourierCapabilityModel> CourierCapabilities { get; }

        Task<int> InsertAsync<T>(T entity) where T : class;
        Task<int> UpdateAsync<T>(T entity) where T : class;
        Task<int> DeleteAsync<T>(T entity) where T : class;
    }
}
