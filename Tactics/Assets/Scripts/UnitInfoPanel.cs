using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UnitInfoPanel : MonoBehaviour
{
    public Unit unit;
    public bool wasNull;
    void Start()
    {
        wasNull = false;
    }

    void Update()
    {
        if (!wasNull && unit != null) wasNull = true;
        if (wasNull && unit != null) {
            transform.Find("UnitName").GetComponent<TextMeshProUGUI>().text = unit.unitName;
            transform.Find("HPLabel").GetComponent<TextMeshProUGUI>().text = "HP: " + unit.HP + "/" + unit.maxHp;
            transform.Find("FaithLabel").GetComponent<TextMeshProUGUI>().text = "Faith: " + unit.Faith + "/" + unit.maxFaith;
            transform.Find("LvlLabel").GetComponent<TextMeshProUGUI>().text = "Lvl: " + unit.level;
            transform.Find("ExpLabel").GetComponent<TextMeshProUGUI>().text = "Exp: " + unit.experience;

            string sList = "";
            foreach (UnitStatus status in unit.statusList) {
                sList += status + ", ";
            }
            if (sList != "") sList = sList.Substring(0, sList.Length - 3);
            transform.Find("Status").GetComponent<TextMeshProUGUI>().text = sList;
            wasNull = false;
        }
    }

    public void Clear() {
        unit = null;
    }
}
