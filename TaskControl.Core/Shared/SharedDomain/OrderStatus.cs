using System.ComponentModel;

public enum OrderStatus
{
    [Description("Ожидает оплаты")]
    AwaitingPayment,
    [Description("Создан")]
    Created,
    [Description("В процессе сборки")]
    Assembly,
    [Description("Собран и ожидает выдачи/доставки")]
    Ready,
    [Description("В пути — для курьера или постамата")]
    InTransit,
    [Description("Завершен")]
    Completed,
    [Description("Частично выдан")]
    PartiallyCompleted,
    [Description("Отказ при получении")]
    Rejected,
    [Description("Отменен")]
    Cancelled
}