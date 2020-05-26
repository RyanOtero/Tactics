using System;
using System.Collections;
using System.Collections.Generic;

public enum UnitClass { Knight, Archer, Bard, Mage, Ranger, Undead }
public enum UnitStatus { Fallen, Poison, Haste, Slow, Curse, Bless, Fear, Brave}
public enum TargetStyle { Band, Cross, Diamond }

[Serializable]
public class UnitData {

    public UnitClass UnitClass { get; set; }
    public List<UnitStatus> StatusList { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public int MaxHP { get; set; }
    public int MaxMP { get; set; }
    public int Movement { get; set; }
    public int Jump { get; set; }
    public float[,] PosRot { get; set; }
    public bool IsPlayer { get; set; }
    public TargetStyle TStyle { get; set; }
    public int TargetRange { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public List<string> Equipment { get; set; }
    public List<string> Prayers { get; set; }
    public UnitData() {
        UnitClass = UnitClass.Knight;
        TStyle = TargetStyle.Cross;
        StatusList = new List<UnitStatus>();
    }

}