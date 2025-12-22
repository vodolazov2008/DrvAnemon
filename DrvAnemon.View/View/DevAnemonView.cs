// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon.View.DevAnemonView
// Assembly: DrvAnemon.View, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 32160B11-AF08-44A7-A737-2520F961A88B
// Assembly location: D:\RapidScada\DrvAnemon\SCADA\ScadaAdmin\Lib\DrvAnemon.View.dll

using Scada.Comm.Config;
using Scada.Comm.Devices;
using System.Collections.Generic;

#nullable disable
namespace Scada.Comm.Drivers.DrvAnemon.View;

internal class DevAnemonView(
  DriverView parentView,
  LineConfig lineConfig,
  DeviceConfig deviceConfig) : DeviceView(parentView, lineConfig, deviceConfig)
{
  public virtual PollingOptions GetPollingOptions() => new PollingOptions(3000, 200);

  public virtual ICollection<CnlPrototype> GetCnlPrototypes()
  {
    return (ICollection<CnlPrototype>) CommUtils.GetCnlPrototypes(CnlPrototypeFactory.GetCnlPrototypeGroups());
  }
}
