using System;
using System.Collections.Generic;

namespace TaskControl.OrderModule.Application.Exceptions
{
    // Исключение, выбрасываемое при нехватке товаров на складе в процессе оформления заказа
    public class OutOfStockException : Exception
    {
        public List<int> OutOfStockItemIds { get; }

        public OutOfStockException(string message, List<int> outOfStockItemIds) : base(message)
        {
            OutOfStockItemIds = outOfStockItemIds;
        }
    }
}
