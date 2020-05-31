using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using static BattleManager;

[CreateAssetMenu(menuName = "Prayer/EffectList")]
public class PrayerEffectList : ScriptableObject {
    Unit caster { get { return selectedUnit.GetComponent<Unit>(); } }
    Unit target { get { return targetUnit.GetComponent<Unit>(); } }

    public void Chill() {
        target.HP -= 6;
    }

    public void Burn() {
        target.HP -= 6;
    }


    public void Heal() {
        if (target.uClass == UnitClass.Undead) {
            target.HP -= 30;
        } else {
            target.HP += 30;
        }
    }
    
    public void Raise() {
        if (target.uClass == UnitClass.Undead) {
            target.HP -= 999;
        } else {
            if (target.statusList.Contains(UnitStatus.Fallen)) {
                target.statusList.Remove(UnitStatus.Fallen);
                target.HP += 12;
            }
        }
    }
}
