using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using static BattleManager;

[CreateAssetMenu(menuName = "Prayer/EffectList")]
public class PrayerEffectList : ScriptableObject {
    Unit unit { get { return SelectedUnit; } }
    List<Unit> targets { get { return targetUnits; } }

    public void Chill() {
        RotateUnit(unit, cursor);
        targets.ForEach(x => x.HP -= 6);
    }

    public void Burn() {
        RotateUnit(unit, cursor);
        targets.ForEach(x => x.HP -= 6);
    }


    public void Heal() {
        RotateUnit(unit, cursor);
        targets.ForEach(x => {
            if (x.uClass == UnitClass.Undead) {
                x.HP -= 30;
            } else {
                x.HP += 30;
            }
        });
    }
    
    public void Raise() {
        RotateUnit(unit, cursor);
        targets.ForEach(x => {

            if (x.uClass == UnitClass.Undead) {
                x.HP -= 999;
            } else {
                if (x.statusList.Contains(UnitStatus.Fallen)) {
                    x.statusList.Remove(UnitStatus.Fallen);
                    x.HP += 12;
                }
            }
        });
    }

    public void Curse() {
        RotateUnit(unit, cursor);
    }

    public void Bless() {
        RotateUnit(unit, cursor);
    }
}
