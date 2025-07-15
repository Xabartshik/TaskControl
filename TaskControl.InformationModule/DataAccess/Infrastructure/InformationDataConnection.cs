using LinqToDB;
using Microsoft.Extensions.Configuration;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Model;
using TaskControl.InventoryModule.DataAccess.Infrastructure;

namespace TaskControl.InformationModule.DataAccess.Infrastructure
{
    public class InformationDataConnection : BaseDataConnection<InformationDataConnection>, IInformationDataConnection
    {
        public InformationDataConnection(IConfiguration configuration)
            : base(configuration, "InformationConnection")
        {
        }

        public ITable<BranchModel> Branches => this.GetTable<BranchModel>();
        public ITable<CheckIOEmployeeModel> CheckIOEmployees => this.GetTable<CheckIOEmployeeModel>();
        public ITable<EmployeeModel> Employees => this.GetTable<EmployeeModel>();
        public ITable<ItemModel> Items => this.GetTable<ItemModel>();
    }
}
