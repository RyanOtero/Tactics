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
            transform.Find("UnitName").GetComponent<TextMeshProUGUI>().text = unit.GetComponent<Unit>().unitName;
            transform.Find("HPLabel").GetComponent<TextMeshProUGUI>().text = "HP: " + unit.GetComponent<Unit>().HP + "/" + unit.GetComponent<Unit>().maxHp;
            transform.Find("FaithLabel").GetComponent<TextMeshProUGUI>().text = "Faith: " + unit.GetComponent<Unit>().Faith + "/" + unit.GetComponent<Unit>().maxFaith;
            transform.Find("LvlLabel").GetComponent<TextMeshProUGUI>().text = "Lvl: " + unit.GetComponent<Unit>().level;
            transform.Find("ExpLabel").GetComponent<TextMeshProUGUI>().text = "Exp: " + unit.GetComponent<Unit>().experience;

            string sList = "";
            foreach (UnitStatus status in unit.GetComponent<Unit>().statusList) {
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
