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
    public int cost;
    public bool restrictCursor;
    public UnityEvent effect;
    public int reach;
    public int bandThickness;
    public TargetStyle targetStyle;
    public int effectReach;
    public int effectBandThickness;
    public TargetStyle effectTargetStyle;
}

