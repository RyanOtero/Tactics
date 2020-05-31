using System;
using System.Collections;
using System.Collections.Generic;

public enum UnitType { Main, Ally, SpecialAlly, Enemy, Boss}
public enum UnitClass { Knight, Archer, Bard, Mage, Ranger, Undead }
public enum UnitStatus { Fallen, Poison, Haste, Slow, Curse, Bless, Fear, Brave}
public enum TargetStyle { Band, Cross, Diamond }

[Serializable]
public class UnitData {

    public UnitType UType { get; set; }
    public UnitClass UClass { get; set; }
    public List<UnitStatus> StatusList { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public string UnitName { get; set; }
    public int MaxHP { get; set; }
    public int MaxMP { get; set; }
    public int Movement { get; set; }
    public int Jump { get; set; }
    public int Speed { get; set; }
    public float[,] PosRot { get; set; }
    public bool IsPlayer { get; set; }
    public TargetStyle TStyle { get; set; }
    public int TargetRange { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public List<string> Equipment { get; set; }
    public List<string> Prayers { get; set; }
    public UnitData() {
        UType = UnitType.Ally;
        UClass = UnitClass.Knight;
        TStyle = TargetStyle.Cross;
        StatusList = new List<UnitStatus>();
    }

}