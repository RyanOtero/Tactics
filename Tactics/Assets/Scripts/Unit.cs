using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour {

    public UnitType uType;
    public UnitClass uClass;
    public List<UnitStatus> statusList;
    public int level;
    public int experience;
    public string unitName;
    public int maxHp;
    public int maxFaith;
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
    public int Faith {
        get {
            return faith;
        }
        set {
            if (value > maxFaith) {
                faith = maxFaith;
            } else if (value < 0) {
                faith = 0;
            } else {
                faith = value;
            }
        }
    }
    [SerializeField]
    private int faith;
    public int movement;
    public int jump;
    public int speed;
    [SerializeField]
    private int speedCounter;
    public int destroyCounter;
    public int attack;
    public int defense;
    public bool hasMoved;
    public bool hasActed;
    public bool activeUnit;
    public UnitData unitData;
    public bool isPlayer;
    public TargetStyle targetStyle;
    public int bandThickness;
    public int reach;
    [SerializeField]
    public List<string> equipment;
    [SerializeField]
    public List<string> prayers;
    public int itemReach;


    public int SpeedCounter {
        get {
            return speedCounter;
        }
        set {
            speedCounter = value;
            if (speedCounter >= 100) {
                speedCounter -= 100;
                ActiveUnit = true;
            }
        }
    }
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
        faith = maxFaith;
        ActiveUnit = true;
        if (speed == 0) {
            speed = 1;
        }
        SpeedCounter += speed;
    }

    public void IncrementSpeed() {
        SpeedCounter += speed;
    }

    public void DeadCheck() {
        if (HP == 0) destroyCounter++;
        if (destroyCounter > 2) {
            //Instantiate(Resources.Load<GameObject>("Prefab/Reward"), transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
    public void SaveUnitData() {
        unitData.UType = uType;
        unitData.UClass = uClass;
        unitData.StatusList = statusList;
        unitData.Level = level;
        unitData.Experience = experience;
        unitData.UnitName = unitName;
        unitData.MaxHP = maxHp;
        unitData.MaxFaith = maxFaith;
        unitData.Movement = movement;
        unitData.Jump = jump;
        unitData.Speed = speed;
        unitData.Attack = attack;
        unitData.Defense = defense;
        unitData.IsPlayer = isPlayer;
        unitData.TStyle = targetStyle;
        unitData.BandThickness = bandThickness;
        unitData.Reach = reach;
        foreach (string itemName in equipment) {
            if (itemName != null) {
                unitData.Equipment.Add(itemName);
            }
        }
        foreach (string prayerName in prayers) {
            if (prayerName != null) {
                unitData.Prayers.Add(prayerName);
            }
        }
        unitData.PosRot = new float[,] { { transform.position.x, transform.position.y, transform.position.z },
            {transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z} };
    }

    public void LoadUnitData(UnitData unitData) {
        uType = unitData.UType;
        uClass = unitData.UClass;
        statusList = unitData.StatusList;
        level = unitData.Level;
        experience = unitData.Experience;
        unitName = unitData.UnitName;
        maxHp = unitData.MaxHP;
        maxFaith = unitData.MaxFaith;
        movement = unitData.Movement;
        jump = unitData.Jump;
        speed = unitData.Speed;
        attack = unitData.Attack;
        defense = unitData.Defense;
        isPlayer = unitData.IsPlayer;
        targetStyle = unitData.TStyle;
        bandThickness = unitData.BandThickness;
        reach = unitData.Reach;
        foreach (string itemName in unitData.Equipment) {
            if (itemName != null) {
                equipment.Add(itemName);
            }
        }
        foreach (string prayerName in unitData.Prayers) {
            if (prayerName != null) {
                prayers.Add(prayerName);
            }
        }
        transform.position = new Vector3(unitData.PosRot[0, 0], unitData.PosRot[0, 1], unitData.PosRot[0, 2]);
        transform.eulerAngles = new Vector3(unitData.PosRot[1, 0], unitData.PosRot[1, 1], unitData.PosRot[1, 2]);
    }
}
