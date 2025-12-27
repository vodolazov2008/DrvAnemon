using Scada.Comm.Config;
using Scada.Comm.Channels;
using Scada.Comm.Drivers.DrvAnemon;
using Scada.Comm.Devices;
using Scada.Config;
using Scada.Data.Const;
using Scada.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scada.Comm.Drivers.DrvAnemon.Logic
{
    /// <summary>
    /// Логика устройства ANEMON-AVTO для TCP протокола
    /// </summary>
    internal class DevAnemonLogic : DeviceLogic
    {
        private const int DefDataLifetime = 600;
        private const int DefaultTcpPort = 4004;
        
        private readonly Dictionary<string, DeviceDataStore> _deviceDataStore;
        private readonly object _dataStoreLock = new object();
        private readonly Dictionary<string, List<byte>> _fragmentedData; // Для обработки фрагментации
        private readonly object _fragmentLock = new object();
        
        private TimeSpan _dataLifetime;
        private int _tcpPort;
        private DateTime _lastSessionTime;
        private bool _lastRequestOK;

        public DevAnemonLogic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
            : base(commContext, lineContext, deviceConfig)
        {
            ConnectionRequired = false; // TCP сервер управляет соединениями
            _deviceDataStore = new Dictionary<string, DeviceDataStore>();
            _fragmentedData = new Dictionary<string, List<byte>>();
            _dataLifetime = TimeSpan.FromSeconds(DefDataLifetime);
            _tcpPort = DefaultTcpPort;
            _lastSessionTime = DateTime.MinValue;
            _lastRequestOK = false;
        }

        /// <summary>
        /// Загрузка конфигурации при старте линии связи
        /// </summary>
        public override void OnCommLineStart()
        {
            base.OnCommLineStart();
            
            // Загрузка конфигурации из свойств линии связи
            var customOptions = LineContext.LineConfig.CustomOptions;
            _dataLifetime = TimeSpan.FromSeconds(ScadaUtils.GetValueAsInt(customOptions, "DataLifetime", DefDataLifetime));
            // Для TCP сервера используем порт по умолчанию, так как LineConfig может не содержать свойство порта
            _tcpPort = DefaultTcpPort;
            
            Log.WriteLine($"Драйвер ANEMON-AVTO запущен на TCP порту {_tcpPort}");
            Log.WriteLine($"Время актуальности данных: {_dataLifetime.TotalSeconds} секунд");
        }

        /// <summary>
        /// Проверка поддержки поведения канала
        /// </summary>
        public override bool CheckBehaviorSupport(ChannelBehavior behavior)
        {
            return behavior == ChannelBehavior.Slave; // Поддерживаем только TCP сервер
        }

        /// <summary>
        /// Инициализация тегов устройства
        /// </summary>
        public override void InitDeviceTags()
        {
            foreach (CnlPrototypeGroup cnlPrototypeGroup in CnlPrototypeFactory.GetCnlPrototypeGroups())
            {
                // Создаем простую группу тегов для демонстрации
                var tagGroup = new TagGroup { Name = cnlPrototypeGroup.Name };
                DeviceTags.AddGroup(tagGroup);
            }
        }

        /// <summary>
        /// Инициализация данных устройства
        /// </summary>
        public override void InitDeviceData()
        {
            base.InitDeviceData();
            DeviceData.Set(0, 0.0); // Количество полученных пакетов
            DeviceData.Set(1, 0.0); // Количество ошибок
            DeviceData.Set(2, (double)DateTime.UtcNow.Ticks); // Время последнего обновления в тиках
        }

        /// <summary>
        /// Выполнение сеанса связи
        /// </summary>
        public override void Session()
        {
            base.Session();

            // Обновление статистики
            int packetReceived = Convert.ToInt32(DeviceData.Get(0));
            int packetFailed = Convert.ToInt32(DeviceData.Get(1));
            
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
                DeviceStatus = _lastSessionTime > DateTime.MinValue ? DeviceStatus.Error : DeviceStatus.Undefined;
            }

            DeviceData.SetStatusTag(DeviceStatus);
        }

        /// <summary>
        /// Обработка входящих TCP данных от устройства
        /// </summary>
        /// <param name="connectionId">Идентификатор соединения</param>
        /// <param name="data">Данные от устройства</param>
        /// <returns>Ответные данные для отправки (null если ответа нет)</returns>
        public byte[] ProcessTcpData(string connectionId, byte[] data)
        {
            try
            {
                _lastSessionTime = DateTime.UtcNow;
                
                // Обработка фрагментации данных
                var completeMessage = AssembleFragmentedMessage(connectionId, data);
                if (completeMessage == null)
                {
                    // Сообщение еще не полное, ожидаем следующий фрагмент
                    return null;
                }

                // Обработка сообщения через протокол ANEMON-AVTO
                var response = AnemonAvtoProtocol.ProcessTcpData(completeMessage, this, (msg) => Log.WriteLine(msg));
                
                if (!string.IsNullOrEmpty(response))
                {
                    IncrementSuccessCount();
                    _lastRequestOK = true;
                    
                    // Подготовка ответа с завершающим символом
                    var responseBytes = AnemonAvtoProtocol.PrepareDataForSending(response);
                    Log.WriteLine($"Обработка TCP данных завершена успешно, ответ: {response.Length} байт");
                    return responseBytes;
                }
                else
                {
                    IncrementErrorCount();
                    _lastRequestOK = false;
                    Log.WriteLine("Ошибка обработки TCP команды");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Ошибка обработки TCP данных: {ex.Message}");
                IncrementErrorCount();
                _lastRequestOK = false;
                return null;
            }
        }

        /// <summary>
        /// Обработка фрагментированных TCP сообщений
        /// </summary>
        private byte[] AssembleFragmentedMessage(string connectionId, byte[] data)
        {
            lock (_fragmentLock)
            {
                if (!_fragmentedData.ContainsKey(connectionId))
                {
                    _fragmentedData[connectionId] = new List<byte>();
                }

                var connectionBuffer = _fragmentedData[connectionId];
                connectionBuffer.AddRange(data);

                // Проверяем наличие завершающего символа '~'
                string messageText = Encoding.UTF8.GetString(connectionBuffer.ToArray());
                
                if (messageText.Contains("~"))
                {
                    // Найден завершающий символ, извлекаем полное сообщение
                    int terminatorIndex = messageText.IndexOf('~');
                    byte[] completeMessage = new byte[terminatorIndex];
                    Array.Copy(connectionBuffer.ToArray(), completeMessage, terminatorIndex);
                    
                    // Очищаем буфер для этого соединения
                    connectionBuffer.Clear();
                    
                    return completeMessage;
                }

                // Сообщение еще не полное
                return null;
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

                // Обновление тегов в Rapid SCADA
                UpdateDeviceTags(sensorValues ?? new Dictionary<int, double>());
            }
        }

        /// <summary>
        /// Обновление тегов устройства в Rapid SCADA
        /// </summary>
        private void UpdateDeviceTags(Dictionary<int, double> sensorValues)
        {
            if (sensorValues == null)
                return;

            foreach (var kvp in sensorValues)
            {
                int tagIndex = kvp.Key;
                double value = kvp.Value;
                
                // Проверяем диапазон тегов (начинаем с 3)
                if (tagIndex >= 3 && tagIndex < 1000)
                {
                    DeviceData.Set(tagIndex, value);
                }
            }

            // Обновление времени последнего обновления
            DeviceData.Set(2, (double)DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// Установка строкового тега
        /// </summary>
        public void SetStringTag(int tagIndex, string value)
        {
            if (tagIndex >= 3 && tagIndex < 1000)
            {
                DeviceData.Set(tagIndex, value);
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
        /// Увеличение счетчика успешных пакетов
        /// </summary>
        private void IncrementSuccessCount()
        {
            double currentCount = Convert.ToDouble(DeviceData.Get(0));
            DeviceData.Set(0, currentCount + 1);
        }

        /// <summary>
        /// Увеличение счетчика ошибок
        /// </summary>
        private void IncrementErrorCount()
        {
            double currentCount = Convert.ToDouble(DeviceData.Get(1));
            DeviceData.Set(1, currentCount + 1);
        }

        /// <summary>
        /// Обработка закрытия соединения
        /// </summary>
        public void OnConnectionClosed(string connectionId)
        {
            lock (_fragmentLock)
            {
                _fragmentedData.Remove(connectionId);
            }
            Log.WriteLine($"TCP соединение {connectionId} закрыто");
        }

        /// <summary>
        /// Отправка команды (заглушка для совместимости)
        /// </summary>
        public override void SendCommand(TeleCommand cmd)
        {
            base.SendCommand(cmd);
            Log.WriteLine("TCP протокол ANEMON-AVTO не поддерживает отправку команд от сервера к устройству");
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            lock (_fragmentLock)
            {
                _fragmentedData.Clear();
            }
        }
    }

    /// <summary>
    /// Хранилище данных устройства
    /// </summary>
    internal class DeviceDataStore
    {
        public string Serial { get; set; } = string.Empty;
        public long LastTimestamp { get; set; }
        public Dictionary<int, double> SensorValues { get; set; } = new Dictionary<int, double>();
        public DateTime LastUpdateTime { get; set; }
    }
}
