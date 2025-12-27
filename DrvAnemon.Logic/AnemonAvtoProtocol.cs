using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scada.Comm.Drivers.DrvAnemon.Logic;

namespace Scada.Comm.Drivers.DrvAnemon
{
    /// <summary>
    /// Протокол ANEMON-AVTO - TCP/JSON протокол для обмена данными с устройствами
    /// </summary>
    internal static class AnemonAvtoProtocol
    {
        /// <summary>
        /// Обработка входящих данных от TCP соединения
        /// </summary>
        /// <param name="rawData">Необработанные данные от TCP соединения</param>
        /// <param name="deviceLogic">Логика устройства для обработки данных</param>
        /// <param name="logWriter">Логгер для записи сообщений</param>
        /// <returns>Ответные данные для отправки клиенту (null если ответа нет)</returns>
        public static string ProcessTcpData(byte[] rawData, DevAnemonLogic deviceLogic, Action<string> logWriter = null)
        {
            try
            {
                // Преобразование байтов в строку и удаление завершающего символа '~'
                string message = Encoding.UTF8.GetString(rawData);
                if (message.EndsWith("~"))
                {
                    message = message.TrimEnd('~');
                }

                // Парсинг JSON сообщения
                using var document = JsonDocument.Parse(message);
                var root = document.RootElement;

                // Проверка наличия поля request для определения типа сообщения
                if (root.TryGetProperty("request", out JsonElement requestElement))
                {
                    int requestType = requestElement.GetInt32();
                    return ProcessRequestMessage(requestType, root, deviceLogic, logWriter);
                }

                // Если это ответное сообщение от сервера
                if (root.TryGetProperty("response", out JsonElement responseElement))
                {
                    ProcessResponseMessage(responseElement.GetInt32(), root, deviceLogic, logWriter);
                    return null; // Ответные сообщения обычно не требуют ответа
                }

                logWriter?.Invoke($"Неизвестный формат сообщения: {message}");
                return null;
            }
            catch (JsonException ex)
            {
                logWriter?.Invoke($"Ошибка парсинга JSON: {ex.Message}");
                return CreateErrorResponse("Invalid JSON format");
            }
            catch (Exception ex)
            {
                logWriter?.Invoke($"Ошибка обработки TCP данных: {ex.Message}");
                return CreateErrorResponse("Processing error");
            }
        }

        /// <summary>
        /// Обработка запросных сообщений от устройств
        /// </summary>
        private static string ProcessRequestMessage(int requestType, JsonElement data, DevAnemonLogic? deviceLogic, Action<string>? logWriter = null)
        {
            switch (requestType)
            {
                case 1:
                    return ProcessRequestType1(data, deviceLogic, logWriter);
                case 2:
                    return ProcessRequestType2(data, deviceLogic, logWriter);
                default:
                    logWriter?.Invoke($"Неизвестный тип запроса: {requestType}");
                    return CreateErrorResponse($"Unknown request type: {requestType}");
            }
        }

        /// <summary>
        /// Обработка Request Type 1 - Идентификация устройства
        /// </summary>
        private static string ProcessRequestType1(JsonElement data, DevAnemonLogic? deviceLogic, Action<string>? logWriter = null)
        {
            try
            {
                if (!data.TryGetProperty("serial", out JsonElement serialElement))
                {
                    logWriter?.Invoke("Отсутствует серийный номер в Request Type 1");
                    return CreateErrorResponse("Missing serial number");
                }

                string serial = serialElement.GetString() ?? "unknown";
                string ip = data.TryGetProperty("ip", out JsonElement ipElement) ? 
                    ipElement.GetString() ?? "unknown" : "unknown";

                logWriter?.Invoke($"Идентификация устройства: {serial} с IP {ip}");

                // Отправляем Time Sync в ответ на идентификацию
                return CreateTimeSyncResponse();
            }
            catch (Exception ex)
            {
                logWriter?.Invoke($"Ошибка обработки Request Type 1: {ex.Message}");
                return CreateErrorResponse("Request Type 1 processing error");
            }
        }

        /// <summary>
        /// Обработка Request Type 2 - Данные устройства
        /// </summary>
        private static string ProcessRequestType2(JsonElement data, DevAnemonLogic? deviceLogic, Action<string>? logWriter = null)
        {
            try
            {
                // Извлечение основной информации об устройстве
                string serial = data.TryGetProperty("serial", out JsonElement serialElement) ? 
                    serialElement.GetString() ?? "unknown" : "unknown";
                string ip = data.TryGetProperty("ip", out JsonElement ipElement) ? 
                    ipElement.GetString() ?? "unknown" : "unknown";

                // Извлечение данных датчиков
                var sensorData = ExtractSensorData(data, deviceLogic, logWriter);

                // Регистрация данных в логике устройства
                long timestamp = data.TryGetProperty("time_unix", out JsonElement timeElement) ? 
                    timeElement.GetInt64() : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                deviceLogic?.RegisterDeviceData(serial, timestamp, sensorData);

                logWriter?.Invoke($"Получены данные от устройства {serial}: {sensorData.Count} датчиков");

                // Отправляем подтверждение получения данных
                return CreateResponseType2();
            }
            catch (Exception ex)
            {
                logWriter?.Invoke($"Ошибка обработки Request Type 2: {ex.Message}");
                return CreateErrorResponse("Request Type 2 processing error");
            }
        }

        /// <summary>
        /// Обработка ответных сообщений
        /// </summary>
        private static void ProcessResponseMessage(int responseType, JsonElement data, DevAnemonLogic? deviceLogic, Action<string>? logWriter = null)
        {
            switch (responseType)
            {
                case 2:
                    // Response Type 2 - подтверждение получения данных
                    logWriter?.Invoke("Получено подтверждение от устройства");
                    break;
                default:
                    logWriter?.Invoke($"Неизвестный тип ответа: {responseType}");
                    break;
            }
        }

        /// <summary>
        /// Извлечение данных датчиков из JSON
        /// </summary>
        private static Dictionary<int, double> ExtractSensorData(JsonElement data, DevAnemonLogic? deviceLogic, Action<string>? logWriter = null)
        {
            var sensorData = new Dictionary<int, double>();

            try
            {
                // Извлечение текущих значений температуры и влажности
                if (data.TryGetProperty("temp_now", out JsonElement tempElement))
                {
                    sensorData[100] = tempElement.GetDouble(); // Датчик температуры (индекс 100)
                }

                if (data.TryGetProperty("hum_now", out JsonElement humElement))
                {
                    sensorData[101] = humElement.GetDouble(); // Датчик влажности (индекс 101)
                }

                // Извлечение исторических данных из массива data
                if (data.TryGetProperty("data", out JsonElement dataArray) && 
                    dataArray.ValueKind == JsonValueKind.Array)
                {
                    int sensorIndex = 0;
                    foreach (JsonElement dataPoint in dataArray.EnumerateArray())
                    {
                        if (dataPoint.TryGetProperty("t", out JsonElement tempPoint))
                        {
                            sensorData[200 + sensorIndex] = tempPoint.GetDouble(); // Исторические данные температуры
                        }

                        if (dataPoint.TryGetProperty("h", out JsonElement humPoint))
                        {
                            sensorData[300 + sensorIndex] = humPoint.GetDouble(); // Исторические данные влажности
                        }

                        sensorIndex++;
                        if (sensorIndex >= 256) // Ограничение на количество датчиков
                            break;
                    }
                }

                // Извлечение дополнительных параметров
                if (data.TryGetProperty("rssi", out JsonElement rssiElement))
                {
                    sensorData[400] = rssiElement.GetInt32(); // RSSI Wi-Fi
                }

                if (data.TryGetProperty("voltage", out JsonElement voltageElement))
                {
                    sensorData[401] = voltageElement.GetInt32(); // Напряжение питания (мВ)
                }

                if (data.TryGetProperty("fw", out JsonElement fwElement))
                {
                    // Версия прошивки сохраняется как строковый тег (индекс 500)
                    deviceLogic?.SetStringTag(500, fwElement.GetString() ?? "unknown");
                }
            }
            catch (Exception ex)
            {
                logWriter?.Invoke($"Ошибка извлечения данных датчиков: {ex.Message}");
            }

            return sensorData;
        }

        /// <summary>
        /// Создание ответа Time Sync
        /// </summary>
        private static string CreateTimeSyncResponse()
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            var timeSync = new
            {
                time = currentTime,
                last = currentTime - 60 // Последние данные были минуту назад
            };

            return JsonSerializer.Serialize(timeSync, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }

        /// <summary>
        /// Создание ответа Response Type 2
        /// </summary>
        private static string CreateResponseType2()
        {
            var response = new
            {
                response = 2,
                status = "ok"
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }

        /// <summary>
        /// Создание ответа с ошибкой
        /// </summary>
        private static string CreateErrorResponse(string errorMessage)
        {
            var errorResponse = new
            {
                error = errorMessage,
                ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }

        /// <summary>
        /// Формирование данных для отправки с завершающим символом
        /// </summary>
        public static byte[] PrepareDataForSending(string jsonData)
        {
            // Добавляем завершающий символ '~' как требует протокол
            string dataWithTerminator = jsonData + "~";
            return Encoding.UTF8.GetBytes(dataWithTerminator);
        }
    }

    /// <summary>
    /// Класс для хранения информации об устройстве ANEMON-AVTO
    /// </summary>
    public class AnemonDeviceInfo
    {
        [JsonPropertyName("serial")]
        public string Serial { get; set; } = string.Empty;

        [JsonPropertyName("ip")]
        public string Ip { get; set; } = string.Empty;

        [JsonPropertyName("device_name")]
        public string DeviceName { get; set; } = string.Empty;

        [JsonPropertyName("fw")]
        public string FirmwareVersion { get; set; } = string.Empty;

        [JsonPropertyName("temp_now")]
        public double Temperature { get; set; }

        [JsonPropertyName("hum_now")]
        public double Humidity { get; set; }

        [JsonPropertyName("time_unix")]
        public long Timestamp { get; set; }

        [JsonPropertyName("data")]
        public List<AnemonDataPoint>? Data { get; set; } = new List<AnemonDataPoint>();
    }

    /// <summary>
    /// Точка данных измерений
    /// </summary>
    public class AnemonDataPoint
    {
        [JsonPropertyName("ts")]
        public long Timestamp { get; set; }

        [JsonPropertyName("t")]
        public double Temperature { get; set; }

        [JsonPropertyName("h")]
        public double Humidity { get; set; }
    }
}
