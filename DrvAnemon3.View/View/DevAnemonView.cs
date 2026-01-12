// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon3.View.DevAnemonView
// Assembly: DrvAnemon3.View, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 32160B11-AF08-44A7-A737-2520F961A88B
// Assembly location: D:\RapidScada\DrvAnemon3\SCADA\ScadaAdmin\Lib\DrvAnemon3.View.dll

using Scada.Comm;
using Scada.Comm.Config;
using Scada.Comm.Devices;
using System.Collections.Generic;

namespace Scada.Comm.Drivers.DrvAnemon3.View;

internal class DevAnemonView : DeviceView
{
    public DevAnemonView(DriverView parentView, LineConfig lineConfig, DeviceConfig deviceConfig) 
        : base(parentView, lineConfig, deviceConfig)
    {
    }

    public override PollingOptions GetPollingOptions() => new PollingOptions(3000, 200);

    public override ICollection<CnlPrototype> GetCnlPrototypes()
    {
        return CnlPrototypeFactory.GetCnlPrototypeGroups().GetCnlPrototypes();
    }
}
