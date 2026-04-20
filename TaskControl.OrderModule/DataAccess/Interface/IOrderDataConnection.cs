using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInfrastructure;
using TaskControl.InventoryModule.DataAccess.Model;
using TaskControl.OrderModule.DataAccess.Model;

namespace TaskControl.OrderModule.DataAccess.Interface
{
    public interface IOrderDataConnection : IDataConnection
    {
        ITable<OrderModel> Orders { get; }
        ITable<OrderPositionModel> OrderPositions { get; }

    }
}
