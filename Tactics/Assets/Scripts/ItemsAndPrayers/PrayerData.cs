using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using static BattleManager;

[CreateAssetMenu(menuName = "Prayer/Data")]
public class PrayerData : ScriptableObject {
    public Sprite sprite;
    public string prayerName;
    public string description;
    public UnityEvent effect;
}

