// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon.Logic.DrvAnemonLogic
// Assembly: DrvAnemon.Logic, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EDD75D42-CE83-4E55-B8C5-83132A68E0A4
// Assembly location: D:\RapidScada\DrvAnemon\SCADA\ScadaComm\Drv\DrvAnemon.Logic.dll

using Scada.Comm.Config;
using Scada.Comm.Devices;

namespace Scada.Comm.Drivers.DrvAnemon.Logic
{
    /// <summary>
    /// Логика драйвера Anemon для HTTP протокола версии 3
    /// </summary>
    public class DrvAnemonLogic : DriverLogic
    {
        public DrvAnemonLogic(ICommContext commContext)
            : base(commContext)
        {
        }

        /// <summary>
        /// Код драйвера
        /// </summary>
        public override string Code => "DrvAnemon";

        /// <summary>
        /// Создание устройства
        /// </summary>
        public override DeviceLogic CreateDevice(ILineContext lineContext, DeviceConfig deviceConfig)
        {
            return new DevAnemonLogic(CommContext, lineContext, deviceConfig);
        }
    }
}
