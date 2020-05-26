using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDisplay : MonoBehaviour {
    public ItemData data;
    public Sprite sprite;

    void Start() {
        Material mat = gameObject.GetComponentInChildren<Renderer>().material;
        mat = Resources.Load<Material>("Materials/Item");
        mat.SetTexture("_MainTex", sprite.texture);
    }
}




