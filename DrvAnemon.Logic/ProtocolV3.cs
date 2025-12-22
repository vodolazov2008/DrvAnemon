using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scada.Comm.Drivers.DrvAnemon.Logic;

namespace Scada.Comm.Drivers.DrvAnemon
{
    /// <summary>
    /// Протокол Anemon версии 3 - HTTP/JSON
    /// </summary>
    internal static class ProtocolV3
    {
        /// <summary>
        /// Обработка команды и формирование ответа
        /// </summary>
        public static string ProcessCommand(string requestBody, string token, DevAnemonLogic deviceLogic)
        {
            try
            {
                // Парсинг JSON команды
                var command = JsonSerializer.Deserialize<AnemonCommand>(requestBody);
                
                if (command == null || string.IsNullOrEmpty(command.Cmd))
                {
                    return CreateErrorResponse("Invalid command format");
                }

                // Проверка токена (базовая реализация)
                if (!ValidateToken(token, deviceLogic))
                {
                    return CreateErrorResponse("Invalid token");
                }

                // Обработка команды
                switch (command.Cmd.ToLower())
                {
                    case "get_last_ts":
                        return ProcessGetLastTs(command, deviceLogic);
                    
                    default:
                        return CreateErrorResponse($"Unknown command: {command.Cmd}");
                }
            }
            catch (Exception ex)
            {
                deviceLogic?.Log?.WriteLine($"Ошибка обработки команды: {ex.Message}");
                return CreateErrorResponse("Internal server error");
            }
        }

        /// <summary>
        /// Обработка команды "get_last_ts"
        /// </summary>
        private static string ProcessGetLastTs(AnemonCommand command, DevAnemonLogic deviceLogic)
        {
            try
            {
                if (command.Data == null || command.Data.Count == 0)
                {
                    return CreateErrorResponse("No device data provided");
                }

                var response = new AnemonResponse
                {
                    Cmd = "get_last_ts_resp",
                    Ts = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds,
                    Data = new List<DeviceDataResponse>()
                };

                foreach (var deviceInfo in command.Data)
                {
                    // Поиск устройства по серийному номеру
                    var deviceData = GetDeviceDataBySerial(deviceInfo.Serial, deviceLogic);
                    
                    response.Data.Add(new DeviceDataResponse
                    {
                        Serial = deviceInfo.Serial,
                        LastTs = deviceData?.LastTimestamp ?? 0
                    });
                }

                return JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
            }
            catch (Exception ex)
            {
                deviceLogic?.Log?.WriteLine($"Ошибка обработки команды get_last_ts: {ex.Message}");
                return CreateErrorResponse("Failed to process get_last_ts");
            }
        }

        /// <summary>
        /// Валидация токена
        /// </summary>
        private static bool ValidateToken(string token, DevAnemonLogic deviceLogic)
        {
            // Базовая валидация токена
            // TODO: Реализовать полную логику валидации токенов
            return !string.IsNullOrEmpty(token) && token.Length >= 10;
        }

        /// <summary>
        /// Получение данных устройства по серийному номеру
        /// </summary>
        private static DeviceTimestampData GetDeviceDataBySerial(string serial, DevAnemonLogic deviceLogic)
        {
            if (string.IsNullOrEmpty(serial) || deviceLogic == null)
                return null;

            try
            {
                // Получение последней временной метки из логики устройства
                long? lastTimestamp = deviceLogic.GetLastTimestamp(serial);
                
                return new DeviceTimestampData
                {
                    Serial = serial,
                    LastTimestamp = lastTimestamp ?? 0
                };
            }
            catch (Exception ex)
            {
                deviceLogic?.Log?.WriteLine($"Ошибка получения данных устройства {serial}: {ex.Message}");
                return new DeviceTimestampData
                {
                    Serial = serial,
                    LastTimestamp = 0
                };
            }
        }

        /// <summary>
        /// Создание ответа с ошибкой
        /// </summary>
        private static string CreateErrorResponse(string errorMessage)
        {
            var errorResponse = new
            {
                cmd = "error",
                error = errorMessage,
                ts = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds
            };

            return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }
    }

    /// <summary>
    /// Команда от устройства
    /// </summary>
    public class AnemonCommand
    {
        [JsonPropertyName("cmd")]
        public string Cmd { get; set; }

        [JsonPropertyName("data")]
        public List<DeviceInfo> Data { get; set; }
    }

    /// <summary>
    /// Информация об устройстве
    /// </summary>
    public class DeviceInfo
    {
        [JsonPropertyName("serial")]
        public string Serial { get; set; }

        [JsonPropertyName("save_interval")]
        public int SaveInterval { get; set; }
    }

    /// <summary>
    /// Ответ устройству
    /// </summary>
    public class AnemonResponse
    {
        [JsonPropertyName("cmd")]
        public string Cmd { get; set; }

        [JsonPropertyName("ts")]
        public long Ts { get; set; }

        [JsonPropertyName("data")]
        public List<DeviceDataResponse> Data { get; set; }
    }

    /// <summary>
    /// Данные устройства в ответе
    /// </summary>
    public class DeviceDataResponse
    {
        [JsonPropertyName("serial")]
        public string Serial { get; set; }

        [JsonPropertyName("last_ts")]
        public long LastTs { get; set; }
    }

    /// <summary>
    /// Данные о временных метках устройства
    /// </summary>
    public class DeviceTimestampData
    {
        public string Serial { get; set; }
        public long LastTimestamp { get; set; }
    }
}
