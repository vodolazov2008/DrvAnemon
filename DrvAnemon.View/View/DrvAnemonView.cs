// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon.View.DrvAnemonView
// Assembly: DrvAnemon.View, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 32160B11-AF08-44A7-A737-2520F961A88B
// Assembly location: D:\RapidScada\DrvAnemon\SCADA\ScadaAdmin\Lib\DrvAnemon.View.dll

using Scada.Comm.Config;
using Scada.Comm.Devices;

#nullable disable
namespace Scada.Comm.Drivers.DrvAnemon.View;

public class DrvAnemonView : DriverView
{
  public DrvAnemonView() => this.CanCreateDevice = true;

  public virtual string Name => "Анемон";

  public virtual string Descr
  {
    get
    {
      return "Принимает данные от контроллеров Анемон производства Дисистех.\n\nПараметры канала связи:\nТип: TCP-сервер,\nПоведение: Slave,\nРежим соединения: Индивидуальное,\nСопоставление устройств: Определяется драйвером.\n\nПользовательские параметры линии связи:\nDataLifetime - время актуальности текущих данных, с. По умолчанию 600.\nProtocolVersion - версия протокола, 1 или 2. По умолчанию 1.\n\nПараметр устройства:\nСтроковый адрес - идентификатор контроллера.";
    }
  }

  public virtual DeviceView CreateDeviceView(LineConfig lineConfig, DeviceConfig deviceConfig)
  {
    return (DeviceView) new DevAnemonView((DriverView) this, lineConfig, deviceConfig);
  }
}
