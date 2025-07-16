using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.InventoryModule.DataAccess.Infrastructure;
using TaskControl.OrderModule.DataAccess.Interface;
using TaskControl.OrderModule.DataAccess.Model;

namespace TaskControl.OrderModule.DataAccess.Infrastructure
{
    public class OrderDataConnection : BaseDataConnection<OrderDataConnection>, IOrderDataConnection
    {
        public OrderDataConnection(IConfiguration configuration)
            : base(configuration, "DefaultConnection")
        {
        }

        public ITable<OrderModel> Orders => this.GetTable<OrderModel>();

    }
}
