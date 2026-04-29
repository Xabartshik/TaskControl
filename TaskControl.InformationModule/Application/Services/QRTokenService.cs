using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;

namespace TaskControl.InformationModule.Application.Services
{
    /// <summary>
    /// Интерфейс для генерации и валидации временных токенов QR-кодов.
    /// </summary>
    public interface IQRTokenService
    {
        string GenerateTokenPayload();
        bool ValidateTokenPayload(string payload, out string errorMessage);
    }

    /// <summary>
    /// Реализация сервиса QR-токенов с использованием HMAC-подписи и проверкой времени жизни.
    /// </summary>
    public class QRTokenService : IQRTokenService
    {
        private readonly string _secretKey;
        private readonly int _validityWindowMinutes = 2; // Код живет 1 минуту + 1 минута на рассинхрон

        public QRTokenService(IOptions<AppSettings> options)
        {
            _secretKey = options.Value.QrHmacSecretKey;

            // Защита от запуска без конфигурации
            if (string.IsNullOrWhiteSpace(_secretKey))
            {
                throw new ArgumentNullException(
                    nameof(AppSettings.QrHmacSecretKey),
                    "Секретный ключ для QR не задан в appsettings.json!"
                );
            }
        }

        /// <summary>
        /// Генерирует строку для QR-кода в формате "timestamp|signature"
        /// </summary>
        public string GenerateTokenPayload()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signature = GenerateSignature(timestamp);
            return $"{timestamp}|{signature}";
        }

        /// <summary>
        /// Проверяет подпись и актуальность времени QR-кода.
        /// </summary>
        public bool ValidateTokenPayload(string payload, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(payload))
            {
                errorMessage = "Пустой QR-код.";
                return false;
            }

            var parts = payload.Split('|');
            if (parts.Length != 2)
            {
                errorMessage = "Неверный формат QR-кода.";
                return false;
            }

            var timestampStr = parts[0];
            var signature = parts[1];

            // 1. Проверяем подпись (защита от подделки)
            var expectedSignature = GenerateSignature(timestampStr);
            if (signature != expectedSignature)
            {
                errorMessage = "QR-код недействителен (неверная подпись).";
                return false;
            }

            // 2. Проверяем время (защита от использования старых фото/копий)
            if (long.TryParse(timestampStr, out long timestamp))
            {
                var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                var currentTime = DateTimeOffset.UtcNow;

                if (currentTime > tokenTime.AddMinutes(_validityWindowMinutes))
                {
                    errorMessage = "Срок действия QR-кода истек.";
                    return false;
                }

                // Опционально: проверка на "будущее" время (если часы на клиенте сильно спешат)
                if (tokenTime > currentTime.AddMinutes(1))
                {
                    errorMessage = "Ошибка времени (QR-код из будущего).";
                    return false;
                }

                return true;
            }

            errorMessage = "Ошибка чтения времени из QR-кода.";
            return false;
        }

        /// <summary>
        /// Создает HMACSHA256 подпись для данных.
        /// </summary>
        private string GenerateSignature(string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
            using var hmac = new HMACSHA256(keyBytes);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var hash = hmac.ComputeHash(dataBytes);
            return Convert.ToBase64String(hash);
        }
    }
}