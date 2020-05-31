using System;
using System.Collections.Generic;
using UnityEngine;

public static class TacticsUtil {
    public static string[] Checkpoints = new string[] {"0", "In the Beginning...", "So far so good", "And then there were 3", "four", "five", "6" };    


    public static string FormatTime(float time) {
        int hours = (int)time / 3600;
        int minutes = ((int)time - 3600 * hours) / 60;
        int seconds = (int)time - 3600 * hours - 60 * minutes;
        return string.Format("{0:000}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    //calculate total of x and z offsets from origin to destination
    public static int CalcCost(Tile origin, Tile destination) {
        return (Mathf.Abs(origin.x - destination.x) + Mathf.Abs(origin.y - destination.y));
    }

    public static int CalcCost(Vector3 origin, Vector3 destination) {
        return (int)(Mathf.Abs(origin.x - destination.x) + Mathf.Abs(origin.z - destination.z));
    }

    //calculate x offset from origin to destination
    public static int CalcXOffset(Tile origin, Tile destination) {
        return Mathf.Abs(destination.x - origin.x);
    }

    //calculate z offset from origin to destination
    public static int CalcYOffset(Tile origin, Tile destination) {
        return Mathf.Abs(destination.y - origin.y);
    }

    //shortcut functions for position vectors
    public static float GetX(GameObject go) {
        float x = go.transform.position.x;
        return x;
    }

    public static float GetX(MonoBehaviour mb) {
        float x = mb.gameObject.transform.position.x;
        return x;
    }

    public static float GetY(GameObject go) {
        float y = go.transform.position.y;
        return y;
    }

    public static float GetY(MonoBehaviour mb) {
        float y = mb.gameObject.transform.position.y;
        return y;
    }

    public static float GetZ(GameObject go) {
        float z = go.transform.position.z;
        return z;
    }
    public static float GetZ(MonoBehaviour mb) {
        float z = mb.gameObject.transform.position.z;
        return z;
    }
}

