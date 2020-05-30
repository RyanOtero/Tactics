using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDisplay : MonoBehaviour {
    public ItemData data;
    public Sprite sprite;

    void Start() {
        Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
        Material mat = new Material(Shader.Find("UI/Unlit/Detail"));
        mat.SetFloat("_Mode", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        mat.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
        mat.renderQueue = 3103;
        mat.SetTexture("_MainTex", sprite.texture);
        renderer.material = mat;
    }
}




