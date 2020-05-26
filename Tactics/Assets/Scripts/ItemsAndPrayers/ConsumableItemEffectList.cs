using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static BattleManager;

[CreateAssetMenu(menuName = "ConsumableItem/EffectList")]
public class ConsumableItemEffectList : ScriptableObject {
    Unit target { get { return targetUnit.GetComponent<Unit>(); } }

    public void venison() {
        target.HP += 20;
    }

    public void smellingSalts() {
        if (target.statusList.Contains(UnitStatus.Fallen)) {
            target.statusList.Remove(UnitStatus.Fallen);
            target.HP += 12;
        }
    }

    public void strangeMushroom() {
        target.MP += 20;
    }
}