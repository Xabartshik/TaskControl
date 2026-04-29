using Microsoft.AspNetCore.SignalR;
using TaskControl.InformationModule.Application.Services;

namespace TaskControl.InformationModule.Services.Hubs
{
    public class QRHub : Hub
    {
        private readonly IQRTokenService _qrTokenService;

        // Внедряем сервис генерации токенов
        public QRHub(IQRTokenService qrTokenService)
        {
            _qrTokenService = qrTokenService;
        }

        // Этот метод клиент (браузер) будет вызывать сразу при старте страницы
        public async Task RequestCurrentQR()
        {
            // Генерируем свежий токен прямо сейчас
            string payload = _qrTokenService.GenerateTokenPayload();
            string expiresAtISO = DateTime.UtcNow.AddMinutes(1).ToString("O");

            // Отправляем данные ТОЛЬКО тому клиенту, который только что запросил (Caller)
            await Clients.Caller.SendAsync("ReceiveNewQR", payload, expiresAtISO);
        }
    }
}