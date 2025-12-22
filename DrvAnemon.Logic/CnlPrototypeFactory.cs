// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon.CnlPrototypeFactory
// Assembly: DrvAnemon.Logic, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EDD75D42-CE83-4E55-B8C5-83132A68E0A4
// Assembly location: D:\RapidScada\DrvAnemon\SCADA\ScadaComm\Drv\DrvAnemon.Logic.dll

using Scada.Comm.Devices;
using System.Collections.Generic;

#nullable disable
namespace Scada.Comm.Drivers.DrvAnemon;

internal static class CnlPrototypeFactory
{
  public static List<CnlPrototypeGroup> GetCnlPrototypeGroups()
  {
    List<CnlPrototypeGroup> cnlPrototypeGroups = new List<CnlPrototypeGroup>();
    CnlPrototypeGroup cnlPrototypeGroup1 = new CnlPrototypeGroup("Связь");
    cnlPrototypeGroup1.AddCnlPrototype("PR", "Получено пакетов").SetFormat("N0");
    cnlPrototypeGroup1.AddCnlPrototype("PF", "Потеряно пакетов").SetFormat("N0");
    cnlPrototypeGroups.Add(cnlPrototypeGroup1);
    CnlPrototypeGroup cnlPrototypeGroup2 = new CnlPrototypeGroup("Контроллер");
    cnlPrototypeGroup2.AddCnlPrototype("DT", "Дата и время").SetFormat("DateTime");
    cnlPrototypeGroups.Add(cnlPrototypeGroup2);
    CnlPrototypeGroup cnlPrototypeGroup3 = new CnlPrototypeGroup("Текущие данные");
    for (int index = 1; index <= 256 /*0x0100*/; ++index)
    {
      string str = "t" + index.ToString();
      cnlPrototypeGroup3.AddCnlPrototype(str, str).SetFormat("N1");
    }
    cnlPrototypeGroups.Add(cnlPrototypeGroup3);
    return cnlPrototypeGroups;
  }
}
