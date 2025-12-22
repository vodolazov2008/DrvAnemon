// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon.DriverUtils
// Assembly: DrvAnemon.Logic, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EDD75D42-CE83-4E55-B8C5-83132A68E0A4
// Assembly location: D:\RapidScada\DrvAnemon\SCADA\ScadaComm\Drv\DrvAnemon.Logic.dll

namespace Scada.Comm.Drivers.DrvAnemon
{
    /// <summary>
    /// Утилиты драйвера Anemon HTTP протокола версии 3
    /// </summary>
    public static class DriverUtils
    {
        public const string DriverCode = "DrvAnemon";
        public const int MaxSensorCnt = 256;
        
        // Константы для HTTP протокола версии 3
        public const string HttpProtocolVersion = "3";
        public const int DefaultHttpPort = 7120;
        public const string DefaultDeviceToken = "3A:E7:4E:00:95:1E";
        public const string JsonContentType = "application/x-www-form-urlencoded";
        
        // Заголовки HTTP
        public const string AnemonProtocolHeader = "Anemon-Protocol";
        public const string AnemonTokenHeader = "Anemon-Token";
        public const string ContentLengthHeader = "Content-Length";
        public const string ContentTypeHeader = "Content-Type";
        
        // Команды протокола
        public const string GetLastTsCommand = "get_last_ts";
        public const string GetLastTsResponse = "get_last_ts_resp";
    }
}
