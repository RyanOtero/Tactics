using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public abstract class ItemData : ScriptableObject {
    public Sprite sprite;
    public string itemName;
    public string description;
    public int value;
    public int sellValue;
}


