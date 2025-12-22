// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon.Logic.DrvAnemonLogic
// Assembly: DrvAnemon.Logic, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EDD75D42-CE83-4E55-B8C5-83132A68E0A4
// Assembly location: D:\RapidScada\DrvAnemon\SCADA\ScadaComm\Drv\DrvAnemon.Logic.dll

using Scada.Comm.Config;
using Scada.Comm.Devices;

#nullable disable
namespace Scada.Comm.Drivers.DrvAnemon.Logic;

public class DrvAnemonLogic(ICommContext commContext) : DriverLogic(commContext)
{
  public virtual string Code => "DrvAnemon";

  public virtual DeviceLogic CreateDevice(ILineContext lineContext, DeviceConfig deviceConfig)
  {
    return (DeviceLogic) new DevAnemonLogic(this.CommContext, lineContext, deviceConfig);
  }
}
