using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour {
    public static ItemManager Instance;
    public DictStringInt inventory;
    public int goldTotal;
    public List<ItemData> allItems;

    void Awake() {
        //Singleton Check
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        if (inventory == null) inventory = new DictStringInt();
        allItems = new List<ItemData>(Resources.LoadAll<ItemData>("ScriptableObjects"));
    }

    public static void PickUpItem() {
        if (GameObject.FindGameObjectsWithTag("Item").Length > 0) {
            foreach (GameObject cItem in GameObject.FindGameObjectsWithTag("Item")) {
                if (cItem.transform.position == BattleManager.selectedUnit.transform.position) {
                    if (Instance.inventory.ContainsKey(cItem.GetComponent<ItemDisplay>().data.itemName)) {
                        Instance.inventory[cItem.GetComponent<ItemDisplay>().data.itemName]++;
                    } else {
                        Instance.inventory.Add(cItem.GetComponent<ItemDisplay>().data.itemName, 1);
                    }
                    Destroy(cItem.gameObject);
                    return;
                }
            }
        }
        if (GameObject.FindGameObjectsWithTag("Gold").Length > 0) {
            foreach (GameObject gold in GameObject.FindGameObjectsWithTag("Gold")) {
                if (gold.transform.position == BattleManager.selectedUnit.transform.position) {
                    Instance.goldTotal += gold.GetComponent<Gold>().value;
                    Destroy(gold);
                    return;
                }
            }
        }
    }

    public static void ItemOnWalkOver() {
        if (GameObject.FindGameObjectsWithTag("Item").Length > 0) {
            foreach (GameObject cItem in GameObject.FindGameObjectsWithTag("Item")) {
                if (cItem.transform.position.WithinMargin(1, BattleManager.selectedUnit.transform.position)) {
                    cItem.GetComponentInChildren<Renderer>().enabled = false;
                } else {
                    cItem.GetComponentInChildren<Renderer>().enabled = true;
                }
            }
        }

        if (GameObject.FindGameObjectsWithTag("Gold").Length > 0) {
            foreach (GameObject gold in GameObject.FindGameObjectsWithTag("Gold")) {
                if (gold.transform.position.WithinMargin(1, BattleManager.selectedUnit.transform.position)) {
                    gold.GetComponentInChildren<Renderer>().enabled = false;
                } else {
                    gold.GetComponentInChildren<Renderer>().enabled = true;
                }
            }
        }
    }
}
