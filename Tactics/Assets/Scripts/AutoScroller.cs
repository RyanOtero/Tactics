using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScroller : MonoBehaviour {

    public int divisionPixelCount;
    public GameObject loadMenuCanvas;
    private bool isHeld;
    private Vector3 startPos;

    void Start() {
        startPos = transform.localPosition;
    }

    void Update() {
        int buttonIndex = CanvasManager.Instance.buttonIndex;
        int maxButtonIndex = CanvasManager.Instance.maxButtonIndex;
        //       Debug.Log(buttonIndex + " " + maxButtonIndex);

        if (InputManager.GoingSouth(InputManager.deadZone) && buttonIndex > 2 && !isHeld) {
            isHeld = true;
            transform.localPosition += Vector3.up * divisionPixelCount;
        }
        if (InputManager.GoingNorth(InputManager.deadZone) && buttonIndex < maxButtonIndex - 3 && !isHeld) {
            isHeld = true;
            transform.localPosition -= Vector3.up * divisionPixelCount;
        }
        if (InputManager.GoingSouth(InputManager.deadZone) && buttonIndex == 0 && !isHeld) {
            isHeld = true;
            transform.localPosition = startPos;
        }
        if (InputManager.GoingNorth(InputManager.deadZone) && buttonIndex == maxButtonIndex - 1 && !isHeld) {
            isHeld = true;
            transform.localPosition = startPos + Vector3.up * (divisionPixelCount * (maxButtonIndex - 1));
        }
        if (InputManager.DirectionsReleased(InputManager.deadZone)) {
            isHeld = false;
        }
    }
}
