using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;

namespace TaskControl.InformationModule.Application.Services
{
    public interface IQRTokenService
    {
        // Старые методы для сотрудников
        string GenerateTokenPayload();
        bool ValidateTokenPayload(string payload, out string errorMessage);

        // НОВЫЕ методы для выдачи заказов
        string GenerateOrderPickupToken(int customerId, int orderId);
        bool ValidateOrderPickupToken(string payload, out int customerId, out int orderId, out string errorMessage);
    }

    public class QRTokenService : IQRTokenService
    {
        private readonly string _secretKey;
        private readonly int _validityWindowMinutes = 2; // Окно действия для токенов сотрудников

        public QRTokenService(IOptions<AppSettings> options)
        {
            _secretKey = options.Value.QrHmacSecretKey;

            if (string.IsNullOrWhiteSpace(_secretKey))
            {
                throw new ArgumentNullException(
                    nameof(AppSettings.QrHmacSecretKey),
                    "Секретный ключ для QR не задан в appsettings.json!"
                );
            }
        }

        #region Методы для пропусков сотрудников (Обратная совместимость)

        public string GenerateTokenPayload()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signature = GenerateSignature(timestamp);
            return $"{timestamp}|{signature}";
        }

        public bool ValidateTokenPayload(string payload, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(payload)) { errorMessage = "Пустой QR-код."; return false; }

            var parts = payload.Split('|');
            if (parts.Length != 2) { errorMessage = "Неверный формат QR-кода."; return false; }

            var timestampStr = parts[0];
            var signature = parts[1];
            var expectedSignature = GenerateSignature(timestampStr);

            if (signature != expectedSignature) { errorMessage = "QR-код недействителен (неверная подпись)."; return false; }

            if (long.TryParse(timestampStr, out long timestamp))
            {
                var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                var currentTime = DateTimeOffset.UtcNow;

                if (currentTime > tokenTime.AddMinutes(_validityWindowMinutes)) { errorMessage = "Срок действия QR-кода истек."; return false; }
                if (tokenTime > currentTime.AddMinutes(1)) { errorMessage = "Ошибка времени (QR-код из будущего)."; return false; }

                return true;
            }

            errorMessage = "Ошибка чтения времени из QR-кода.";
            return false;
        }

        #endregion

        #region Методы для выдачи заказов

        /// <summary>
        /// Генерирует QR-код клиента со сроком жизни до следующей полуночи по МСК
        /// Формат: customerId|orderId|expiresAt|signature
        /// </summary>
        public string GenerateOrderPickupToken(int customerId, int orderId)
        {
            var expiresAt = GetNextMidnightMskUnixTime();
            var dataToSign = $"{customerId}|{orderId}|{expiresAt}";
            var signature = GenerateSignature(dataToSign);

            return $"{dataToSign}|{signature}";
        }

        /// <summary>
        /// Проверяет подпись и срок жизни QR-кода клиента
        /// </summary>
        public bool ValidateOrderPickupToken(string payload, out int customerId, out int orderId, out string errorMessage)
        {
            customerId = 0;
            orderId = 0;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(payload))
            {
                errorMessage = "Пустой QR-код.";
                return false;
            }

            var parts = payload.Split('|');
            if (parts.Length != 4)
            {
                errorMessage = "Неверный формат QR-кода заказа.";
                return false;
            }

            var customerIdStr = parts[0];
            var orderIdStr = parts[1];
            var expiresAtStr = parts[2];
            var signature = parts[3];

            // 1. Проверяем подпись (защита от подмены данных)
            var expectedSignature = GenerateSignature($"{customerIdStr}|{orderIdStr}|{expiresAtStr}");
            if (signature != expectedSignature)
            {
                errorMessage = "QR-код недействителен (неверная подпись).";
                return false;
            }

            // 2. Проверяем срок действия
            if (!long.TryParse(expiresAtStr, out long expiresAtUnix))
            {
                errorMessage = "Ошибка чтения времени из QR-кода.";
                return false;
            }

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiresAtUnix)
            {
                errorMessage = "Срок действия QR-кода истек. Пожалуйста, обновите код в приложении.";
                return false;
            }

            // 3. Извлекаем данные
            if (!int.TryParse(customerIdStr, out customerId) || !int.TryParse(orderIdStr, out orderId))
            {
                errorMessage = "Ошибка обработки идентификаторов в QR-коде.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Вычисляет Unix Timestamp для 00:00 следующего дня по Московскому времени
        /// </summary>
        private long GetNextMidnightMskUnixTime()
        {
            TimeZoneInfo mskZone;
            try
            {
                // Для Linux/Docker сред
                mskZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
            }
            catch (TimeZoneNotFoundException)
            {
                // Для Windows сред
                mskZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            }

            var nowMsk = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, mskZone);
            var nextMidnightMsk = nowMsk.Date.AddDays(1); // 00:00 следующего дня

            var nextMidnightUtc = TimeZoneInfo.ConvertTimeToUtc(nextMidnightMsk, mskZone);
            return new DateTimeOffset(nextMidnightUtc).ToUnixTimeSeconds();
        }

        #endregion

        /// <summary>
        /// Создает HMACSHA256 подпись для строки данных
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