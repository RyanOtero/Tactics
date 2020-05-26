using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "ConsumableItem/Data")]
public class ConsumableItemData : ItemData {
    public UnityEvent effect;
}