using Scada.Comm.Config;
using Scada.Comm.Channels;
using Scada.Comm.Devices;
using Scada.Config;

namespace Scada.Comm.Drivers.DrvAnemon
{
    // Интерфейсы Rapid SCADA для драйвера DrvAnemon
    // Этот файл предоставляет псевдонимы для удобства разработки
    
    /// <summary>
    /// Псевдоним для ICommContext из Rapid SCADA
    /// </summary>
    public interface ICommContext
    {
        // Базовые методы контекста коммуникации
    }

    /// <summary>
    /// Псевдоним для ILineContext из Rapid SCADA
    /// </summary>
    public interface ILineContext
    {
        /// <summary>
        /// Конфигурация линии связи
        /// </summary>
        LineConfig LineConfig { get; }
    }

    /// <summary>
    /// Псевдоним для DriverLogic из Rapid SCADA
    /// </summary>
    public abstract class DriverLogic
    {
        /// <summary>
        /// Контекст коммуникации
        /// </summary>
        protected ICommContext CommContext { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        protected DriverLogic(ICommContext commContext)
        {
            CommContext = commContext;
        }

        /// <summary>
        /// Код драйвера
        /// </summary>
        public abstract string Code { get; }

        /// <summary>
        /// Создание логики устройства
        /// </summary>
        public abstract DeviceLogic CreateDevice(ILineContext lineContext, DeviceConfig deviceConfig);
    }

    /// <summary>
    /// Псевдоним для DeviceLogic из Rapid SCADA
    /// </summary>
    public abstract class DeviceLogic
    {
        /// <summary>
        /// Контекст коммуникации
        /// </summary>
        protected ICommContext CommContext { get; }

        /// <summary>
        /// Контекст линии связи
        /// </summary>
        protected ILineContext LineContext { get; }

        /// <summary>
        /// Конфигурация устройства
        /// </summary>
        protected DeviceConfig DeviceConfig { get; }

        /// <summary>
        /// Требуется ли соединение
        /// </summary>
        public bool ConnectionRequired { get; set; } = true;

        /// <summary>
        /// Заголовок устройства
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Статус устройства
        /// </summary>
        public DeviceStatus DeviceStatus { get; set; }

        /// <summary>
        /// Данные устройства
        /// </summary>
        public DeviceData DeviceData { get; }

        /// <summary>
        /// Статистика устройства
        /// </summary>
        public DeviceStats DeviceStats { get; }

        /// <summary>
        /// Теги устройства
        /// </summary>
        public DeviceTags DeviceTags { get; }

        /// <summary>
        /// Логгер
        /// </summary>
        public ICommLogFormatter Log { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        protected DeviceLogic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
        {
            CommContext = commContext;
            LineContext = lineContext;
            DeviceConfig = deviceConfig;
            Title = "Device"; // Используем простое имя для демонстрации
            DeviceData = new DeviceData();
            DeviceStats = new DeviceStats();
            DeviceTags = new DeviceTags();
            Log = new CommLogFormatter(CommContext, Title);
        }

        /// <summary>
        /// Загрузка конфигурации при старте линии связи
        /// </summary>
        public virtual void OnCommLineStart() { }

        /// <summary>
        /// Проверка поддержки поведения канала
        /// </summary>
        public virtual bool CheckBehaviorSupport(ChannelBehavior behavior) => false;

        /// <summary>
        /// Инициализация тегов устройства
        /// </summary>
        public virtual void InitDeviceTags() { }

        /// <summary>
        /// Инициализация данных устройства
        /// </summary>
        public virtual void InitDeviceData() { }

        /// <summary>
        /// Выполнение сеанса связи
        /// </summary>
        public virtual void Session() { }

        /// <summary>
        /// Отправка команды
        /// </summary>
        public virtual void SendCommand(TeleCommand cmd) { }
    }

    /// <summary>
    /// Статус устройства
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// Неопределен
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Нормальный
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Ошибка
        /// </summary>
        Error = 2
    }

    /// <summary>
    /// Поведение канала
    /// </summary>
    public enum ChannelBehavior
    {
        /// <summary>
        /// Мастер
        /// </summary>
        Master = 0,

        /// <summary>
        /// Слейв
        /// </summary>
        Slave = 1
    }
}
