// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon3.Logic.TagIndex
// Assembly: DrvAnemon3.Logic, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EDD75D42-CE83-4E55-B8C5-83132A68E0A4
// Assembly location: D:\RapidScada\DrvAnemon3\SCADA\ScadaComm\Drv\DrvAnemon3.Logic.dll

namespace Scada.Comm.Drivers.DrvAnemon3.Logic
{
    /// <summary>
    /// Индексы тегов для HTTP протокола версии 3
    /// </summary>
    internal static class TagIndex
    {
        // Основные теги устройства
        public const int PacketReceived = 0;    // Пакеты получено
        public const int PacketFailed = 1;      // Ошибки пакетов
        public const int LastUpdateTime = 2;    // Время последнего обновления
        
        // Теги датчиков начинаются с индекса 3
        public const int Sensors = 3;

        /// <summary>
        /// Получение индекса тега датчика
        /// </summary>
        /// <param name="sensorNum">Номер датчика (начинается с 1)</param>
        /// <returns>Индекс тега датчика</returns>
        public static int Sensor(int sensorNum)
        {
            return Sensors + sensorNum - 1;
        }
    }
}
