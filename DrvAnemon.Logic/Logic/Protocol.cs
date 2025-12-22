// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon.Logic.Protocol
// Assembly: DrvAnemon.Logic, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EDD75D42-CE83-4E55-B8C5-83132A68E0A4
// Assembly location: D:\RapidScada\DrvAnemon\SCADA\ScadaComm\Drv\DrvAnemon.Logic.dll

using Scada.Data.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

#nullable disable
namespace Scada.Comm.Drivers.DrvAnemon.Logic;

internal static class Protocol
{
  private const char PacketSepChar = '#';
  private const char FieldNameSepChar = ':';
  private const double UndefVal = -100.0;
  private const int MaxSensorCnt = 256 /*0x0100*/;
  public const int PacketEndCode = 10;
  public const string Acknowledge = "@OK\r\n";
  public const string NegativeAcknowledge = "@FAIL\r\n";
  private static readonly string PacketSepStr = '#'.ToString();
  private static readonly char[] FieldSep = new char[1]
  {
    ';'
  };

  public static bool DecodeDataPacket(
    byte[] buffer,
    int offset,
    int count,
    out Protocol.DataPacket dataPacket,
    out string errMsg)
  {
    dataPacket = (Protocol.DataPacket) null;
    try
    {
      string str1 = Encoding.ASCII.GetString(buffer, offset, count).Trim();
      string[] strArray = str1.StartsWith(Protocol.PacketSepStr, StringComparison.Ordinal) && str1.EndsWith(Protocol.PacketSepStr, StringComparison.Ordinal) ? str1.Trim('#').Split(Protocol.FieldSep, StringSplitOptions.RemoveEmptyEntries) : throw new FormatException("Некорректное начало или окончание пакета.");
      dataPacket = new Protocol.DataPacket();
      foreach (string str2 in strArray)
      {
        int length = str2.IndexOf(':');
        if (length > 0)
        {
          string str3 = str2.Substring(0, length);
          string s = str2.Substring(length + 1);
          switch (str3)
          {
            case "id":
              dataPacket.DeviceID = s;
              continue;
            case "datetime":
              DateTime result1;
              if (!DateTime.TryParseExact(s, "yyyyMMddHHmmss", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result1))
                throw new FormatException("Некорректный формат даты и времени.");
              dataPacket.DateTime = DateTime.SpecifyKind(result1, DateTimeKind.Utc);
              continue;
            case "live":
              dataPacket.IsCurrent = s == "1";
              continue;
            default:
              if (str3.StartsWith("t", StringComparison.Ordinal))
              {
                int result2;
                if (!int.TryParse(str3.Substring(1), out result2))
                  throw new FormatException("Некорректный номера датчика.");
                if (result2 < 1 || result2 > 256 /*0x0100*/)
                  throw new Exception("Недопустимый номер датчика.");
                double result3;
                if (!double.TryParse(s, NumberStyles.Number, (IFormatProvider) CultureInfo.InvariantCulture, out result3))
                  throw new FormatException("Некорректное измеренное значение.");
                dataPacket.SensorValues.Add(new Protocol.SensorValue()
                {
                  SensorNum = result2,
                  Value = result3
                });
                continue;
              }
              continue;
          }
        }
      }
      errMsg = "";
      return true;
    }
    catch (Exception ex)
    {
      errMsg = "Ошибка при расшифровке пакета данных: " + ex.Message;
      return false;
    }
  }

  public struct SensorValue
  {
    public int SensorNum { get; set; }

    public double Value { get; set; }

    public CnlData GetCnlData()
    {
      return this.Value != -100.0 ? new CnlData(this.Value, 1) : CnlData.Empty;
    }
  }

  public class DataPacket
  {
    public DataPacket()
    {
      this.DeviceID = "";
      this.DateTime = DateTime.MinValue;
      this.IsCurrent = true;
      this.SensorValues = new List<Protocol.SensorValue>();
    }

    public string DeviceID { get; set; }

    public DateTime DateTime { get; set; }

    public bool IsCurrent { get; set; }

    public List<Protocol.SensorValue> SensorValues { get; private set; }
  }
}
