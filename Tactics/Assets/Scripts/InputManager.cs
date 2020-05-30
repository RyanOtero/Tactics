using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleManager;
using static TacticsUtil;
using static CameraController;
using System;
using UnityEngine.Events;

public enum InputType { None, Directional, CamClock, CamCounterclock, CamElevation, CamZoom }

public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; } = null;
    public static bool isPastHoldTime;
    public static bool isRepeating;
    private static float timer;
    public static float delayTime;
    public static float repeatTime;
    public static float cameraTimerMax;
    private static Func<float, bool>[] directions;
    private static float[] elevations;
    private static float[] zooms;
    public static Vector3 lastCameraPosition;
    public static bool WasInputLocked { get; private set; }
    public static bool camInUse;
    private static int camZoomCounter;
    private static InputType inputType;
    public static float deadZone;
    public static bool isPaused;
    private static bool isPanelUp;
    public static List<IndexButton> contextButtonList = new List<IndexButton>();

    private int camRotationCounter;
    private int camElevationCounter;
    public GameObject pauseCanvas;

    void Awake() {
        //Singleton Check
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        delayTime = .3f;
        repeatTime = .1f;
        deadZone = .3f;
        isPaused = false;
        isPanelUp = false;
        camInUse = false;
        directions = new Func<float, bool>[] { GoingNorth, GoingEast, GoingSouth, GoingWest };
        elevations = new float[] { 45f, 15f, 25f, 35f };
        zooms = new float[] { -30f, -20f, -40f };
        inputType = InputType.None;
        cameraTimerMax = 1f / 3f;
    }

    void Update() {
        //for (int i = 0; i < 20; i++) {
        //    if (Input.GetKeyDown("joystick button " + i)) {
        //        print("joystick button " + i);
        //    }
        //}

        if (PausingGame()) {
            if (CanvasManager.Instance.activeCanvas == null
                || !CanvasManager.Instance.activeCanvas.name.ToLower().Contains("options")
                && !CanvasManager.Instance.activeCanvas.name.ToLower().Contains("load")
                && !CanvasManager.Instance.activeCanvas.name.ToLower().Contains("controls")) {
                isPaused = !isPaused;
                Time.timeScale = isPaused ? 0 : 1;
                if (isPaused) {
                    if (CanvasManager.IsInputLocked) {
                        CanvasManager.ToggleInputLock();
                        WasInputLocked = true;
                    }
                    if (CanvasManager.Instance.activeCanvas != null) {
                        CanvasManager.Instance.previousCanvas = CanvasManager.Instance.activeCanvas.name.ToLower().Replace("menucanvas(clone)", "");
                    }
                    if (!isPanelUp) TogglePanelUp();
                    CanvasManager.Instance.SetActiveCanvas("pause");
                } else {
                    if (string.IsNullOrEmpty(CanvasManager.Instance.previousCanvas)) {
                        TogglePanelUp();
                        CanvasManager.Instance.SetActiveCanvas("none");
                    }
                    if (WasInputLocked) {
                        CanvasManager.ToggleInputLock();
                        WasInputLocked = false;
                    } else {
                        CanvasManager.Instance.SetActiveCanvas(CanvasManager.Instance.previousCanvas.ToLower().Replace("menucanvas(clone)", ""), CanvasManager.Instance.previousButtonIndex);
                    }
                    CanvasManager.Instance.previousCanvas = "";
                }
            }
        }
        if (isPanelUp) {

            //look for menu input
            CanvasManager.Instance.NavMenu();
            CanvasManager.Instance.NavOptions();

            //Handle first frame after selecting an item in action menu
            if (Phase == PhaseOfTurn.Attack && !isPaused) {
                SetLastLocation();
                availableTargets = GetTargetRange(selectedUnit, true, false);
                PaintRange(availableTargets, "Prefab/Painters/AttackPainter");
                TogglePanelUp();
                CanvasManager.Instance.SetActiveCanvas("none");
            } else if ((Phase == PhaseOfTurn.Prayer || Phase == PhaseOfTurn.Item) && !isPaused) {
                SetLastLocation();
                availableTargets = GetTargetRange(selectedUnit, false, false);
                PaintRange(availableTargets, "Prefab/Painters/ActionPainter");
                TogglePanelUp();
                CanvasManager.Instance.SetActiveCanvas("none");
            } else if (Phase == PhaseOfTurn.Move && !isPaused) {
                SetLastLocation();
                PathNavigationController.startRotation = selectedUnit.transform.eulerAngles;
                availableMoves = GetMoveRange(selectedUnit);
                PaintRange(availableMoves, "Prefab/Painters/MovePainter");
                CanvasManager.Instance.SetActiveCanvas("none");
                TogglePanelUp();
            }

            //Cancel out of menu
            else if (Phase == PhaseOfTurn.SelectAction && Input.GetButtonDown("Cancel") && !Input.GetKey(KeyCode.LeftShift) && !isPaused
                && !selectedUnit.GetComponent<Unit>().HasActed && !selectedUnit.GetComponent<Unit>().HasMoved) {
                CanvasManager.Instance.SetActiveCanvas("none");
                ChangePhase(PhaseOfTurn.SelectUnit);
                selectedUnit = null;
                TogglePanelUp();
            } else if ((Phase == PhaseOfTurn.SelectItem || Phase == PhaseOfTurn.SelectPrayer) && Input.GetButtonDown("Cancel")
                  && !Input.GetKey(KeyCode.LeftShift) && !isPaused) {
                CanvasManager.Instance.SetActiveCanvas("action", CanvasManager.Instance.previousButtonIndex);
                ChangePhase(PhaseOfTurn.SelectAction);
            }


            //if panel is down
        } else {

            //Show/Hide Items
            if (Phase == PhaseOfTurn.Confirm && PreviousPhase == PhaseOfTurn.Move) {// && selectedUnit.GetComponent<PathNavigationController>().isMoving == false) {
                ItemManager.ItemOnWalkOver();
            }

            //allow cursor to move after panel goes down
            if ((Phase == PhaseOfTurn.Attack || Phase == PhaseOfTurn.Prayer || Phase == PhaseOfTurn.Item
                || Phase == PhaseOfTurn.Move) && !CanvasManager.IsInputLocked) {
                CanvasManager.ToggleInputLock();
            }

            //handling submit button when panel is down 
            if (Input.GetButtonDown("Submit")) {
                //different phases
                if (Phase == PhaseOfTurn.SelectUnit) {
                    selectedUnit = SelectUnit();
                    if (selectedUnit != null) {
                        CanvasManager.Instance.SetActiveCanvas("action");
                        ChangePhase(PhaseOfTurn.SelectAction);
                        TogglePanelUp();
                    }
                    //move goes to confirm phase
                } else if (Phase == PhaseOfTurn.Move) {
                    if (availableMoves.Exists(x => (x.x == GetX(cursor)) && (x.y == GetZ(cursor)))) {
                        selectedTile = availableMoves.Find(x => (x.x == GetX(cursor)) && (x.y == GetZ(cursor)));
                        selectedUnit.GetComponent<PathNavigationController>().isMoving = true;
                        ChangePhase(PhaseOfTurn.Confirm);
                    }
                    //actions go to confirm phase
                } else if (Phase == PhaseOfTurn.Attack) {
                    PreActionCheck();
                } else if (Phase == PhaseOfTurn.Prayer) {
                    PreActionCheck();
                } else if (Phase == PhaseOfTurn.Item) {
                    PreActionCheck();
                    //perform actions if confirmed
                } else if (Phase == PhaseOfTurn.Confirm) {
                    if (PreviousPhase == PhaseOfTurn.Attack) {
                        UnityEvent attackEvent = new UnityEvent();
                        attackEvent.AddListener(Attack);
                        PerformAction(attackEvent);
                        CanvasManager.ToggleInputLock();
                    } else if (PreviousPhase == PhaseOfTurn.Prayer) {
                        PerformAction(UsePrayer);
                        CanvasManager.ToggleInputLock();
                    } else if (PreviousPhase == PhaseOfTurn.Item) {
                        PerformAction(UseItem);
                        CanvasManager.ToggleInputLock();
                        //confirmed move phase
                    } else if (selectedUnit.GetComponent<PathNavigationController>().isMoving == false) {
                        CanvasManager.ToggleInputLock();
                        ItemManager.PickUpItem();
                        selectedUnit.GetComponent<Unit>().HasMoved = true;
                        PostPhaseEval();
                    }
                }
            }
            //handle cancel when panel is down
            if (Input.GetButtonDown("Cancel") && !Input.GetKey(KeyCode.LeftShift) && selectedUnit != null) {

                //go back to move/attack/prayer/item phase
                if (Phase == PhaseOfTurn.Confirm) {
                    if (PreviousPhase == PhaseOfTurn.Move) {
                        selectedUnit.GetComponent<PathNavigationController>().Reset();
                        selectedUnit.transform.eulerAngles = PathNavigationController.startRotation;
                        selectedUnit.transform.position = lastLocation;
                        ItemManager.ItemOnWalkOver();

                    }
                    ChangePhase(PreviousPhase);
                    //go back to action menu
                } else if (Phase == PhaseOfTurn.Attack || Phase == PhaseOfTurn.Move) {
                    CanvasManager.ToggleInputLock();
                    cursorTrans.position = new Vector3(GetX(selectedUnit), GetY(selectedUnit) + 1f, GetZ(selectedUnit));
                    ClearPainters();
                    CanvasManager.Instance.SetActiveCanvas("action", CanvasManager.Instance.previousButtonIndex);
                    ChangePhase(PhaseOfTurn.SelectAction);
                    TogglePanelUp();
                } else if (Phase == PhaseOfTurn.SelectPrayer || Phase == PhaseOfTurn.SelectItem) {
                    CanvasManager.Instance.SetActiveCanvas("action", CanvasManager.Instance.previousButtonIndex);
                    ChangePhase(PhaseOfTurn.SelectAction);
                } else if (Phase == PhaseOfTurn.Prayer || Phase == PhaseOfTurn.Item) {
                    CanvasManager.ToggleInputLock();
                    cursorTrans.position = new Vector3(GetX(selectedUnit), GetY(selectedUnit) + 1f, GetZ(selectedUnit));
                    ClearPainters();
                    CanvasManager.Instance.SetActiveCanvas(CanvasManager.Instance.previousCanvas, CanvasManager.Instance.previousButtonIndex);
                    TogglePanelUp();
                    ChangePhase(PreviousPhase);
                }
            }

            #region Directional Input
            //disable cursor movement on Confirm phase
            if (Phase != PhaseOfTurn.Confirm && inputType == InputType.None || inputType == InputType.Directional) {
                //handling initial press/joystick move
                if (directions[(0 + camRotationCounter) % 4](deadZone) && !directions[(2 + camRotationCounter) % 4](deadZone)
                    && isPastHoldTime == false && GetZ(cursor) < terrain.GetComponent<Renderer>().bounds.size.z - 1 && inputType == InputType.None) {
                    Cursor.GoNorth();
                    timer += Time.deltaTime;
                    isPastHoldTime = true;
                    inputType = InputType.Directional;
                } else if (directions[(2 + camRotationCounter) % 4](deadZone) && !directions[(0 + camRotationCounter) % 4](deadZone)
                    && isPastHoldTime == false && GetZ(cursor) > 0 && inputType == InputType.None) {
                    Cursor.GoSouth();
                    timer += Time.deltaTime;
                    isPastHoldTime = true;
                    inputType = InputType.Directional;
                } else if (directions[(1 + camRotationCounter) % 4](deadZone) && !directions[(3 + camRotationCounter) % 4](deadZone)
                    && isPastHoldTime == false && GetX(cursor) < terrain.GetComponent<Renderer>().bounds.size.x - 1 && inputType == InputType.None) {
                    Cursor.GoEast();
                    timer += Time.deltaTime;
                    isPastHoldTime = true;
                    inputType = InputType.Directional;
                } else if (directions[(3 + camRotationCounter) % 4](deadZone) && !directions[(1 + camRotationCounter) % 4](deadZone)
                    && isPastHoldTime == false && GetX(cursor) > 0 && inputType == InputType.None) {
                    Cursor.GoWest();
                    timer += Time.deltaTime;
                    isPastHoldTime = true;
                    inputType = InputType.Directional;
                }
                //after timer is past delay time, continue to move at rate dictated by repeat time
                if (isPastHoldTime) {
                    timer += Time.deltaTime;
                    if (timer >= delayTime && !isRepeating) {
                        isRepeating = true;
                        timer = 0f;
                    }
                    if (timer >= repeatTime && isRepeating) {
                        timer = 0f;
                        if (GetZ(cursor) < terrain.GetComponent<Renderer>().bounds.size.z - 1 && directions[(0 + camRotationCounter) % 4](deadZone) && !directions[(2 + camRotationCounter) % 4](deadZone)) {
                            Cursor.GoNorth();
                        }
                        if (GetZ(cursor) > 0 && directions[(2 + camRotationCounter) % 4](deadZone) && !directions[(0 + camRotationCounter) % 4](deadZone)) {
                            Cursor.GoSouth();
                        }
                        if (GetX(cursor) < terrain.GetComponent<Renderer>().bounds.size.x - 1 && directions[(1 + camRotationCounter) % 4](deadZone) && !directions[(3 + camRotationCounter) % 4](deadZone)) {
                            Cursor.GoEast();
                        }
                        if (GetX(cursor) > 0 && directions[(3 + camRotationCounter) % 4](deadZone) && !directions[(1 + camRotationCounter) % 4](deadZone)) {
                            Cursor.GoWest();
                        }
                    }
                }
            }
            //Stop moving
            if ((Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow)
               || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)
               || DirectionsReleased(deadZone)) && isPastHoldTime) {
                isPastHoldTime = false;
                isRepeating = false;
                timer = 0f;
                inputType = InputType.None;
            }
            //Debug: report tile under cursor
            Cursor.ltile = Cursor.ctile;
            #endregion

        }


        #region Camera Movement
        //handle clockwise cam movements
        if ((!camInUse && (Input.GetButton("CamClock") || Input.GetAxis("CamRotate") > 0f)
            && !Input.GetButton("CamCounterclock") && timer == 0f && inputType == InputType.None)
            || camInUse && timer > 0f && inputType == InputType.CamClock) {
            if (timer == 0f) {
                startRotation = cameraParentTrans.rotation;
                inputType = InputType.CamClock;
            }
            timer += Time.deltaTime;
            if (timer > cameraTimerMax) timer = cameraTimerMax;
            cameraParentTrans.rotation = Quaternion.Slerp(startRotation, startRotation * Quaternion.Euler(0f, -45f, 0f), timer * 3f);
        }
        //stop when timer reaches goal
        if (timer == cameraTimerMax && inputType == InputType.CamClock) {
            timer = 0f;
            if (Mathf.Round(45 + cameraParentTrans.rotation.eulerAngles.y) % 90 == 0) {
                camRotationCounter++;
            }
        }
        //handle counter-clockwise cam movements
        if ((!camInUse && (Input.GetButton("CamCounterclock") || Input.GetAxis("CamRotate") < 0f)
            && !Input.GetButton("CamClock") && timer == 0f && inputType == InputType.None)
            || camInUse && timer > 0f && inputType == InputType.CamCounterclock) {
            if (timer == 0f) {
                startRotation = cameraParentTrans.rotation;
                inputType = InputType.CamCounterclock;
            }
            timer += Time.deltaTime;
            if (timer > cameraTimerMax) timer = cameraTimerMax;
            cameraParentTrans.rotation = Quaternion.Slerp(startRotation, startRotation * Quaternion.Euler(0f, 45f, 0f), timer * 3f);
        }
        //stop when timer reaches goal
        if (timer == cameraTimerMax && inputType == InputType.CamCounterclock) {
            timer = 0f;
            if (Mathf.Round(cameraParentTrans.rotation.eulerAngles.y) % 90 == 0) {
                camRotationCounter--;
            }
        }

        //tracking camera rotation for directional input
        if (camRotationCounter > 3) camRotationCounter = 0;
        else if (camRotationCounter < 0) camRotationCounter = 3;

        //handle camera elevation movements
        if ((!camInUse && Input.GetButtonDown("CamElevation") && inputType == InputType.None)
            || camInUse && timer > 0f && inputType == InputType.CamElevation) {
            if (timer == 0f) {
                startRotation = ePivotTrans.localRotation;
                inputType = InputType.CamElevation;
            }
            timer += Time.deltaTime;
            if (timer > cameraTimerMax) timer = cameraTimerMax;
            isResettingCamParentOffset = true;
            ePivotTrans.localRotation = Quaternion.Slerp(startRotation, Quaternion.Euler(elevations[camElevationCounter], 45f, 0f), timer * 3f);
        }

        //stop when timer reaches goal
        if (timer == cameraTimerMax && inputType == InputType.CamElevation) {
            timer = 0f;
            camElevationCounter++;
        }
        if (camElevationCounter > 3) camElevationCounter = 0;

        //handle camera zoom movements
        if ((!camInUse && Input.GetButtonDown("CamZoom") && inputType == InputType.None)
            || camInUse && timer > 0f && inputType == InputType.CamZoom) {
            if (timer == 0f) {
                startZoom = camTrans.localPosition;
                inputType = InputType.CamZoom;
            }
            timer += Time.deltaTime;
            if (timer > cameraTimerMax) timer = cameraTimerMax;
            isResettingCamParentOffset = true;
            camTrans.localPosition = Vector3.Lerp(startZoom, new Vector3(0f, 0f, zooms[camZoomCounter]), timer * 3f);
        }

        //stop when timer reaches goal
        if (timer == cameraTimerMax && inputType == InputType.CamZoom) {
            timer = 0f;
            if (camZoomCounter == 0) SetThresholds(.4f, .6f);
            if (camZoomCounter == 1) SetThresholds(.45f, .55f);
            if (camZoomCounter == 2) SetThresholds(.35f, .65f);
            camZoomCounter++;
        }

        if (camZoomCounter > 2) camZoomCounter = 0;


        if (Camera.main.transform.position != lastCameraPosition) {
            camInUse = true;
            lastCameraPosition = Camera.main.transform.position;
        } else {
            camInUse = false;
            inputType = InputType.None;
        }

        #endregion
    }


    #region Util Methods

    public void PostPhaseEval() {
        int bttnIndex = 0;
        if ((int)PreviousPhase == 4) bttnIndex = 1;
        ClearPainters();
        if (selectedUnit.GetComponent<Unit>().ActiveUnit) {
            cursorTrans.position = new Vector3(GetX(selectedUnit), GetY(selectedUnit) + 1f, GetZ(selectedUnit));
            CanvasManager.Instance.SetActiveCanvas("action", bttnIndex);
            ChangePhase(PhaseOfTurn.SelectAction);
            TogglePanelUp();
        } else {
            cursorTrans.position = new Vector3(GetX(selectedUnit), GetY(selectedUnit) + 1f, GetZ(selectedUnit));
            selectedUnit = null;
            selectedTile = null;
            CanvasManager.Instance.SetActiveCanvas("none");
            if (Phase == PhaseOfTurn.SelectAction) TogglePanelUp();
            ChangePhase(PhaseOfTurn.SelectUnit);
            isPlayerTurn = !isPlayerTurn;
        }
    }

    private void PerformAction(UnityEvent action) {
        action.Invoke();
        selectedUnit.GetComponent<Unit>().HasActed = true;
        PostPhaseEval();
    }

    private void PreActionCheck() {
        if (availableTargets.Exists(x => (x.x == GetX(cursor)) && (x.y == GetZ(cursor)))) {
            targetTile = availableTargets.Find(x => (x.x == GetX(cursor)) && (x.y == GetZ(cursor)));
            targetUnit = TargetOnTile(targetTile, unitList);
            if (targetUnit != null) {
                ChangePhase(PhaseOfTurn.Confirm);
            }
        }
    }

    private void SetLastLocation() {
        lastLocation = new Vector3(GetX(selectedUnit), GetY(selectedUnit), GetZ(selectedUnit));
    }

    public void TogglePanelUp() {
        isPanelUp = !isPanelUp;
    }

    private bool PausingGame() {
        return (Input.GetButtonDown("Cancel") && Input.GetKey(KeyCode.LeftShift) || Input.GetButtonDown("Start"));
    }

    public void MainMenu() {
        StartCoroutine(CanvasManager.Instance.FadeAndLoadScene("MainMenu"));
    }

    public void Quit() {
        StartCoroutine(CanvasManager.Instance.FadeAndQuit());
    }

    public static void CleanButtonList() {
        List<IndexButton> temp = new List<IndexButton>();
        for (int x = contextButtonList.Count - 1; x > -1; x--) {
            if (contextButtonList[x] == null) temp.Add(contextButtonList[x]);
        }
        contextButtonList = temp;
    }

    #endregion

    #region Input Methods
    public static bool GoingWest(float deadZone) {
        return Input.GetKey(KeyCode.LeftArrow) || Input.GetAxis("Horizontal") < -deadZone;
    }

    public static bool GoingEast(float deadZone) {
        return Input.GetKey(KeyCode.RightArrow) || Input.GetAxis("Horizontal") > deadZone;
    }

    public static bool GoingSouth(float deadZone) {
        return Input.GetKey(KeyCode.DownArrow) || Input.GetAxis("Vertical") < -deadZone;
    }

    public static bool GoingNorth(float deadZone) {
        return Input.GetKey(KeyCode.UpArrow) || Input.GetAxis("Vertical") > deadZone;
    }

    public static bool DirectionsReleased(float deadZone) {
        if (GoingNorth(deadZone) || GoingSouth(deadZone) || GoingEast(deadZone) || GoingWest(deadZone)) {
            return false;
        }
        return true;
    }

    #endregion
}


