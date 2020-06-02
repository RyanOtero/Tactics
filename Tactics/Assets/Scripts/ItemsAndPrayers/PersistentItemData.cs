using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "PersistentItem/Data")]
public class PersistentItemData : ItemData {
    public UnityEvent equipEffect = new UnityEvent();
    public UnityEvent unequipEffect = new UnityEvent();
}