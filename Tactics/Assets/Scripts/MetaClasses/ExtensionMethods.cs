using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Extensions {

    #region RectTransform
    public static void SetLeft(this RectTransform rt, float left) {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right) {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top) {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom) {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }
    #endregion

    #region Vector3
    public static bool WithinMargin(this Vector3 v3, int margin, Vector3 other) {
        if (v3.x + margin / 2f > other.x && v3.x - margin / 2f < other.x) {
            if (v3.y + margin / 2f > other.y && v3.y - margin / 2f < other.y) {
                if (v3.z + margin / 2f > other.z && v3.z - margin / 2f < other.z) {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool SameXZ(this Vector3 v3, Vector3 other) {
        if (v3.x == other.x && v3.z == other.z) return true;
        return false;
    }
    #endregion

    #region Int
    public static bool WithinMargin(this int thisInt, int margin, int other) {
        if (thisInt + margin / 2f > other && thisInt - margin / 2f < other) {
            return true;
        }
        return false;
    }
    #endregion

    #region Float
    public static bool WithinMargin(this float thisFloat, int margin, float other) {
        if (thisFloat + margin / 2f > other && thisFloat - margin / 2f < other) {
            return true;
        }
        return false;
    }
    #endregion
}
