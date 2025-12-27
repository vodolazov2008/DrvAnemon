using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Scada.Comm.Drivers.DrvAnemon.Test
{
    /// <summary>
    /// Тестовый клиент для симуляции устройств ANEMON-AVTO
    /// </summary>
    public class AnemonAvtoTestClient
    {
        private const string DefaultServerHost = "localhost";
        private const int DefaultServerPort = 4004;
        
        private readonly string _host;
        private readonly int _port;
        private readonly string _serialNumber;
        private readonly string _deviceIp;

        public AnemonAvtoTestClient(string serialNumber, string deviceIp = "192.168.2.4", string host = DefaultServerHost, int port = DefaultServerPort)
        {
            _serialNumber = serialNumber;
            _deviceIp = deviceIp;
            _host = host;
            _port = port;
        }

        /// <summary>
        /// Запуск тестового клиента
        /// </summary>
        public async Task RunAsync()
        {
            Console.WriteLine($"Запуск тестового клиента ANEMON-AVTO");
            Console.WriteLine($"Серийный номер: {_serialNumber}");
            Console.WriteLine($"IP устройства: {_deviceIp}");
            Console.WriteLine($"Сервер: {_host}:{_port}");
            Console.WriteLine();

            while (true)
            {
                try
                {
                    await RunSingleSessionAsync();
                    
                    // Пауза между сеансами связи (55-60 секунд как в реальных устройствах)
                    Console.WriteLine($"Ожидание 60 секунд до следующего сеанса...");
                    await Task.Delay(60000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в сеансе связи: {ex.Message}");
                    await Task.Delay(10000); // Пауза при ошибке
                }
            }
        }

        /// <summary>
        /// Выполнение одного сеанса связи
        /// </summary>
        private async Task RunSingleSessionAsync()
        {
            using (var client = new TcpClient())
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Подключение к серверу...");
                await client.ConnectAsync(_host, _port);
                
                var stream = client.GetStream();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Подключение установлено");

                // Шаг 1: Отправка Request Type 1 (идентификация)
                await SendIdentificationRequestAsync(stream);
                
                // Шаг 2: Получение Time Sync от сервера
                var timeSyncResponse = await ReceiveResponseAsync(stream);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Получен Time Sync: {timeSyncResponse}");

                // Шаг 3: Отправка Request Type 2 (данные устройства)
                await SendDataRequestAsync(stream);
                
                // Шаг 4: Получение подтверждения
                var confirmationResponse = await ReceiveResponseAsync(stream);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Получено подтверждение: {confirmationResponse}");

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Сеанс связи завершен успешно");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Отправка Request Type 1 - Идентификация устройства
        /// </summary>
        private async Task SendIdentificationRequestAsync(NetworkStream stream)
        {
            var request1 = new
            {
                request = 1,
                serial = _serialNumber,
                ip = _deviceIp
            };

            var json = JsonSerializer.Serialize(request1, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var messageWithTerminator = json + "~";
            var data = Encoding.UTF8.GetBytes(messageWithTerminator);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Отправка Request Type 1 (идентификация)");
            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();
        }

        /// <summary>
        /// Отправка Request Type 2 - Данные устройства
        /// </summary>
        private async Task SendDataRequestAsync(NetworkStream stream)
        {
            var currentTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            
            // Генерация тестовых данных датчиков
            var random = new Random();
            double temperature = 20.0 + random.NextDouble() * 10.0; // 20-30°C
            int humidity = 30 + random.Next(0, 50); // 30-80%
            int rssi = -80 + random.Next(0, 30); // -80 to -50 dBm
            int voltage = 3800 + random.Next(0, 300); // 3800-4100 mV

            var request2 = new
            {
                request = 2,
                serial = _serialNumber,
                ip = _deviceIp,
                device_name = "avto",
                comment = "",
                time_unix = currentTime,
                ssid = "TestNetwork",
                pass = "TestPassword123",
                fw = "1.25",
                arhive_type = 1,
                temp_now = Math.Round(temperature, 1),
                hum_now = humidity,
                temp_alarm = new
                {
                    alarm_flag = 0,
                    high = 0,
                    low = 0
                },
                hum_alarm = new
                {
                    alarm_flag = 0,
                    high = 0,
                    low = 0
                },
                alarm_delay = 0,
                rssi = rssi,
                voltage = voltage,
                measure_interval = 1,
                sending_interval = 1,
                sensor_type = "DTV01",
                data = new[]
                {
                    new
                    {
                        ts = currentTime - 10,
                        t = Math.Round(temperature - 0.5, 1),
                        h = humidity - 5
                    },
                    new
                    {
                        ts = currentTime - 5,
                        t = Math.Round(temperature - 0.2, 1),
                        h = humidity - 2
                    },
                    new
                    {
                        ts = currentTime,
                        t = Math.Round(temperature, 1),
                        h = humidity
                    }
                },
                data_size = 3,
                last_rec = 4583
            };

            var json = JsonSerializer.Serialize(request2, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var messageWithTerminator = json + "~";
            var data = Encoding.UTF8.GetBytes(messageWithTerminator);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Отправка Request Type 2 (данные устройства)");
            Console.WriteLine($"    Температура: {temperature:F1}°C");
            Console.WriteLine($"    Влажность: {humidity}%");
            Console.WriteLine($"    RSSI: {rssi} dBm");
            Console.WriteLine($"    Напряжение: {voltage} mV");
            
            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();
        }

        /// <summary>
        /// Получение ответа от сервера
        /// </summary>
        private async Task<string> ReceiveResponseAsync(NetworkStream stream)
        {
            var buffer = new byte[4096];
            var messageBuilder = new StringBuilder();
            
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                messageBuilder.Append(chunk);

                // Проверяем наличие завершающего символа
                if (chunk.Contains("~"))
                {
                    break;
                }
            }

            var fullMessage = messageBuilder.ToString();
            if (fullMessage.EndsWith("~"))
            {
                fullMessage = fullMessage.TrimEnd('~');
            }

            return fullMessage;
        }

        /// <summary>
        /// Запуск одиночного теста
        /// </summary>
        public static async Task RunSingleTestAsync(string serialNumber = "2023060019")
        {
            var client = new AnemonAvtoTestClient(serialNumber);
            await client.RunSingleSessionAsync();
        }

        /// <summary>
        /// Главная функция для запуска тестов
        /// </summary>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Тестовый клиент ANEMON-AVTO");
            Console.WriteLine("===========================");
            Console.WriteLine();

            if (args.Length > 0 && args[0] == "--single")
            {
                // Одиночный тест
                string serialNumber = args.Length > 1 ? args[1] : "2023060019";
                Console.WriteLine($"Запуск одиночного теста для устройства {serialNumber}");
                await RunSingleTestAsync(serialNumber);
            }
            else
            {
                // Непрерывная работа
                string serialNumber = args.Length > 0 ? args[0] : "2023060019";
                var client = new AnemonAvtoTestClient(serialNumber);
                await client.RunAsync();
            }
        }
    }
}
