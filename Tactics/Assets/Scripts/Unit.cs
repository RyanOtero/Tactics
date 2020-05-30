﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    public UnitClass unitClass;
    public List<UnitStatus> statusList;
    public int level;
    public int experience;

    public int maxHp;
    public int maxMp;
    public int HP {
        get {
            return hp; 
        }
        set {
            if (value > maxHp) {
                hp = maxHp;
            } else if (value < 0) {
                hp = 0;
            } else {
                hp = value;
            }
        }
    }
    [SerializeField]
    private int hp;
    public int MP {
        get {
            return mp;
        }
        set {
            if (value > maxMp) {
                mp = maxMp;
            } else if (value < 0) {
                mp = 0;
            } else {
                mp = value;
            }
        }
    }
    [SerializeField]
    private int mp;
    public int movement;
    public int jump;
    public int attack;
    public int defense;
    public bool hasMoved;
    public bool hasActed;
    public bool activeUnit;
    public UnitData unitData;
    public bool isPlayer;
    public TargetStyle targetStyle;
    public int targetRange;
    [SerializeField]
    public List<PersistentItemData> equipment;
    [SerializeField]
    public List<PrayerData> prayers;

    public bool HasMoved {
        get { return hasMoved; }
        set {
            hasMoved = value;
            if (value && HasActed) {
                ActiveUnit = false;
            }
        }
    }

    public bool HasActed {
        get { return hasActed; }
        set {
            hasActed = value;
            if (HasMoved && value) {
                ActiveUnit = false;
            }
        }
    }
    public bool ActiveUnit {
        get { return activeUnit; }
        set {
            activeUnit = value;
            if (value) {
                HasActed = false;
                HasMoved = false;
            }
        }
    }

    void Start() {
        unitData = new UnitData();
        hp = maxHp;
        mp = maxMp;
        ActiveUnit = true;
    }



    public void SaveUnitData() {
        unitData.UnitClass = unitClass;
        unitData.StatusList = statusList;
        unitData.Level = level;
        unitData.Experience = experience;
        unitData.MaxHP = maxHp;
        unitData.MaxMP = maxMp;
        unitData.Movement = movement;
        unitData.Jump = jump;
        unitData.Attack = attack;
        unitData.Defense = defense;
        unitData.IsPlayer = isPlayer;
        unitData.TStyle = targetStyle;
        unitData.TargetRange = targetRange;
        foreach (PersistentItemData itemData in equipment) {
            if (itemData != null) {
                unitData.Equipment.Add(itemData.itemName);
            }
        }
        foreach (PrayerData prayerData in prayers) {
            if (prayerData != null) {
                unitData.Prayers.Add(prayerData.prayerName);
            }
        }
        unitData.PosRot = new float[,] { { transform.position.x, transform.position.y, transform.position.z },
            {transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z} };
    }

    public void LoadUnitData(UnitData unitData) {
        unitClass = unitData.UnitClass;
        statusList = unitData.StatusList;
        level = unitData.Level;
        experience = unitData.Experience;
        maxHp = unitData.MaxHP;
        maxMp = unitData.MaxMP;
        movement = unitData.Movement;
        jump = unitData.Jump;
        attack = unitData.Attack;
        defense = unitData.Defense;
        isPlayer = unitData.IsPlayer;
        targetStyle = unitData.TStyle;
        targetRange = unitData.TargetRange;
        //equipment = unitData.Equipment;
        //prayers = unitData.Prayers;
        transform.position = new Vector3(unitData.PosRot[0, 0], unitData.PosRot[0, 1], unitData.PosRot[0, 2]);
        transform.eulerAngles = new Vector3(unitData.PosRot[1, 0], unitData.PosRot[1, 1], unitData.PosRot[1, 2]);
    }
}
