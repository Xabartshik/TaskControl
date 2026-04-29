using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.Services.Hubs;

namespace TaskControl.InformationModule.Services.BackgroundServices
{
    public class QRGeneratorService : BackgroundService
    {
        private readonly IHubContext<QRHub> _hubContext;
        private readonly IQRTokenService _qrTokenService;
        private readonly ILogger<QRGeneratorService> _logger;

        public QRGeneratorService(
            IHubContext<QRHub> hubContext,
            IQRTokenService qrTokenService,
            ILogger<QRGeneratorService> logger)
        {
            _hubContext = hubContext;
            _qrTokenService = qrTokenService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Служба генерации QR-кодов запущена.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Генерируем криптографическую строку
                    string payload = _qrTokenService.GenerateTokenPayload();

                    // Вычисляем время "смерти" кода для таймера на экране
                    string expiresAtISO = DateTime.UtcNow.AddMinutes(1).ToString("O");

                    // Рассылаем данные всем подключенным экранам проходных
                    await _hubContext.Clients.All.SendAsync("ReceiveNewQR", payload, expiresAtISO, stoppingToken);
                    //_logger.LogInformation("Текущий payload: {payload}", payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при генерации/отправке QR-кода.");
                }

                // Ждем ровно 1 минуту до следующего обновления
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}