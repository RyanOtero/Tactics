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
    List<Unit> targets { get { return targetUnits; } }

    public void venison() {
        
        targets.ForEach(x => x.HP += 20);
    }

    public void smellingSalts() {
        targets.ForEach(x => {
            if (x.statusList.Contains(UnitStatus.Fallen)) {
                x.statusList.Remove(UnitStatus.Fallen);
                x.HP += 12;
            }
        });
    }

    public void strangeMushroom() {
        targets.ForEach(x => x.Faith += 20);
    }
}