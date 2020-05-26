using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ConsumableItemDisplay : MonoBehaviour {

    public ConsumableItemData data;

    void OnEnable() {
    }

    void OnDisable() {
        
    }

    void OnValidate() {
        if (data.sprite != null) gameObject.GetComponent<SpriteRenderer>().sprite = data.sprite;
    }

}
