using LinqToDB;
using Microsoft.Extensions.Configuration;
using TaskControl.Core.Shared.Shared.SharedInfrastructure;
using TaskControl.Core.Shared.SharedInfrastructure;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Model;

namespace TaskControl.InformationModule.DataAccess.Infrastructure
{
    public class InformationDataConnection : BaseDataConnection<InformationDataConnection>, IInformationDataConnection
    {
        public InformationDataConnection(IConfiguration configuration)
            : base(configuration, "DefaultConnection")
        {
        }

        public ITable<BranchModel> Branches => this.GetTable<BranchModel>();
        public ITable<CheckIOEmployeeModel> CheckIOEmployees => this.GetTable<CheckIOEmployeeModel>();
        public ITable<EmployeeModel> Employees => this.GetTable<EmployeeModel>();
        public ITable<ItemModel> Items => this.GetTable<ItemModel>();
    }
}
