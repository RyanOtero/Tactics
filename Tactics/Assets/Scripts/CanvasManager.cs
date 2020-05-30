using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TacticsUtil;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class CanvasManager : MonoBehaviour {

    public static CanvasManager Instance { get; private set; } = null;
    public static bool IsInputLocked { get; private set; }
    public static List<Resolution> resolutions;
    private static bool isHeld;
    public static float fadeSpeed = .75f;

    public int ActiveCanvasIndex { get { return activeCanvasIndex; } set { activeCanvasIndex = value; } }
    public int CurrentCanvasMaxButtonIndex { get; private set; }
    //[HideInInspector]
    public GameObject activeCanvas;
    //[HideInInspector]
    public string previousCanvas;
    [HideInInspector]
    public string[] menus = new[] { "main", "options", "load", "controls", "action", "pause", "prayer", "item", "none" };
    public GameObject transition;
    public AudioMixer musicMixer;
    public AudioMixer fxMixer;
    public TextMeshProUGUI resolutionText;
    public Toggle fullScreenToggle;
    public Slider music;
    public Slider fx;
    public int buttonIndex;
    public int previousButtonIndex;
    private int currentResIndex;
    private List<SaveData> saveList;
    private string[] menusStrArr;
    private int activeCanvasIndex;


    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        resolutions = null;
    }

    void OnEnable() {
        buttonIndex = 0;
        previousButtonIndex = 0;
    }

    void Start() {
        //setup for main menu/pause menu
        CurrentCanvasMaxButtonIndex = 3;
        saveList = null;
        if (SceneManager.GetActiveScene().name == "MainMenu") activeCanvas = GetCanvas("main");
    }

    public void ChangeScreen(string canvasToChangeTo) {
        buttonIndex = -100;
        StartCoroutine(ChangeScreenCoroutine(canvasToChangeTo));
    }

    public IEnumerator FadeAndQuit() {
        ToggleInputLock();
        Time.timeScale = 1;
        transition.GetComponent<Animator>().SetTrigger("FadeOut");
        yield return new WaitForSeconds(1f);
        ToggleInputLock();
        Application.Quit();
    }

    public IEnumerator FadeAndLoadScene(string SceneToLoad) {
        ToggleInputLock();
        Time.timeScale = 1;
        Debug.Log("TimeScale: " + Time.timeScale);
        transition.GetComponent<Animator>().SetTrigger("FadeOut");
        yield return new WaitForSeconds(1f);
        ToggleInputLock();
        Destroy(activeCanvas);
        SceneManager.LoadScene(SceneToLoad, LoadSceneMode.Single);
    }

    public static void SetCanvasAlpha(int fadeDirection, GameObject uiElement) {
        uiElement.GetComponent<CanvasGroup>().alpha += Time.unscaledDeltaTime * (1.0f / fadeSpeed) * 5 * ((fadeDirection == 0) ? -1 : 1);
    }

    public static IEnumerator FadeUIElement(GameObject uiElementOut, GameObject uiElementIn) {
        ToggleInputLock();
        float n = 1;
        while (n >= 0) {
            if (uiElementOut != null) SetCanvasAlpha(0, uiElementOut);
            if (uiElementIn != null) SetCanvasAlpha(1, uiElementIn);
            n -= Time.unscaledDeltaTime * (1.0f / fadeSpeed) * 5;

            yield return null;
        }
        ToggleInputLock();
    }

    public IEnumerator ChangeScreenCoroutine(string canvasType) {
        transition.GetComponent<Animator>().SetTrigger("FadeOut");
        yield return new WaitForSecondsRealtime(1f);
        SetActiveCanvasInstantly(canvasType);
        transition.GetComponent<Animator>().SetTrigger("FadeIn");
    }

    public void SetMaxIndexForCurrentCanvas(int maxIndex) {
        CurrentCanvasMaxButtonIndex = maxIndex;
    }

    public void UpdatePreviousCanvas() {
        if (activeCanvas != null) {
            previousCanvas = activeCanvas.name.ToLower().Replace("menucanvas(clone)", "");
        }
    }

    public void SetActiveCanvas(string canvasTypeToSwitchTo, int bttnIndex) {
        if (activeCanvas == null || !activeCanvas.name.ToLower().Contains(canvasTypeToSwitchTo.ToLower())) {
            GameObject canvasToSwitchTo = null;
            InputManager.CleanButtonList();
            canvasToSwitchTo = GetCanvas(canvasTypeToSwitchTo);
            StartCoroutine(FadeUIElement(activeCanvas, canvasToSwitchTo));
            previousButtonIndex = buttonIndex;
            buttonIndex = bttnIndex;
            UpdatePreviousCanvas();
            Destroy(activeCanvas);
            activeCanvas = canvasToSwitchTo;
        }
    }

    public void SetActiveCanvas(string canvasTypeToSwitchTo) {
        if (activeCanvas == null || !activeCanvas.name.ToLower().Contains(canvasTypeToSwitchTo.ToLower())) {
            GameObject canvasToSwitchTo = null;
            InputManager.CleanButtonList();
            canvasToSwitchTo = GetCanvas(canvasTypeToSwitchTo);
            StartCoroutine(FadeUIElement(activeCanvas, canvasToSwitchTo));
            if (canvasToSwitchTo != null) {
                previousButtonIndex = buttonIndex;
                buttonIndex = 0;
            } else {
                StartCoroutine(WaitToChangeIndex());
            }
            UpdatePreviousCanvas();
            Destroy(activeCanvas);
            activeCanvas = canvasToSwitchTo;
        }
    }

    //Only used with black transition
    public void SetActiveCanvasInstantly(string canvasTypeToSwitchTo) {
        previousButtonIndex = buttonIndex;
        previousCanvas = activeCanvas.name.ToLower().Replace("menucanvas(clone)","");
        buttonIndex = 0;
        GameObject canvasToSwitchTo = null;
        InputManager.CleanButtonList();
        canvasToSwitchTo = GetCanvas(canvasTypeToSwitchTo);
        Destroy(activeCanvas);
        activeCanvas = canvasToSwitchTo;
        CanvasGroup cg = activeCanvas.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1;
    }


    //"main", "options", "load", "controls", "action", "pause", "prayer", "item", "none"

    public void ButtonConfirmAssign(GameObject canvas) {
        foreach (IndexButton bttn in canvas.GetComponentsInChildren<IndexButton>()) {
            if (bttn.gameObject.name == "NewGame") bttn.confirm.AddListener(() => { GetCanvas("newgame"); });
            if (bttn.gameObject.name == "LoadGame") bttn.confirm.AddListener(() => { ChangeScreen("load"); });
            if (bttn.gameObject.name == "SaveGame") bttn.confirm.AddListener(() => { BattleManager.Instance.SaveGame(); });
            if (bttn.gameObject.name == "Options") bttn.confirm.AddListener(() => { ChangeScreen("options"); });
            if (bttn.gameObject.name == "Quit") bttn.confirm.AddListener(() => { StartCoroutine(FadeAndQuit()); });
            if (bttn.gameObject.name == "MainMenu") bttn.confirm.AddListener(() => { StartCoroutine(FadeAndLoadScene("MainMenu")); });
            if (bttn.gameObject.name == "Fullscreen") bttn.confirm.AddListener(FlipFullscreenToggle);
            if (bttn.gameObject.name == "Controls") bttn.confirm.AddListener(() => { ChangeScreen("controls"); });
            if (bttn.gameObject.name == "Move") bttn.confirm.AddListener(() => { BattleManager.ChangePhase(PhaseOfTurn.Move); });
            if (bttn.gameObject.name == "Attack") bttn.confirm.AddListener(() => { BattleManager.ChangePhase(PhaseOfTurn.Attack); });
            if (bttn.gameObject.name == "Prayer") bttn.confirm.AddListener(() => {
                BattleManager.ChangePhase(PhaseOfTurn.SelectPrayer);
                SetActiveCanvas("prayer");
            });
            if (bttn.gameObject.name == "Item") bttn.confirm.AddListener(() => {
                BattleManager.ChangePhase(PhaseOfTurn.SelectItem);
                SetActiveCanvas("inventory");
            });
            if (bttn.gameObject.name == "EndTurn") bttn.confirm.AddListener(() => {
                BattleManager.selectedUnit.GetComponent<Unit>().HasMoved = true;
                BattleManager.selectedUnit.GetComponent<Unit>().HasActed = true;
                InputManager.Instance.PostPhaseEval();
                BattleManager.ChangePhase(PhaseOfTurn.SelectUnit);
            });

            if (bttn.gameObject.name == "Back") {
                if (SceneManager.GetActiveScene().name == "MainMenu") {
                    if (!canvas.gameObject.name.ToLower().Contains("controls")) {
                        bttn.confirm.AddListener(() => { ChangeScreen("main"); });
                    } else {
                        bttn.confirm.AddListener(() => { ChangeScreen("options"); });
                    }
                } else if (SceneManager.GetActiveScene().name == "Battle") {
                    if (!canvas.gameObject.name.ToLower().Contains("controls")) {
                        bttn.confirm.AddListener(() => { ChangeScreen("pause"); });
                    } else {
                        bttn.confirm.AddListener(() => { ChangeScreen("options"); });
                    }
                }
            }
        }
    }

    public GameObject GetCanvas(string canvasType) {
        GameObject canvas = null;
        switch (canvasType) {
            case "main":
                canvas = Instantiate(Resources.Load("Prefab/UI/MainMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                CurrentCanvasMaxButtonIndex = 3;
                break;
            case "options":
                canvas = Instantiate(Resources.Load("Prefab/UI/OptionsMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                SetupOptionsMenu(canvas);
                CurrentCanvasMaxButtonIndex = 5;
                break;
            case "load":
                canvas = Instantiate(Resources.Load("Prefab/UI/LoadMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                PopulateLoadMenu();
                break;
            case "controls":
                canvas = Instantiate(Resources.Load("Prefab/UI/ControlsMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                CurrentCanvasMaxButtonIndex = 0;
                break;
            case "newgame":
                if (activeCanvas.name.ToLower().Contains("main")) BattleManager.saveData = null;
                StartCoroutine(FadeAndLoadScene("Battle"));
                CurrentCanvasMaxButtonIndex = 0;
                break;
            case "action":
                canvas = Instantiate(Resources.Load("Prefab/UI/ActionMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                SetupActionMenu(canvas);
                CurrentCanvasMaxButtonIndex = 4;
                break;
            case "pause":
                canvas = Instantiate(Resources.Load("Prefab/UI/PauseMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                CurrentCanvasMaxButtonIndex = 4;
                UpdatePreviousCanvas();
                break;
            case "prayer":

                break;
            case "inventory":
                canvas = Instantiate(Resources.Load("Prefab/UI/InventoryMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                PopulateInventoryMenu();
                break;
            case "store":

                break;
            case "none":
                CurrentCanvasMaxButtonIndex = 0;
                break;
            default:
                break;
        }
        if (canvas != null) {
            canvas.transform.SetParent(GameObject.Find("MainCanvas").transform, false);
            canvas.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            canvas.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            canvas.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            canvas.GetComponent<RectTransform>().SetLeft(0);
            canvas.GetComponent<RectTransform>().SetRight(0);
            canvas.GetComponent<RectTransform>().SetTop(0);
            canvas.GetComponent<RectTransform>().SetBottom(0);
            canvas.GetComponent<RectTransform>().localRotation = Quaternion.identity;
            ButtonConfirmAssign(canvas);
        }
        return canvas;
    }

  

    public IEnumerator WaitToChangeIndex() {
        yield return new WaitForSeconds(fadeSpeed / 5 + .015f);
        previousButtonIndex = buttonIndex;
        buttonIndex = 0;
    }

    public void MusicVolume(float value) {
        musicMixer.SetFloat("MusicVol", value);
    }

    public void FXVolume(float value) {
        fxMixer.SetFloat("FXVol", value);
    }

    public void SetFullScreen(bool isFullScreen) {
        Screen.fullScreen = isFullScreen;
    }

    public void FlipFullscreenToggle() {
        fullScreenToggle.isOn = !fullScreenToggle.isOn;
        SetFullScreen(fullScreenToggle.isOn);
    }

    public void SetResolution(int index) {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        resolutionText.SetText(resolutions[index].width + " x " + resolutions[index].height);

    }

  #region Menu Navigation Handling
 
    //handling horizontal input on options menu
    public void NavOptions() {
        if (!IsInputLocked) {
            if (InputManager.GoingEast(InputManager.deadZone) && !isHeld) {
                if (activeCanvas.name.ToLower().Contains("options")) {
                    if (buttonIndex == 0) {
                        isHeld = true;
                        if (currentResIndex < resolutions.Count - 1) {
                            currentResIndex++;
                            SetResolution(currentResIndex);
                        }
                    } else if (buttonIndex == 2) {
                        if (music.value < music.maxValue) {
                            music.value += .05f;
                        }
                    } else if (buttonIndex == 3) {
                        if (fx.value < fx.maxValue) {
                            fx.value += .05f;
                        }
                    }
                }
            } else if (InputManager.GoingWest(InputManager.deadZone) && !isHeld) {
                if (activeCanvas.name.ToLower().Contains("options")) {
                    if (buttonIndex == 0) {
                        isHeld = true;
                        if (currentResIndex > 0) {
                            currentResIndex--;
                            SetResolution(currentResIndex);
                        }
                    } else if (buttonIndex == 2) {
                        if (music.value > music.minValue) {
                            music.value -= .05f;
                        }
                    } else if (buttonIndex == 3) {
                        if (fx.value > fx.minValue) {
                            fx.value -= .05f;
                        }
                    }
                }
            } else if (InputManager.DirectionsReleased(InputManager.deadZone)) {
                isHeld = false;
            }
        }
    }

    //handling vertical input in menus
    public void NavMenu() {
        if (!IsInputLocked) {
            if (InputManager.contextButtonList.Count > 1) {
                if (InputManager.contextButtonList[buttonIndex].IsEnabled == false) {
                    buttonIndex++;
                    LoopIndex();
                }
            }
            if (InputManager.GoingNorth(InputManager.deadZone) && !isHeld) {
                isHeld = true;
                buttonIndex--;
                LoopIndex();
                if (InputManager.Instance != null && activeCanvas != null && activeCanvas.name.ToLower().Contains("action")) {
                    for (int i = InputManager.contextButtonList.Count - 1; i > -1; i--) {
                        if (InputManager.contextButtonList[i].index == buttonIndex
                            && InputManager.contextButtonList[i].IsEnabled == false) buttonIndex--;
                    }
                }
                LoopIndex();
            } else if (InputManager.GoingSouth(InputManager.deadZone) && !isHeld) {
                isHeld = true;
                buttonIndex++;
                LoopIndex();
                if (InputManager.Instance != null && activeCanvas != null && activeCanvas.name.ToLower().Contains("action")) {
                    InputManager.contextButtonList.ForEach(b => { if (b.index == buttonIndex && b.IsEnabled == false) buttonIndex++; });
                }
                LoopIndex();
            } else if (InputManager.DirectionsReleased(InputManager.deadZone)) {
                isHeld = false;
            }
        }
    }

    //handling horizontal input in menus
    public void NavHorizontalMenu() {
        if (!IsInputLocked) {
            if (InputManager.GoingWest(InputManager.deadZone) && !isHeld) {
                isHeld = true;
                buttonIndex--;
                LoopIndex();
                if (InputManager.Instance != null && activeCanvas != null && activeCanvas.name.ToLower().Contains("action")) {
                    for (int i = InputManager.contextButtonList.Count - 1; i > -1; i--) {
                        if (InputManager.contextButtonList[i].index == buttonIndex
                            && InputManager.contextButtonList[i].IsEnabled == false) buttonIndex--;
                    }
                }
                LoopIndex();
            } else if (InputManager.GoingEast(InputManager.deadZone) && !isHeld) {
                isHeld = true;
                buttonIndex++;
                LoopIndex();
                if (InputManager.Instance != null && activeCanvas != null && activeCanvas.name.ToLower().Contains("action")) {
                    InputManager.contextButtonList.ForEach(b => { if (b.index == buttonIndex && b.IsEnabled == false) buttonIndex++; });
                }
                LoopIndex();
            } else if (InputManager.DirectionsReleased(InputManager.deadZone)) {
                isHeld = false;
            }
        }
    }

    public static void ToggleInputLock() {
        IsInputLocked = !IsInputLocked;
    }

    public void LoopIndex() {
        if (buttonIndex != -100) {
            if (buttonIndex > CurrentCanvasMaxButtonIndex) buttonIndex = 0;
            else if (buttonIndex < 0) buttonIndex = CurrentCanvasMaxButtonIndex;
        }
    }

    #endregion

    public void SetupActionMenu(GameObject canvas) {
        List<string> actions = new List<string>() { "attack", "item", "prayer" };
        foreach (IndexButton button in canvas.GetComponentsInChildren<IndexButton>()) {
            if (actions.Contains(button.gameObject.name.ToLower()) && BattleManager.selectedUnit.GetComponent<Unit>().HasActed) button.IsEnabled = false;
            if (button.gameObject.name.ToLower() == "move" && BattleManager.selectedUnit.GetComponent<Unit>().HasMoved) button.IsEnabled = false;
            InputManager.contextButtonList.Add(button);
        }

    }

    public void SetupOptionsMenu(GameObject canvas) {
        foreach (TextMeshProUGUI tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>()) {
            if (tmp.name == "Label") resolutionText = tmp;
        }
        fullScreenToggle = canvas.GetComponentInChildren<Toggle>();

        foreach (Slider sld in canvas.GetComponentsInChildren<Slider>()) {
            if (sld.name == "MusicSlider") music = sld;
            if (sld.name == "FXSlider") fx = sld;
        }
        resolutions = new List<Resolution>();
        foreach (var t in Screen.resolutions) {
            if (!resolutions.Exists(res => res.width == t.width
                                           && res.height == t.height)) resolutions.Add(t);
        }
        resolutions.ForEach(res => {
            if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height) {
                currentResIndex = resolutions.IndexOf(res);
            }
        });
        SetResolution(currentResIndex);
        resolutionText.SetText(resolutions[currentResIndex].width + " x " + resolutions[currentResIndex].height);
    }

    public void PopulateLoadMenu() {
        GameObject parent = GameObject.Find("Content");
        saveList = null;
        //clear existing items
        for (int i = 0; i < parent.transform.childCount; i++) {
            Destroy(parent.transform.GetChild(i).gameObject);
        }
        saveList = SaveData.GetSaves();
        for (int i = 0; i < saveList.Count; i++) {
            GameObject saveDataPanel = Instantiate(Resources.Load("Prefab/UI/SaveDataPanel")) as GameObject;
            saveDataPanel.transform.SetParent(parent.transform);
            saveDataPanel.transform.localPosition = Vector3.zero;
            saveDataPanel.transform.localRotation = Quaternion.identity;
            saveDataPanel.transform.localScale = Vector3.one;
            TextMeshProUGUI slot = saveDataPanel.transform.Find("SlotNumber").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI checkpoint = saveDataPanel.transform.Find("Checkpoint").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI Time = saveDataPanel.transform.Find("Time").GetComponent<TextMeshProUGUI>();
            slot.text = saveList[i].Slot.ToString("D3");
            checkpoint.text = Checkpoints[saveList[i].Checkpoint % 7];
            Time.text = FormatTime(saveList[i].GameTime);
            IndexButton indexButton = slot.GetComponent<IndexButton>();
            indexButton.index = i;
            indexButton.confirm.AddListener(OnConfirmLoadGame);
        }
        SetMaxIndexForCurrentCanvas(saveList.Count);
        GameObject.Find("LoadMenuCanvas(Clone)").transform.Find("Back").GetComponent<IndexButton>().index = CurrentCanvasMaxButtonIndex;
    }


    public void PopulateInventoryMenu() {
        GameObject parent = GameObject.Find("Content");
        //clear existing items
        for (int i = 0; i < parent.transform.childCount; i++) {
            Destroy(parent.transform.GetChild(i).gameObject);
        }
        GameObject desc = GameObject.Find("Description");
        int index = 0;
        foreach (KeyValuePair<string, int> item in ItemManager.Instance.inventory) {
            ItemData itemData = ItemManager.Instance.allItems.Find(x => x.itemName == item.Key);
            GameObject itemPanel = Instantiate(Resources.Load("Prefab/UI/ItemPanel")) as GameObject;
            itemPanel.transform.SetParent(parent.transform);
            itemPanel.transform.localPosition = Vector3.zero;
            itemPanel.transform.localRotation = Quaternion.identity;
            itemPanel.transform.localScale = Vector3.one;
            TextMeshProUGUI name = itemPanel.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI qty = itemPanel.transform.Find("Qty").GetComponent<TextMeshProUGUI>();
            Image thumbnail = itemPanel.transform.Find("Thumbnail").GetComponent<Image>();
            name.text = item.Key;
            qty.text = item.Value.ToString();
            thumbnail.sprite = ItemManager.Instance.allItems.Find(x => x.itemName == item.Key).sprite;
            desc.GetComponent<TextMeshProUGUI>().text = itemData.description;
            IndexButton indexButton = name.GetComponent<IndexButton>();
            InputManager.contextButtonList.Add(indexButton);
            indexButton.index = index;
            index++;
            ConsumableItemData cid = itemData as ConsumableItemData;
            PersistentItemData pid = itemData as PersistentItemData;
            if (cid == null) {
                indexButton.IsEnabled = false;
            } else {
                indexButton.confirm.AddListener(() => {
                    BattleManager.UseItem = cid.effect;
                    BattleManager.ChangePhase(PhaseOfTurn.Item);
                });
            }

        }
        SetMaxIndexForCurrentCanvas(ItemManager.Instance.inventory.Count - 1);
    }

    public void OnConfirmLoadGame() {
        BattleManager.saveData = saveList[buttonIndex];
        Destroy(activeCanvas);
        StartCoroutine(FadeAndLoadScene("Battle"));
    }

    public void OnSelectLoadGame() {

    }
}
