// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon3.Logic.DrvAnemon3Logic
// Assembly: DrvAnemon3.Logic, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EDD75D42-CE83-4E55-B8C5-83132A68E0A4
// Assembly location: D:\RapidScada\DrvAnemon3\SCADA\ScadaComm\Drv\DrvAnemon3.Logic.dll

using Scada.Comm.Config;
using Scada.Comm.Devices;

namespace Scada.Comm.Drivers.DrvAnemon3.Logic
{
    /// <summary>
    /// Логика драйвера Anemon для HTTP протокола версии 3
    /// </summary>
    public class DrvAnemon3Logic : DriverLogic
    {
        public DrvAnemon3Logic(ICommContext commContext)
            : base(commContext)
        {
        }

        /// <summary>
        /// Код драйвера
        /// </summary>
        public override string Code => "DrvAnemon3";

        /// <summary>
        /// Создание устройства
        /// </summary>
        public override DeviceLogic CreateDevice(ILineContext lineContext, DeviceConfig deviceConfig)
        {
            return new DevAnemonLogic(CommContext, lineContext, deviceConfig);
        }
    }
}
