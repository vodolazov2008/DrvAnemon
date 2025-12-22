// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon.Logic.DevAnemonLogic
// Assembly: DrvAnemon.Logic, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EDD75D42-CE83-4E55-B8C5-83132A68E0A4
// Assembly location: D:\RapidScada\DrvAnemon\SCADA\ScadaComm\Drv\DrvAnemon.Logic.dll

using Scada.Comm.Channels;
using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable disable
namespace Scada.Comm.Drivers.DrvAnemon.Logic;

internal class DevAnemonLogic : DeviceLogic
{
  private const int InBufLenght = 10000;
  private const int DefDataLifetime = 600;
  private const int DefProtocolVersion = 1;
  private static readonly BinStopCondition StopCond = new BinStopCondition((byte) 10);
  private readonly byte[] inBuf;
  private bool alive;
  private TimeSpan dataLifetime;
  private int protocolVersion;

  public DevAnemonLogic(
    ICommContext commContext,
    ILineContext lineContext,
    DeviceConfig deviceConfig)
    : base(commContext, lineContext, deviceConfig)
  {
    this.ConnectionRequired = false;
    this.inBuf = new byte[10000];
    this.alive = false;
    this.dataLifetime = TimeSpan.FromSeconds(600.0);
    this.protocolVersion = 1;
  }

  private bool ProcData(
    byte[] buffer,
    int offset,
    int count,
    Connection conn,
    IncomingRequestArgs requestArgs)
  {
    Protocol.DataPacket dataPacket;
    string errMsg;
    int num = Protocol.DecodeDataPacket(buffer, offset, count, out dataPacket, out errMsg) ? 1 : 0;
    DeviceLogic firstDevice = requestArgs.GetFirstDevice();
    if (firstDevice == null && dataPacket != null && !string.IsNullOrEmpty(dataPacket.DeviceID))
      this.LineContext.GetDeviceByAddress(dataPacket.DeviceID, ref firstDevice);
    if (num != 0)
    {
      if (firstDevice == null)
        this.Log.WriteLine("Ошибка: устройство с позывным {0} не найдено", new object[1]
        {
          (object) dataPacket.DeviceID
        });
      else if (firstDevice.StrAddress != dataPacket.DeviceID)
      {
        this.Log.WriteLine("Ошибка: пакет данных не соответствует устройству с позывным {0}", new object[1]
        {
          (object) firstDevice.StrAddress
        });
      }
      else
      {
        DevAnemonLogic.RetrieveData(firstDevice, dataPacket);
        this.SendAcknowledge(conn, "@OK\r\n");
        requestArgs.TargetDevices.Add(firstDevice);
        requestArgs.HasError = false;
        return true;
      }
    }
    else
      this.Log.WriteLine(errMsg, Array.Empty<object>());
    this.SendAcknowledge(conn, "@FAIL\r\n");
    requestArgs.HasError = true;
    return false;
  }

  private void SendAcknowledge(Connection conn, string ack)
  {
    if (this.protocolVersion >= 2)
      ack = $"@{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}@{ack}";
    byte[] bytes = Encoding.ASCII.GetBytes(ack);
    string str;
    conn.Write(bytes, 0, bytes.Length, (ProtocolFormat) 1, ref str);
    this.Log.WriteLine(str, Array.Empty<object>());
  }

  private static void RetrieveData(DeviceLogic deviceLogic, Protocol.DataPacket dataPacket)
  {
    if (!(deviceLogic is DevAnemonLogic devAnemonLogic))
      return;
    if (dataPacket.IsCurrent)
    {
      devAnemonLogic.DeviceData.SetDateTime(2, dataPacket.DateTime, 1);
      foreach (Protocol.SensorValue sensorValue in dataPacket.SensorValues)
        devAnemonLogic.DeviceData[TagIndex.Sensor(sensorValue.SensorNum)] = sensorValue.GetCnlData();
    }
    else
    {
      int count = dataPacket.SensorValues.Count;
      DeviceSlice deviceSlice = new DeviceSlice(dataPacket.DateTime, count, count);
      for (int index = 0; index < count; ++index)
      {
        Protocol.SensorValue sensorValue = dataPacket.SensorValues[index];
        deviceSlice.DeviceTags[index] = devAnemonLogic.DeviceTags[TagIndex.Sensor(sensorValue.SensorNum)];
        deviceSlice.CnlData[index] = sensorValue.GetCnlData();
      }
      deviceSlice.Descr = string.Join(", ", ((IEnumerable<DeviceTag>) deviceSlice.DeviceTags).Select<DeviceTag, string>((Func<DeviceTag, string>) (tag => tag.Name)));
      devAnemonLogic.DeviceData.EnqueueSlice(deviceSlice);
    }
  }

  private static void FinishRequest(DeviceLogic deviceLogic, bool procDataOK)
  {
    if (!(deviceLogic is DevAnemonLogic devAnemonLogic))
      return;
    devAnemonLogic.DeviceData.Add(procDataOK ? 0 : 1, 1.0);
    devAnemonLogic.LastRequestOK = procDataOK;
    devAnemonLogic.LastSessionTime = DateTime.UtcNow;
    if (procDataOK)
      return;
    devAnemonLogic.DeviceData.Invalidate(2, 257);
  }

  public virtual void OnCommLineStart()
  {
    OptionList customOptions = this.LineContext.LineConfig.CustomOptions;
    this.dataLifetime = TimeSpan.FromSeconds((double) ScadaUtils.GetValueAsInt((IDictionary<string, string>) customOptions, "DataLifetime", 600));
    this.protocolVersion = ScadaUtils.GetValueAsInt((IDictionary<string, string>) customOptions, "ProtocolVersion", 1);
  }

  public virtual bool CheckBehaviorSupport(ChannelBehavior behavior) => behavior == 1;

  public virtual void InitDeviceTags()
  {
    foreach (CnlPrototypeGroup cnlPrototypeGroup in CnlPrototypeFactory.GetCnlPrototypeGroups())
      this.DeviceTags.AddGroup(cnlPrototypeGroup.ToTagGroup());
  }

  public virtual void InitDeviceData()
  {
    base.InitDeviceData();
    this.DeviceData.Set(0, 0.0);
    this.DeviceData.Set(1, 0.0);
  }

  public virtual void Session()
  {
    int num1 = (int) this.DeviceData.Get(0);
    int num2 = (int) this.DeviceData.Get(1);
    this.DeviceStats.SessionCount = this.DeviceStats.RequestCount = num1 + num2;
    this.DeviceStats.SessionErrors = this.DeviceStats.RequestErrors = num2;
    if (DateTime.UtcNow - this.LastSessionTime <= this.dataLifetime)
    {
      this.alive = true;
      this.DeviceStatus = this.LastRequestOK ? (DeviceStatus) 1 : (DeviceStatus) 2;
    }
    else
    {
      if (this.alive)
      {
        this.alive = false;
        this.Log.WriteLine("Установка недостоверности текущих данных {0}", new object[1]
        {
          (object) this.Title
        });
        this.DeviceData.Invalidate(2, 257);
      }
      this.DeviceStatus = this.LastSessionTime > DateTime.MinValue ? (DeviceStatus) 2 : (DeviceStatus) 0;
    }
    this.DeviceData.SetStatusTag(this.DeviceStatus);
  }

  public virtual void ReceiveIncomingRequest(Connection conn, IncomingRequestArgs requestArgs)
  {
    base.ReceiveIncomingRequest(conn, requestArgs);
    bool flag;
    string str;
    int count = conn.Read(this.inBuf, 0, 10000, this.PollingOptions.Timeout, DevAnemonLogic.StopCond, ref flag, (ProtocolFormat) 1, ref str);
    this.Log.WriteLine(str, Array.Empty<object>());
    if (!flag)
      this.Log.WriteLine("Ошибка: пакет данных не обнаружен", Array.Empty<object>());
    bool procDataOK = flag && this.ProcData(this.inBuf, 0, count, conn, requestArgs);
    DevAnemonLogic.FinishRequest(requestArgs.GetFirstDevice(), procDataOK);
  }

  public virtual void ProcessIncomingRequest(
    byte[] buffer,
    int offset,
    int count,
    IncomingRequestArgs requestArgs)
  {
    base.ProcessIncomingRequest(buffer, offset, count, requestArgs);
    bool procDataOK = this.ProcData(buffer, offset, count, this.Connection, requestArgs);
    DevAnemonLogic.FinishRequest(requestArgs.GetFirstDevice(), procDataOK);
  }
}
