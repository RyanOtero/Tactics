﻿using System.Collections;
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
                    if (CanvasManager.Instance.previousCanvas == "none") {
                        TogglePanelUp();
                    }
                    if (WasInputLocked) {
                        CanvasManager.ToggleInputLock();
                        WasInputLocked = false;
                    }
                    CanvasManager.Instance.SetActiveCanvas(CanvasManager.Instance.previousCanvas.ToLower().Replace("menucanvas(clone)", ""), CanvasManager.Instance.previousButtonIndex);
                }
            }
        }
        if (isPanelUp) {

            //look for menu input
            CanvasManager.Instance.NavMenu();
            CanvasManager.Instance.NavHorizontalMenu();
            CanvasManager.Instance.NavOptions();
            //Handle first frame after selecting an item in menu
            if (Phase == PhaseOfTurn.Attack && !isPaused) {
                SetLastLocation();
                availableTargetTiles = GetAvailableTargetRange(SelectedUnit, true, false);
                PaintRange(availableTargetTiles, "Prefab/Painters/AttackPainter");
                Cursor.ChangeCursorColors(true);
                TogglePanelUp();
                CanvasManager.Instance.SetActiveCanvas("none");
            } else if ((Phase == PhaseOfTurn.Prayer || Phase == PhaseOfTurn.Item) && !isPaused) {
                SetLastLocation();
                availableTargetTiles = GetAvailableTargetRange(SelectedUnit, false, false);
                PaintRange(availableTargetTiles, "Prefab/Painters/ActionPainter");
                PaintCursorRange(GetTargetedTiles(), "Prefab/Painters/CursorPainter");
                Cursor.ChangeCursorColors(true);
                TogglePanelUp();
                CanvasManager.Instance.SetActiveCanvas("none");
            } else if (Phase == PhaseOfTurn.Move && !isPaused) {
                SetLastLocation();
                PathNavigationController.startRotation = SelectedUnit.transform.eulerAngles;
                availableMoves = GetMoveRange(SelectedUnit);
                PaintRange(availableMoves, "Prefab/Painters/MovePainter");
                CanvasManager.Instance.SetActiveCanvas("none");
                TogglePanelUp();
            }

            //Cancel out of menu
            else if (Input.GetButtonDown("Cancel") && !Input.GetKey(KeyCode.LeftShift) && !isPaused) {
                if (Phase == PhaseOfTurn.SelectAction && !SelectedUnit.HasActed && !SelectedUnit.HasMoved) {
                    CanvasManager.Instance.SetActiveCanvas("none");
                    ChangePhase(PhaseOfTurn.SelectUnit);
                    SelectedUnit = null;
                    TogglePanelUp();
                } else if (Phase == PhaseOfTurn.SelectItem || Phase == PhaseOfTurn.SelectPrayer) {
                    CanvasManager.Instance.SetActiveCanvas("action", Phase == PhaseOfTurn.SelectItem ? 2 : 3);
                    ChangePhase(PhaseOfTurn.SelectAction);
                    Cursor.ChangeCursorColors();
                } else if (Phase == PhaseOfTurn.UnitInfo) {
                    CanvasManager.Instance.SetActiveCanvas("action", 4);

                }
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

            if (Phase == PhaseOfTurn.SelectUnit) {
                ShowUnitInfo();
            } else if (Phase != PhaseOfTurn.SelectUnit && Phase != PhaseOfTurn.Move) {
                ShowTargetInfo();
            }

            //handling submit button when panel is down 
            if (Input.GetButtonDown("Submit")) {
                //different phases
                if (Phase == PhaseOfTurn.SelectUnit) {
                    SelectedUnit = SelectUnit();
                    if (SelectedUnit != null) {
                        CanvasManager.Instance.SetActiveCanvas("action");
                        ChangePhase(PhaseOfTurn.SelectAction);
                        TogglePanelUp();
                    }
                    //move goes to confirm phase
                } else if (Phase == PhaseOfTurn.Move) {
                    if (availableMoves.Exists(x => (x.x == GetX(cursor)) && (x.y == GetZ(cursor)))) {
                        selectedTile = availableMoves.Find(x => (x.x == GetX(cursor)) && (x.y == GetZ(cursor)));
                        SelectedUnit.GetComponent<PathNavigationController>().isMoving = true;
                        ChangePhase(PhaseOfTurn.Confirm);
                    }
                    //actions go to confirm phase
                } else if (Phase == PhaseOfTurn.Attack) {
                    Cursor.ChangeCursorColors(true);
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
                    } else if (SelectedUnit.GetComponent<PathNavigationController>().isMoving == false) {
                        CanvasManager.ToggleInputLock();
                        ItemManager.PickUpItem();
                        SelectedUnit.HasMoved = true;
                        PostPhaseEval();
                    }
                } else if (Phase == PhaseOfTurn.PickDirection) {
                    SelectedUnit.transform.eulerAngles = new Vector3(0, CanvasManager.Instance.directions.transform.rotation.eulerAngles.y, 0);
                    Destroy(CanvasManager.Instance.directions);
                    SelectedUnit.pickedDirection = true;
                    cursor.transform.Find("CursorFlag").gameObject.SetActive(true);
                    PostPhaseEval();
                }
            }

            if (Input.GetButtonDown("Cancel") && Phase == PhaseOfTurn.SelectUnit) {
                cursorTrans.position = new Vector3(GetX(activeUnitList[0]), GetY(activeUnitList[0]) + 1f, GetZ(activeUnitList[0]));
            }

            //handle cancel when panel is down
            if (Input.GetButtonDown("Cancel") && !Input.GetKey(KeyCode.LeftShift) && SelectedUnit != null) {
                if (Phase != PhaseOfTurn.Confirm) Cursor.ChangeCursorColors();
                //go back to move/attack/prayer/item phase
                if (Phase == PhaseOfTurn.Confirm) {
                    if (PreviousPhase == PhaseOfTurn.Move) {
                        SelectedUnit.GetComponent<PathNavigationController>().Reset();
                        SelectedUnit.transform.eulerAngles = PathNavigationController.startRotation;
                        SelectedUnit.transform.position = lastLocation;
                        ItemManager.ItemOnWalkOver();
                    }

                    ChangePhase(PreviousPhase);
                    //go back to action menu
                } else if (Phase == PhaseOfTurn.Attack || Phase == PhaseOfTurn.Move) {
                    CanvasManager.ToggleInputLock();
                    cursorTrans.position = new Vector3(GetX(SelectedUnit), GetY(SelectedUnit) + 1f, GetZ(SelectedUnit));
                    ClearPainters();
                    ClearCursorPainters();
                    Cursor.ChangeCursorColors();
                    CanvasManager.Instance.SetActiveCanvas("action", CanvasManager.Instance.previousButtonIndex);
                    ChangePhase(PhaseOfTurn.SelectAction);
                    TogglePanelUp();
                } else if (Phase == PhaseOfTurn.Prayer || Phase == PhaseOfTurn.Item) {
                    CanvasManager.ToggleInputLock();
                    cursorTrans.position = new Vector3(GetX(SelectedUnit), GetY(SelectedUnit) + 1f, GetZ(SelectedUnit));
                    ClearPainters();
                    ClearCursorPainters();
                    Cursor.ChangeCursorColors();
                    Cursor.isRestricted = false;
                    CanvasManager.Instance.SetActiveCanvas(CanvasManager.Instance.previousCanvas,
                        CanvasManager.Instance.previousButtonIndex, CanvasManager.Instance.previousButtonColumnIndex);
                    TogglePanelUp();
                    if (Phase == PhaseOfTurn.Prayer) {
                        ChangePhase(PhaseOfTurn.SelectPrayer);
                    } else {
                        ChangePhase(PhaseOfTurn.SelectItem);
                    }
                }
            }

            #region Directional Input
            //disable cursor movement on Confirm and PickDirection phases
            if (Phase != PhaseOfTurn.Confirm && Phase != PhaseOfTurn.PickDirection && inputType == InputType.None || inputType == InputType.Directional) {
                bool moved = false;
                bool inRange = false;
                //handling initial press/joystick move
                if (directions[(0 + camRotationCounter) % 4](deadZone) && !directions[(2 + camRotationCounter) % 4](deadZone)
                    && isPastHoldTime == false && GetZ(cursor) < terrain.GetComponent<Renderer>().bounds.size.z - 1 && inputType == InputType.None) {
                    if (Cursor.isRestricted) {
                        Tile t = new Tile(GetX(cursor), GetZ(cursor) + 1);
                        restrictedCursorRange.ForEach(x => { if (x.SameXY(t)) inRange = true; });
                        if (inRange) {
                            Cursor.GoNorth();
                            moved = true;
                        }
                    } else {
                        Cursor.GoNorth();
                        moved = true;
                    }
                } else if (directions[(2 + camRotationCounter) % 4](deadZone) && !directions[(0 + camRotationCounter) % 4](deadZone)
                    && isPastHoldTime == false && GetZ(cursor) > 0 && inputType == InputType.None) {
                    if (Cursor.isRestricted) {
                        Tile t = new Tile(GetX(cursor), GetZ(cursor) - 1);
                        restrictedCursorRange.ForEach(x => { if (x.SameXY(t)) inRange = true; });
                        if (inRange) {
                            Cursor.GoSouth();
                            moved = true;
                        }
                    } else {
                        Cursor.GoSouth();
                        moved = true;
                    }
                } else if (directions[(1 + camRotationCounter) % 4](deadZone) && !directions[(3 + camRotationCounter) % 4](deadZone)
                    && isPastHoldTime == false && GetX(cursor) < terrain.GetComponent<Renderer>().bounds.size.x - 1 && inputType == InputType.None) {
                    if (Cursor.isRestricted) {
                        Tile t = new Tile(GetX(cursor) + 1, GetZ(cursor));
                        restrictedCursorRange.ForEach(x => { if (x.SameXY(t)) inRange = true; });
                        if (inRange) {
                            Cursor.GoEast();
                            moved = true;
                        }
                    } else {
                        Cursor.GoEast();
                        moved = true;
                    }
                } else if (directions[(3 + camRotationCounter) % 4](deadZone) && !directions[(1 + camRotationCounter) % 4](deadZone)
                    && isPastHoldTime == false && GetX(cursor) > 0 && inputType == InputType.None) {
                    if (Cursor.isRestricted) {
                        Tile t = new Tile(GetX(cursor) - 1, GetZ(cursor));
                        restrictedCursorRange.ForEach(x => { if (x.SameXY(t)) inRange = true; });
                        if (inRange) {
                            Cursor.GoWest();
                            moved = true;
                        }
                    } else {
                        Cursor.GoWest();
                        moved = true;
                    }
                }
                if (moved) {
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
            if (Phase == PhaseOfTurn.PickDirection) {

                if (CanvasManager.Instance.directions == null) {
                    CanvasManager.Instance.directions = Instantiate(Resources.Load<GameObject>("Prefab/Direction"), SelectedUnit.transform.position, Quaternion.identity, SelectedUnit.transform);
                }
                if (directions[(0 + camRotationCounter) % 4](deadZone) && !directions[(2 + camRotationCounter) % 4](deadZone)) {
                    CanvasManager.Instance.directions.transform.eulerAngles = new Vector3(0, 0, 0);
                } else if (directions[(2 + camRotationCounter) % 4](deadZone) && !directions[(0 + camRotationCounter) % 4](deadZone)) {
                    CanvasManager.Instance.directions.transform.eulerAngles = new Vector3(0, 180f, 0);
                } else if (directions[(1 + camRotationCounter) % 4](deadZone) && !directions[(3 + camRotationCounter) % 4](deadZone)) {
                    CanvasManager.Instance.directions.transform.eulerAngles = new Vector3(0, 90, 0);
                } else if (directions[(3 + camRotationCounter) % 4](deadZone) && !directions[(1 + camRotationCounter) % 4](deadZone)) {
                    CanvasManager.Instance.directions.transform.eulerAngles = new Vector3(0, -90, 0);
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
        ClearCursorPainters();
        Cursor.ChangeCursorColors();
        Cursor.isRestricted = false;
        int bttnIndex = 0;
        if ((int)PreviousPhase == 4) bttnIndex = 1;
        ClearPainters();

        if (SelectedUnit.ActiveUnit) {
            cursorTrans.position = new Vector3(GetX(SelectedUnit), GetY(SelectedUnit) + 1f, GetZ(SelectedUnit));
            CanvasManager.Instance.SetActiveCanvas("action", bttnIndex);
            ChangePhase(PhaseOfTurn.SelectAction);
            TogglePanelUp();
            CanvasManager.Instance.targetInfoPanel.GetComponent<UnitInfoPanel>().Clear();
            Instance.StartCoroutine(CanvasManager.FadeUIElement(null, CanvasManager.Instance.targetInfoPanel, false));
        } else {
            if (SelectedUnit.pickedDirection) {
                unitList.RemoveAll(x => x == null);
                foreach (Unit unit in unitList) {
                    unit.DeadCheck();
                }
                activeUnitList.Remove(SelectedUnit);
                GetTurns();
                SelectedUnit.pickedDirection = false;
                SelectedUnit = null;
                cursorTrans.position = new Vector3(GetX(activeUnitList[0]), GetY(activeUnitList[0]) + 1f, GetZ(activeUnitList[0]));
                selectedTile = null;
                CanvasManager.Instance.SetActiveCanvas("none");
                if (Phase == PhaseOfTurn.SelectAction) TogglePanelUp();
                ChangePhase(PhaseOfTurn.SelectUnit);
                CanvasManager.Instance.unitInfoPanel.GetComponent<UnitInfoPanel>().Clear();
                Instance.StartCoroutine(CanvasManager.FadeUIElement(null, CanvasManager.Instance.unitInfoPanel, false));
                CanvasManager.Instance.targetInfoPanel.GetComponent<UnitInfoPanel>().Clear();
                Instance.StartCoroutine(CanvasManager.FadeUIElement(null, CanvasManager.Instance.targetInfoPanel, false));
            } else {
                ChangePhase(PhaseOfTurn.PickDirection);
                cursorTrans.position = new Vector3(GetX(SelectedUnit), GetY(SelectedUnit) + 1f, GetZ(SelectedUnit));
                cursor.transform.Find("CursorFlag").gameObject.SetActive(false);

            }
        }
    }

    private void PerformAction(UnityEvent action) {
        action.Invoke();
        SelectedUnit.HasActed = true;
        PostPhaseEval();
    }

    private void PreActionCheck() {
        targetedTiles = GetTargetedTiles();
        if (availableTargetTiles.Exists(x => (x.x == GetX(cursor)) && (x.y == GetZ(cursor)))) {
            targetUnits = TargetsOnTiles(targetedTiles, unitList);
            if (targetUnits.Count > 0) {
                ChangePhase(PhaseOfTurn.Confirm);
            } else if (Phase == PhaseOfTurn.Prayer) {
                ChangePhase(PhaseOfTurn.Confirm);
            }
        }
    }

    private void SetLastLocation() {
        lastLocation = new Vector3(GetX(SelectedUnit), GetY(SelectedUnit), GetZ(SelectedUnit));
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


