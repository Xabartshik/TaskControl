using System.Collections.Generic;

namespace TaskControl.OrderModule.Application.DTOs
{
    // Объект передачи данных для информирования клиента о причинах конфликта при оформлении заказа
    public class OrderCreationConflictDto
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<int> InvalidItemIds { get; set; }
    }
}
