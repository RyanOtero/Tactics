using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public static CameraController Instance { get; private set; } = null;
    public static GameObject cam;
    public static Transform camTrans;
    public static Transform ePivotTrans;
    public static Transform cameraParentTrans;
    public static Quaternion startRotation;
    public static Vector3 startZoom;
    public static Transform cursorTrans;
    public static Vector3 camParentOffset;
    public static float smoothTime = 0.4f;
    public static Vector3 velocity;
    public static float lowerThreshold;
    public static float upperThreshold;
    public static bool isResettingCamParentOffset;
    public static float timer;
    public static Vector3 startPoint;


    void Awake() {
        //Singleton Check
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        ePivotTrans = GameObject.FindGameObjectWithTag("EPivot").transform;
        cameraParentTrans = GameObject.FindGameObjectWithTag("CameraParent").transform;
        startRotation = Quaternion.identity;
        camTrans = cam.transform;
        cursorTrans = GameObject.FindGameObjectWithTag("Cursor").transform;
        cameraParentTrans.position = cursorTrans.position + new Vector3(0f, -1.25f, 0f);
        SetThresholds(.35f, .65f);
    }

    void LateUpdate() {
        Vector3 cursorPos = Camera.main.WorldToViewportPoint(cursorTrans.position + new Vector3(0f, -1f, 0f));
        if (cursorPos.x >= lowerThreshold && cursorPos.x <= upperThreshold && cursorPos.y >= lowerThreshold && cursorPos.y <= upperThreshold) {
            SetCamParentOffset();
        }
        if (cursorPos.x < lowerThreshold || cursorPos.x > upperThreshold || cursorPos.y < lowerThreshold || cursorPos.y > upperThreshold) {
            cameraParentTrans.position = Vector3.SmoothDamp(cameraParentTrans.position, cursorTrans.position + camParentOffset, ref velocity, smoothTime);
        }
        if (isResettingCamParentOffset) {
            if (timer == 0f) {
                startPoint = cameraParentTrans.position;
                timer += Time.deltaTime;
            }
            if (timer > 0f && timer < 1f / 3f) {
                cameraParentTrans.position = Vector3.Lerp(startPoint, cursorTrans.position + new Vector3(0f, -1f, 0f), timer * 3f);
                timer += Time.deltaTime;
            }
            if (timer >= 1f / 3f) {
                timer = 0f;
                cameraParentTrans.position = cursorTrans.position + new Vector3(0f, -1f, 0f);
                isResettingCamParentOffset = !isResettingCamParentOffset;
            }
        }
    }

    public static void SetCamParentOffset() {
        camParentOffset = cameraParentTrans.position - cursorTrans.position;
    }

    public static void SetThresholds(float lower, float upper) {
        lowerThreshold = lower;
        upperThreshold = upper;
    }
}
