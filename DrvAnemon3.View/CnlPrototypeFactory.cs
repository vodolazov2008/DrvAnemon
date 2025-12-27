// Decompiled with JetBrains decompiler
// Type: Scada.Comm.Drivers.DrvAnemon3.CnlPrototypeFactory
// Assembly: DrvAnemon3.View, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 32160B11-AF08-44A7-A737-2520F961A88B
// Assembly location: D:\RapidScada\DrvAnemon3\SCADA\ScadaAdmin\Lib\DrvAnemon3.View.dll

using Scada.Comm.Devices;
using System.Collections.Generic;

namespace Scada.Comm.Drivers.DrvAnemon3;

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
