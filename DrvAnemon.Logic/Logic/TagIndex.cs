// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon.Logic.TagIndex
// Assembly: DrvAnemon.Logic, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EDD75D42-CE83-4E55-B8C5-83132A68E0A4
// Assembly location: D:\RapidScada\DrvAnemon\SCADA\ScadaComm\Drv\DrvAnemon.Logic.dll

#nullable disable
namespace Scada.Comm.Drivers.DrvAnemon.Logic;

internal static class TagIndex
{
  public const int PacketReceived = 0;
  public const int PacketFailed = 1;
  public const int DateTime = 2;
  public const int Sensors = 3;

  public static int Sensor(int sensorNum) => 3 + sensorNum - 1;
}
