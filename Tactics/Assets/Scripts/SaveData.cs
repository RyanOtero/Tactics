using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData {

    public int Checkpoint { get; set; }
    public List<UnitData> UnitDataList { get; private set; }
    public int Slot { get; private set; }
    public float GameTime { get; private set; }
    public int GoldTotal { get; set; }
    public DictStringInt Inventory { get; set; }

    public SaveData() {
        UnitDataList = new List<UnitData>();
        List<string> filesInSaveDir = new List<string>(Directory.GetFiles(Application.persistentDataPath));
        Slot = filesInSaveDir.FindAll(s => s.Contains("save")).Count;
        Inventory = new DictStringInt();
    }

    public void RecordUnitData(List<Unit> units) {
        units.ForEach(u => {
            u.GetComponent<Unit>().SaveUnitData();
            UnitDataList.Add(u.GetComponent<Unit>().unitData);
        });
    }

    public void Save() {
        GameTime += Time.time;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/save" + Slot.ToString("D2") + ".dat");
        try {
            bf.Serialize(file, this);
        } catch (Exception e) {
            Debug.Log(e.Message +"\n Unable to save game!");
        } finally {
            file.Close();
        }
    }

    public static List<SaveData> GetSaves() {
        BinaryFormatter bf = new BinaryFormatter();
        List<string> filesInSaveDir = new List<string>(Directory.GetFiles(Application.persistentDataPath));
        List<string> stringsInSaveDir = filesInSaveDir.FindAll(s => s.Contains("save"));
        List<SaveData> savesList = new List<SaveData>();
        stringsInSaveDir.ForEach(s => {
            FileStream file = File.OpenRead(s);
            try {
                savesList.Add((SaveData)bf.Deserialize(file));
            } catch (Exception e) {
                Debug.Log(e.Message + "\n Unable to add save!");
            } finally {
                file.Close();
            }
        });
        return savesList;
    }

    public static SaveData Load(int slot) {
        SaveData saveData = null;
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(Application.persistentDataPath + "/save" + slot.ToString("D2") + ".dat")) {
            FileStream file = File.OpenRead(Application.persistentDataPath + "/save" + slot.ToString("D2") + ".dat");
            try {
                saveData = (SaveData)bf.Deserialize(file);
            } catch (Exception e) {
                Debug.Log(e.Message + "\n Unable to load game!");
            } finally {
                file.Close();
            }
        } else {
            Debug.Log("File does not exist!");
        }
        return saveData;
    }


}
