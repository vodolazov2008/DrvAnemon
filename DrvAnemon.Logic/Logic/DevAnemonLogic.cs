using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Config;
using Scada.Data.Const;
using Scada.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scada.Comm.Drivers.DrvAnemon.Logic
{
    /// <summary>
    /// Логика устройства Anemon для HTTP протокола версии 3
    /// </summary>
    internal class DevAnemonLogic : DeviceLogic
    {
        private const int DefDataLifetime = 600;
        private const int DefaultHttpPort = 7120;
        private const string DefaultToken = "3A:E7:4E:00:95:1E";
        
        private readonly Dictionary<string, DeviceDataStore> _deviceDataStore;
        private readonly object _dataStoreLock = new object();
        private HttpServer _httpServer;
        private TimeSpan _dataLifetime;
        private int _httpPort;
        private string _deviceToken;
        private bool _isServerRunning;
        private DateTime _lastSessionTime;
        private bool _lastRequestOK;

        public DevAnemonLogic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
            : base(commContext, lineContext, deviceConfig)
        {
            ConnectionRequired = false;
            _deviceDataStore = new Dictionary<string, DeviceDataStore>();
            _dataLifetime = TimeSpan.FromSeconds(DefDataLifetime);
            _httpPort = DefaultHttpPort;
            _deviceToken = DefaultToken;
            _isServerRunning = false;
            _lastSessionTime = DateTime.MinValue;
            _lastRequestOK = false;
        }

        /// <summary>
        /// Запуск HTTP сервера при старте линии связи
        /// </summary>
        public override void OnCommLineStart()
        {
            base.OnCommLineStart();
            
            // Загрузка конфигурации
            OptionList customOptions = LineContext.LineConfig.CustomOptions;
            _dataLifetime = TimeSpan.FromSeconds(ScadaUtils.GetValueAsInt(customOptions, "DataLifetime", DefDataLifetime));
            _httpPort = ScadaUtils.GetValueAsInt(customOptions, "HttpPort", DefaultHttpPort);
            _deviceToken = ScadaUtils.GetValueAsString(customOptions, "DeviceToken", DefaultToken);

            // Запуск HTTP сервера
            _ = Task.Run(async () =>
            {
                try
                {
                    _httpServer = new HttpServer(_httpPort, this);
                    _isServerRunning = true;
                    Log.WriteLine($"Запуск HTTP сервера на порту {_httpPort} для протокола Anemon v3");
                    await _httpServer.StartAsync();
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"Ошибка запуска HTTP сервера: {ex.Message}");
                    _isServerRunning = false;
                }
            });
        }

        /// <summary>
        /// Проверка поддержки поведения канала
        /// </summary>
        public override bool CheckBehaviorSupport(ChannelBehavior behavior)
        {
            return behavior == ChannelBehavior.Server;
        }

        /// <summary>
        /// Инициализация тегов устройства
        /// </summary>
        public override void InitDeviceTags()
        {
            foreach (CnlPrototypeGroup cnlPrototypeGroup in CnlPrototypeFactory.GetCnlPrototypeGroups())
            {
                DeviceTags.AddGroup(cnlPrototypeGroup.ToTagGroup());
            }
        }

        /// <summary>
        /// Инициализация данных устройства
        /// </summary>
        public override void InitDeviceData()
        {
            base.InitDeviceData();
            DeviceData.Set(0, 0.0); // Пакеты получено
            DeviceData.Set(1, 0.0); // Ошибки
            DeviceData.Set(2, DateTime.UtcNow); // Время последнего обновления
        }

        /// <summary>
        /// Выполнение сеанса связи
        /// </summary>
        public override void Session()
        {
            base.Session();

            // Обновление статистики
            int packetReceived = (int)DeviceData.Get(0);
            int packetFailed = (int)DeviceData.Get(1);
            
            DeviceStats.SessionCount = DeviceStats.RequestCount = packetReceived + packetFailed;
            DeviceStats.SessionErrors = DeviceStats.RequestErrors = packetFailed;

            // Проверка активности устройства
            if (DateTime.UtcNow - _lastSessionTime <= _dataLifetime)
            {
                DeviceStatus = _lastRequestOK ? DeviceStatus.Normal : DeviceStatus.Error;
            }
            else
            {
                if (DeviceStatus == DeviceStatus.Normal)
                {
                    Log.WriteLine($"Установка недостоверности текущих данных для {Title}");
                    DeviceData.Invalidate(3, 258); // Инвалидация тегов датчиков
                }
                DeviceStatus = _lastSessionTime > DateTime.MinValue ? DeviceStatus.Error : DeviceStatus.Off;
            }

            DeviceData.SetStatusTag(DeviceStatus);
        }

        /// <summary>
        /// Обработка входящего HTTP запроса
        /// </summary>
        public void ProcessHttpRequest(string requestBody, string token)
        {
            try
            {
                _lastSessionTime = DateTime.UtcNow;
                
                // Проверка токена
                if (!ValidateToken(token))
                {
                    Log.WriteLine("Ошибка: недействительный токен");
                    IncrementErrorCount();
                    _lastRequestOK = false;
                    return;
                }

                // Обработка команды
                var response = ProtocolV3.ProcessCommand(requestBody, token, this);
                
                if (!string.IsNullOrEmpty(response))
                {
                    IncrementSuccessCount();
                    _lastRequestOK = true;
                    Log.WriteLine($"HTTP команда обработана успешно");
                }
                else
                {
                    IncrementErrorCount();
                    _lastRequestOK = false;
                    Log.WriteLine("Ошибка обработки HTTP команды");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Ошибка обработки HTTP запроса: {ex.Message}");
                IncrementErrorCount();
                _lastRequestOK = false;
            }
        }

        /// <summary>
        /// Регистрация данных устройства
        /// </summary>
        public void RegisterDeviceData(string serial, long timestamp, Dictionary<int, double> sensorValues)
        {
            if (string.IsNullOrEmpty(serial))
                return;

            lock (_dataStoreLock)
            {
                if (!_deviceDataStore.ContainsKey(serial))
                {
                    _deviceDataStore[serial] = new DeviceDataStore { Serial = serial };
                }

                var deviceData = _deviceDataStore[serial];
                deviceData.LastTimestamp = timestamp;
                deviceData.SensorValues = sensorValues ?? new Dictionary<int, double>();
                deviceData.LastUpdateTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Получение последней временной метки устройства
        /// </summary>
        public long? GetLastTimestamp(string serial)
        {
            if (string.IsNullOrEmpty(serial))
                return null;

            lock (_dataStoreLock)
            {
                return _deviceDataStore.ContainsKey(serial) ? _deviceDataStore[serial].LastTimestamp : null;
            }
        }

        /// <summary>
        /// Валидация токена устройства
        /// </summary>
        private bool ValidateToken(string token)
        {
            return !string.IsNullOrEmpty(token) && token.Equals(_deviceToken, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Увеличение счетчика успешных пакетов
        /// </summary>
        private void IncrementSuccessCount()
        {
            double currentCount = DeviceData.Get(0);
            DeviceData.Set(0, currentCount + 1);
        }

        /// <summary>
        /// Увеличение счетчика ошибок
        /// </summary>
        private void IncrementErrorCount()
        {
            double currentCount = DeviceData.Get(1);
            DeviceData.Set(1, currentCount + 1);
        }

        /// <summary>
        /// Завершение запроса
        /// </summary>
        private void FinishRequest(bool success)
        {
            DeviceData.Add(success ? 0 : 1, 1.0);
            _lastRequestOK = success;
            _lastSessionTime = DateTime.UtcNow;

            if (success)
            {
                DeviceData.Set(2, DateTime.UtcNow); // Обновление времени
            }
        }

        /// <summary>
        /// Завершение сеанса
        /// </summary>
        private void FinishSession()
        {
            // Дополнительная логика завершения сеанса при необходимости
        }

        /// <summary>
        /// Завершение команды
        /// </summary>
        private void FinishCommand()
        {
            // Логика завершения команды при необходимости
        }

        /// <summary>
        /// Отправка команды (заглушка для совместимости)
        /// </summary>
        public override void SendCommand(TeleCommand cmd)
        {
            base.SendCommand(cmd);
            Log.WriteLine("HTTP протокол версии 3 не поддерживает отправку команд от сервера к устройству");
            FinishCommand();
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpServer?.Stop();
                _httpServer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Хранилище данных устройства
    /// </summary>
    internal class DeviceDataStore
    {
        public string Serial { get; set; }
        public long LastTimestamp { get; set; }
        public Dictionary<int, double> SensorValues { get; set; } = new Dictionary<int, double>();
        public DateTime LastUpdateTime { get; set; }
    }
}
