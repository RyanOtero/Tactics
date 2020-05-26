using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CanvasManager))]
public class dropdowneditor : Editor {

    //public override void OnInspectorGUI() {
    //    CanvasManager script = (CanvasManager)target;

    //    string[] strArr = new string[script.menus.Keys.Count];
    //    script.menus.Keys.CopyTo(strArr,0);
    //    GUIContent list = new GUIContent("active canvas");
    //    script.ActiveCanvasIndex = EditorGUILayout.Popup(list, script.ActiveCanvasIndex, strArr);
    //    base.OnInspectorGUI();
    //}

}
