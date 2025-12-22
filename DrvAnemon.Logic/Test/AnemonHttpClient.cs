using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Comm.Drivers.DrvAnemon.Test
{
    /// <summary>
    /// Тестовый клиент для HTTP протокола Anemon версии 3
    /// </summary>
    public class AnemonHttpClient
    {
        private readonly string _serverUrl;
        private readonly string _deviceToken;

        public AnemonHttpClient(string serverUrl = "http://176.213.136.69:7120", string deviceToken = "3A:E7:4E:00:95:1E")
        {
            _serverUrl = serverUrl;
            _deviceToken = deviceToken;
        }

        /// <summary>
        /// Отправка команды get_last_ts
        /// </summary>
        public async Task<string> SendGetLastTsCommandAsync(string serialNumber, int saveInterval = 600)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Формирование JSON команды
                    var command = new
                    {
                        cmd = "get_last_ts",
                        data = new[]
                        {
                            new { serial = serialNumber, save_interval = saveInterval }
                        }
                    };

                    var jsonContent = System.Text.Json.JsonSerializer.Serialize(command);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Добавление заголовков протокола
                    client.DefaultRequestHeaders.Add("Anemon-Protocol", "3");
                    client.DefaultRequestHeaders.Add("Anemon-Token", _deviceToken);
                    client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

                    // Отправка POST запроса
                    var response = await client.PostAsync(_serverUrl, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"Статус ответа: {response.StatusCode}");
                    Console.WriteLine($"Ответ сервера: {responseContent}");

                    return responseContent;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки команды: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Тестирование команды get_last_ts
        /// </summary>
        public static async Task TestGetLastTsCommand()
        {
            Console.WriteLine("=== Тестирование HTTP протокола Anemon версии 3 ===");
            
            var client = new AnemonHttpClient();
            
            // Тест с примером из задания
            string testSerial = "2023060019";
            Console.WriteLine($"Отправка команды для устройства: {testSerial}");
            
            var response = await client.SendGetLastTsCommandAsync(testSerial);
            
            if (response != null)
            {
                Console.WriteLine("Команда успешно отправлена");
                Console.WriteLine($"Ожидаемый ответ: {{\"cmd\":\"get_last_ts_resp\",\"ts\":1766418214,\"data\":[{{\"serial\":\"{testSerial}\",\"last_ts\":0}}]}}");
            }
            else
            {
                Console.WriteLine("Ошибка отправки команды");
            }
        }
    }

    /// <summary>
    /// Программа для тестирования
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            await AnemonHttpClient.TestGetLastTsCommand();
            Console.WriteLine("Тестирование завершено. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
