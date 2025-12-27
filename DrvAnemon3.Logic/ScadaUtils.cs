using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Config;

namespace Scada.Comm.Drivers.DrvAnemon3
{
    /// <summary>
    /// Утилиты Rapid SCADA для работы с конфигурацией
    /// </summary>
    public static class ScadaUtils
    {
        /// <summary>
        /// Получение целочисленного значения из списка опций
        /// </summary>
        public static int GetValueAsInt(object options, string key, int defaultValue)
        {
            if (options == null || string.IsNullOrEmpty(key))
                return defaultValue;

            try
            {
                // Простая реализация для демонстрации
                // В реальном драйвере здесь был бы доступ к настоящим опциям Rapid SCADA
                if (key == "DataLifetime")
                    return defaultValue;
                    
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Отдельная опция конфигурации
    /// </summary>
    public class Option
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Логгер коммуникации
    /// </summary>
    public interface ICommLogFormatter
    {
        /// <summary>
        /// Запись строки в лог
        /// </summary>
        void WriteLine(string message);
    }

    /// <summary>
    /// Реализация логгера коммуникации
    /// </summary>
    public class CommLogFormatter : ICommLogFormatter
    {
        private readonly ICommContext _commContext;
        private readonly string _title;

        public CommLogFormatter(ICommContext commContext, string title)
        {
            _commContext = commContext;
            _title = title;
        }

        public void WriteLine(string message)
        {
            Console.WriteLine($"[{_title}] {message}");
        }
    }

    /// <summary>
    /// Данные устройства
    /// </summary>
    public class DeviceData
    {
        private readonly Dictionary<int, object> _data = new Dictionary<int, object>();

        /// <summary>
        /// Установка значения
        /// </summary>
        public void Set(int tagIndex, object value)
        {
            _data[tagIndex] = value;
        }

        /// <summary>
        /// Получение значения
        /// </summary>
        public object Get(int tagIndex)
        {
            return _data.ContainsKey(tagIndex) ? _data[tagIndex] : 0.0;
        }

        /// <summary>
        /// Инвалидация тегов
        /// </summary>
        public void Invalidate(int startTag, int endTag)
        {
            for (int i = startTag; i <= endTag; i++)
            {
                _data[i] = double.NaN;
            }
        }

        /// <summary>
        /// Установка статуса тега
        /// </summary>
        public void SetStatusTag(DeviceStatus status)
        {
            _data[999] = (int)status; // Специальный тег для статуса
        }
    }

    /// <summary>
    /// Теги устройства
    /// </summary>
    public class DeviceTags
    {
        private readonly List<TagGroup> _groups = new List<TagGroup>();

        /// <summary>
        /// Добавление группы тегов
        /// </summary>
        public void AddGroup(TagGroup group)
        {
            _groups.Add(group);
        }
    }

    /// <summary>
    /// Группа тегов
    /// </summary>
    public class TagGroup
    {
        public string Name { get; set; } = string.Empty;
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }

    /// <summary>
    /// Тег
    /// </summary>
    public class Tag
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Статистика устройства
    /// </summary>
    public class DeviceStats
    {
        public int SessionCount { get; set; }
        public int SessionErrors { get; set; }
        public int RequestCount { get; set; }
        public int RequestErrors { get; set; }
    }

    /// <summary>
    /// Телекоманда
    /// </summary>
    public class TeleCommand
    {
        public int CmdNum { get; set; }
        public string CmdCode { get; set; } = string.Empty;
        public List<CmdData> CmdDataList { get; set; } = new List<CmdData>();
    }

    /// <summary>
    /// Данные команды
    /// </summary>
    public class CmdData
    {
        public int CnlNum { get; set; }
        public object Val { get; set; }
    }
}
