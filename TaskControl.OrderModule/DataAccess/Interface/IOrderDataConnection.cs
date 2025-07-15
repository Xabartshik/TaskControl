using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.Core.SharedInfrastructure;
using TaskControl.OrderModule.DataAccess.Model;

namespace TaskControl.OrderModule.DataAccess.Interface
{
    public interface IOrderDataConnection : IDataConnection
    {
        ITable<OrderModel> Orders { get; }

    }
}
