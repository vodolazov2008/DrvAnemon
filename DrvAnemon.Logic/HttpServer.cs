using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Scada.Comm.Drivers.DrvAnemon.Logic;

namespace Scada.Comm.Drivers.DrvAnemon
{
    /// <summary>
    /// HTTP сервер для приема запросов от устройств по протоколу Anemon версии 3
    /// </summary>
    internal class HttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly int _port;
        private readonly DevAnemonLogic _deviceLogic;
        private bool _isRunning;

        public HttpServer(int port, DevAnemonLogic deviceLogic)
        {
            _port = port;
            _deviceLogic = deviceLogic;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{_port}/");
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Запуск HTTP сервера
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                _listener.Start();
                _isRunning = true;
                _deviceLogic?.Log?.WriteLine($"HTTP сервер запущен на порту {_port}");

                while (_isRunning && _listener.IsListening)
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context), _cancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                _deviceLogic?.Log?.WriteLine($"Ошибка запуска HTTP сервера: {ex.Message}");
            }
        }

        /// <summary>
        /// Остановка HTTP сервера
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _cancellationTokenSource.Cancel();
            _listener?.Stop();
            _deviceLogic?.Log?.WriteLine("HTTP сервер остановлен");
        }

        /// <summary>
        /// Обработка HTTP запроса
        /// </summary>
        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                if (context.Request.HttpMethod == "POST")
                {
                    await HandlePostRequestAsync(context);
                }
                else
                {
                    SendResponse(context, HttpStatusCode.MethodNotAllowed, "Method Not Allowed");
                }
            }
            catch (Exception ex)
            {
                _deviceLogic?.Log?.WriteLine($"Ошибка обработки HTTP запроса: {ex.Message}");
                SendResponse(context, HttpStatusCode.InternalServerError, "Internal Server Error");
            }
        }

        /// <summary>
        /// Обработка POST запроса
        /// </summary>
        private async Task HandlePostRequestAsync(HttpListenerContext context)
        {
            // Проверка заголовков протокола
            var protocolVersion = context.Request.Headers["Anemon-Protocol"];
            var token = context.Request.Headers["Anemon-Token"];
            var contentType = context.Request.Headers["Content-Type"];

            if (protocolVersion != "3")
            {
                SendResponse(context, HttpStatusCode.BadRequest, "Invalid Anemon-Protocol header");
                return;
            }

            if (token == null)
            {
                SendResponse(context, HttpStatusCode.BadRequest, "Missing Anemon-Token header");
                return;
            }

            if (contentType != "application/x-www-form-urlencoded")
            {
                SendResponse(context, HttpStatusCode.BadRequest, "Invalid Content-Type header");
                return;
            }

            // Чтение тела запроса
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                string requestBody = await reader.ReadToEndAsync();
                
                // Передача запроса логике устройства для обработки
                _deviceLogic.ProcessHttpRequest(requestBody, token);
                
                // Отправка успешного ответа
                SendJsonResponse(context, "{\"status\":\"success\",\"ts\":" + 
                    (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + "}");
            }
        }

        /// <summary>
        /// Отправка JSON ответа
        /// </summary>
        private void SendJsonResponse(HttpListenerContext context, string jsonResponse)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
            
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.StatusCode = 200;
            
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        /// <summary>
        /// Отправка текстового ответа
        /// </summary>
        private void SendResponse(HttpListenerContext context, HttpStatusCode statusCode, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            Stop();
            _listener?.Close();
            _cancellationTokenSource?.Dispose();
        }
    }
}
