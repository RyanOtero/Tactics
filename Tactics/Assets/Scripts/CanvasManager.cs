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
    public float fadeSpeed;
    private static bool isHeld;

    public int maxButtonIndex;
    public int maxButtonColumnIndex;
    //[HideInInspector]
    public GameObject activeCanvas;
    //[HideInInspector]
    public string previousCanvas;
    [HideInInspector]
    public string[] menus = new[] { "main", "options", "load", "controls", "action", "pause", "prayer", "item", "none" };
    [HideInInspector]
    public GameObject transition;
    [HideInInspector]
    public AudioMixer musicMixer;
    [HideInInspector]
    public AudioMixer fxMixer;
    [HideInInspector]
    public TextMeshProUGUI resolutionText;
    [HideInInspector]
    public Toggle fullScreenToggle;
    [HideInInspector]
    public Slider music;
    [HideInInspector]
    public Slider fx;
    public int buttonIndex;
    public int buttonColumnIndex;
    public int previousButtonIndex;
    public int previousButtonColumnIndex;
    public GameObject unitInfoPanel;
    public GameObject targetInfoPanel;
    public GameObject directions;

    private int currentResIndex;
    private List<SaveData> saveList;
    private GameObject canvasToDestroy;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        resolutions = null;
        if (fadeSpeed == 0) fadeSpeed = 5f;
    }

    void OnEnable() {
        buttonIndex = 0;
        previousButtonIndex = 0;
    }

    void Start() {
        //setup for main menu/pause menu
        maxButtonIndex = 3;
        saveList = null;
        if (SceneManager.GetActiveScene().name == "MainMenu") {
            activeCanvas = GetCanvas("main");
        } else {
            Cursor.ChangeCursorColors();
        }
    }

    void Update() {
        if (canvasToDestroy != null) {
            if (canvasToDestroy.GetComponent<CanvasGroup>().alpha == 0) {
                Destroy(canvasToDestroy);
            }
        }
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
        transition.GetComponent<Animator>().SetTrigger("FadeOut");
        yield return new WaitForSeconds(1f);
        ToggleInputLock();
        Destroy(activeCanvas);
        SceneManager.LoadScene(SceneToLoad, LoadSceneMode.Single);
    }

    public static void SetCanvasAlpha(int fadeDirection, GameObject uiElement) {
        uiElement.GetComponent<CanvasGroup>().alpha += Time.unscaledDeltaTime * (Instance.fadeSpeed / 1.0f) * ((fadeDirection == 0) ? -1 : 1);
    }

    public static IEnumerator FadeUIElement(GameObject uiElementIn, GameObject uiElementOut, bool toggleLock = true) {
        if (toggleLock) ToggleInputLock();
        float n = 1;
        while (n >= 0) {
            if (uiElementOut != null) SetCanvasAlpha(0, uiElementOut);
            if (uiElementIn != null) SetCanvasAlpha(1, uiElementIn);
            n -= Time.unscaledDeltaTime * (Instance.fadeSpeed / 1.0f);

            yield return null;
        }
        if (toggleLock) ToggleInputLock();
    }

    public IEnumerator ChangeScreenCoroutine(string canvasType) {
        transition.GetComponent<Animator>().SetTrigger("FadeOut");
        yield return new WaitForSecondsRealtime(.5f);
        SetActiveCanvasInstantly(canvasType);
        transition.GetComponent<Animator>().SetTrigger("FadeIn");
    }

    public void SavePreviousCanvasName() {
        if (activeCanvas != null) {
            previousCanvas = activeCanvas.name.ToLower().Replace("menucanvas(clone)", "");
        } else {
            previousCanvas = "none";
        }
    }

    public void SetActiveCanvas(string canvasTypeToSwitchTo, int bttnIndex = 0, int bttnColIndex = 0) {
        if (activeCanvas == null || !activeCanvas.name.ToLower().Contains(canvasTypeToSwitchTo.ToLower())) {
            GameObject canvasToSwitchTo = null;
            InputManager.CleanButtonList();
            canvasToSwitchTo = GetCanvas(canvasTypeToSwitchTo);
            StartCoroutine(FadeUIElement(canvasToSwitchTo, activeCanvas));
            previousButtonIndex = buttonIndex;
            previousButtonColumnIndex = buttonColumnIndex;
            canvasToDestroy = activeCanvas;
            buttonIndex = bttnIndex;
            buttonColumnIndex = bttnColIndex;
            SavePreviousCanvasName();
            activeCanvas = canvasToSwitchTo;
        }
    }

    //Only used with black transition
    public void SetActiveCanvasInstantly(string canvasTypeToSwitchTo) {
        previousButtonIndex = buttonIndex;
        SavePreviousCanvasName();
        buttonIndex = 0;
        GameObject canvasToSwitchTo;
        InputManager.CleanButtonList();
        canvasToSwitchTo = GetCanvas(canvasTypeToSwitchTo);
        canvasToDestroy = activeCanvas;
        activeCanvas = canvasToSwitchTo;
        CanvasGroup cg = activeCanvas.GetComponent<CanvasGroup>();
        CanvasGroup cgDestroy = canvasToDestroy.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1;
        if (cgDestroy != null) cgDestroy.alpha = 0;
    }


    //"main", "options", "load", "controls", "action", "pause", "prayer", "item", "none"

    public void ButtonConfirmAssign(GameObject canvas) {
        foreach (IndexButton bttn in canvas.GetComponentsInChildren<IndexButton>()) {
            if (bttn.name == "NewGame") bttn.confirm.AddListener(() => { GetCanvas("newgame"); });
            if (bttn.name == "LoadGame") bttn.confirm.AddListener(() => { ChangeScreen("load"); });
            if (bttn.name == "SaveGame") bttn.confirm.AddListener(() => { BattleManager.Instance.SaveGame(); });
            if (bttn.name == "Options") bttn.confirm.AddListener(() => { ChangeScreen("options"); });
            if (bttn.name == "Quit") bttn.confirm.AddListener(() => { StartCoroutine(FadeAndQuit()); });
            if (bttn.name == "MainMenu") bttn.confirm.AddListener(() => { StartCoroutine(FadeAndLoadScene("MainMenu")); });
            if (bttn.name == "Fullscreen") bttn.confirm.AddListener(FlipFullscreenToggle);
            if (bttn.name == "Controls") bttn.confirm.AddListener(() => { ChangeScreen("controls"); });
            if (bttn.name == "Move") bttn.confirm.AddListener(() => { BattleManager.ChangePhase(PhaseOfTurn.Move); });
            if (bttn.name == "Attack") bttn.confirm.AddListener(() => { BattleManager.ChangePhase(PhaseOfTurn.Attack); });
            if (bttn.name == "Prayer") bttn.confirm.AddListener(() => {
                BattleManager.ChangePhase(PhaseOfTurn.SelectPrayer);
                SetActiveCanvas("prayer");
            });
            if (bttn.name == "Item") bttn.confirm.AddListener(() => {
                BattleManager.ChangePhase(PhaseOfTurn.SelectItem);
                SetActiveCanvas("inventory");
            });
            if (bttn.name == "Info") bttn.confirm.AddListener(() => {
                BattleManager.ChangePhase(PhaseOfTurn.UnitInfo);
                SetActiveCanvas("info");
            });
            if (bttn.name == "EndTurn") bttn.confirm.AddListener(() => {
                BattleManager.SelectedUnit.HasMoved = true;
                BattleManager.SelectedUnit.HasActed = true;
                Instance.SetActiveCanvas("none");
                InputManager.Instance.TogglePanelUp();
                BattleManager.ChangePhase(PhaseOfTurn.SelectUnit);
               Instance.unitInfoPanel.GetComponent<UnitInfoPanel>().Clear();
                Instance.StartCoroutine(CanvasManager.FadeUIElement(null,Instance.unitInfoPanel, false));
               Instance.targetInfoPanel.GetComponent<UnitInfoPanel>().Clear();
                Instance.StartCoroutine(CanvasManager.FadeUIElement(null,Instance.targetInfoPanel, false));
                InputManager.Instance.PostPhaseEval();
            });

            if (bttn.name == "Back") {
                if (SceneManager.GetActiveScene().name == "MainMenu") {
                    if (!canvas.name.ToLower().Contains("controls")) {
                        bttn.confirm.AddListener(() => { ChangeScreen("main"); });
                    } else {
                        bttn.confirm.AddListener(() => { ChangeScreen("options"); });
                    }
                } else if (SceneManager.GetActiveScene().name == "Battle") {
                    if (!canvas.name.ToLower().Contains("controls")) {
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
                maxButtonIndex = 3;
                maxButtonColumnIndex = 0;
                break;
            case "options":
                canvas = Instantiate(Resources.Load("Prefab/UI/OptionsMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                SetupOptionsMenu(canvas);
                maxButtonIndex = 5;
                maxButtonColumnIndex = 0;
                break;
            case "load":
                canvas = Instantiate(Resources.Load("Prefab/UI/LoadMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                PopulateLoadMenu();
                maxButtonColumnIndex = 0;
                break;
            case "controls":
                canvas = Instantiate(Resources.Load("Prefab/UI/ControlsMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                maxButtonIndex = 0;
                maxButtonColumnIndex = 0;
                break;
            case "newgame":
                if (activeCanvas.name.ToLower().Contains("main")) BattleManager.saveData = null;
                StartCoroutine(FadeAndLoadScene("Battle"));
                maxButtonIndex = 0;
                maxButtonColumnIndex = 0;
                break;
            case "action":
                canvas = Instantiate(Resources.Load("Prefab/UI/ActionMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                canvas.GetComponent<CanvasGroup>().alpha = 0;
                PopulateActionMenu(canvas);
                maxButtonIndex = 5;
                maxButtonColumnIndex = 0;
                break;
            case "pause":
                canvas = Instantiate(Resources.Load("Prefab/UI/PauseMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                canvas.GetComponent<CanvasGroup>().alpha = 0;
                maxButtonIndex = 4;
                maxButtonColumnIndex = 0;
                SavePreviousCanvasName();
                break;
            case "prayer":
                canvas = Instantiate(Resources.Load("Prefab/UI/PrayerMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                canvas.GetComponent<CanvasGroup>().alpha = 0;
                PopulatePrayerMenu();
                maxButtonColumnIndex = 1;
                break;
            case "inventory":
                canvas = Instantiate(Resources.Load("Prefab/UI/InventoryMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                canvas.GetComponent<CanvasGroup>().alpha = 0;
                PopulateInventoryMenu();
                maxButtonColumnIndex = 1;
                break;
            case "info":
                canvas = Instantiate(Resources.Load("Prefab/UI/InfoMenuCanvas")) as GameObject;
                canvas.GetComponent<Canvas>().worldCamera = Camera.main;
                canvas.GetComponent<CanvasGroup>().alpha = 0;
                PopulateInfoMenu();
                maxButtonColumnIndex = 0;
                break;
            case "store":

                break;
            case "none":
                maxButtonIndex = 0;
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

    #region Options Setup
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
    #endregion

    public void PopulateActionMenu(GameObject canvas) {
        List<string> buttonsToDestroy = new List<string> { "move", "attack", "item", "prayer", "endturn" };
        List<string> actions = new List<string>() { "attack", "item", "prayer" };
        if (BattleManager.SelectedUnit == BattleManager.activeUnitList[0]) {
            foreach (IndexButton button in canvas.GetComponentsInChildren<IndexButton>()) {
                if (actions.Contains(button.name.ToLower()) && BattleManager.SelectedUnit.HasActed) button.IsEnabled = false;
                if (button.name.ToLower() == "move" && BattleManager.SelectedUnit.HasMoved) button.IsEnabled = false;
                InputManager.contextButtonList.Add(button);
                if (button.name.ToLower() == "prayer" && BattleManager.SelectedUnit.maxFaith == 0) button.IsEnabled = false;
            }

        } else {
            IndexButton[] buttons = canvas.GetComponentsInChildren<IndexButton>();
            for (int i = buttons.Length - 1; i > -1; i--) {
                if (buttonsToDestroy.Contains(buttons[i].name.ToLower())) {
                    Destroy(buttons[i].gameObject);
                }
            }
        }

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
        maxButtonIndex = saveList.Count;
        GameObject.Find("LoadMenuCanvas(Clone)").transform.Find("Back").GetComponent<IndexButton>().index = maxButtonIndex;
    }

    public void PopulateInventoryMenu() {
        GameObject parent = GameObject.Find("Content");
        //clear existing items
        for (int i = 0; i < parent.transform.childCount; i++) {
            Destroy(parent.transform.GetChild(i).gameObject);
        }
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
            name.rectTransform.sizeDelta = new Vector2(1100, 120);
            qty.text = item.Value.ToString();
            thumbnail.sprite = itemData.sprite;
            IndexButton indexButton = name.GetComponent<IndexButton>();
            InputManager.contextButtonList.Add(indexButton);
            if (index % 2 == 0) {
                indexButton.index = index / 2;
                indexButton.columnIndex = 0;
            } else {
                indexButton.index = index / 2;
                indexButton.columnIndex = 1;
            }
            index++;
            ConsumableItemData cid = itemData as ConsumableItemData;
            PersistentItemData pid = itemData as PersistentItemData;
            indexButton.select.AddListener(() => {
                GameObject desc = GameObject.Find("Description");
                desc.GetComponent<TextMeshProUGUI>().text = itemData.description;
            });
            if (cid == null) {
                indexButton.IsDimmed = true;
                qty.GetComponent<TextMeshProUGUI>().fontMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF Grey +103");
            } else {
                indexButton.confirm.AddListener(() => {
                    BattleManager.UseItem = cid.effect;
                    BattleManager.ChangePhase(PhaseOfTurn.Item);
                });
            }

        }
        maxButtonIndex = ItemManager.Instance.inventory.Count % 2 == 0 ? ItemManager.Instance.inventory.Count / 2 - 1 : ItemManager.Instance.inventory.Count / 2;
    }

    public void PopulatePrayerMenu() {
        GameObject parent = GameObject.Find("Content");
        //clear existing items
        for (int i = 0; i < parent.transform.childCount; i++) {
            Destroy(parent.transform.GetChild(i).gameObject);
        }
        int index = 0;
        foreach (string prayer in BattleManager.SelectedUnit.prayers) {
            PrayerData prayerData = BattleManager.allPrayers.Find(x => x.prayerName == prayer);
            GameObject prayerPanel = Instantiate(Resources.Load("Prefab/UI/PrayerPanel")) as GameObject;
            prayerPanel.transform.SetParent(parent.transform);
            prayerPanel.transform.localPosition = Vector3.zero;
            prayerPanel.transform.localRotation = Quaternion.identity;
            prayerPanel.transform.localScale = Vector3.one;
            TextMeshProUGUI name = prayerPanel.transform.Find("PrayerName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI cost = prayerPanel.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
            Image thumbnail = prayerPanel.transform.Find("Thumbnail").GetComponent<Image>();
            name.text = prayer;
            name.rectTransform.sizeDelta = new Vector2(1100, 120);
            cost.text = prayerData.cost.ToString();
            thumbnail.sprite = prayerData.sprite;
            IndexButton indexButton = name.GetComponent<IndexButton>();
            InputManager.contextButtonList.Add(indexButton);
            if (index % 2 == 0) {
                indexButton.index = index / 2;
                indexButton.columnIndex = 0;
            } else {
                indexButton.index = index / 2;
                indexButton.columnIndex = 1;
            }
            index++;
            indexButton.select.AddListener(() => {
                GameObject desc = GameObject.Find("Description");
                desc.GetComponent<TextMeshProUGUI>().text = prayerData.description;
            });
            if (prayerData.cost > BattleManager.SelectedUnit.Faith) {
                indexButton.IsDimmed = true;
                cost.GetComponent<TextMeshProUGUI>().fontMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF Grey +103");
            } else {
                indexButton.confirm.AddListener(() => {
                    Cursor.isRestricted = prayerData.restrictCursor;
                    BattleManager.Instance.prayerReach = prayerData.reach;
                    BattleManager.Instance.prayerTargetStyle = prayerData.targetStyle;
                    BattleManager.Instance.prayerBandThickness = prayerData.bandThickness;
                    BattleManager.Instance.prayerEffectReach = prayerData.effectReach;
                    BattleManager.Instance.prayerEffectTargetStyle = prayerData.effectTargetStyle;
                    BattleManager.Instance.prayerEffectBandThickness = prayerData.effectBandThickness;
                    BattleManager.UsePrayer = prayerData.effect;
                    BattleManager.ChangePhase(PhaseOfTurn.Prayer);
                });
            }

        }
        int maxIndex = BattleManager.SelectedUnit.prayers.Count % 2 == 0 ? BattleManager.SelectedUnit.prayers.Count / 2 - 1 : BattleManager.SelectedUnit.prayers.Count / 2;
        maxButtonIndex = maxIndex;
    }

    private void PopulateInfoMenu() {
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
        int cIndex;
        if (!IsInputLocked) {
            //skip button if it's disabled
            if (InputManager.contextButtonList.Count > 1) {
                if (InputManager.contextButtonList[buttonIndex].IsEnabled == false) {
                    buttonIndex++;
                    LoopIndexCheck();
                }
            }
            if (InputManager.GoingNorth(InputManager.deadZone) && !isHeld) {
                isHeld = true;
                if (buttonIndex == 0) {
                    cIndex = buttonColumnIndex;
                    buttonColumnIndex--;
                    LoopColumnIndexCheck();
                }
                buttonIndex--;
                LoopIndexCheck();
                if (InputManager.Instance != null && activeCanvas != null && activeCanvas.name.ToLower().Contains("action")) {
                    for (int i = InputManager.contextButtonList.Count - 1; i > -1; i--) {
                        if (InputManager.contextButtonList[i].index == buttonIndex
                            && InputManager.contextButtonList[i].IsEnabled == false) buttonIndex--;
                    }
                }
                LoopIndexCheck();
            } else if (InputManager.GoingSouth(InputManager.deadZone) && !isHeld) {
                isHeld = true;
                if (buttonIndex == maxButtonIndex) {
                    cIndex = buttonColumnIndex;
                    buttonColumnIndex++;
                    LoopColumnIndexCheck();
                }
                buttonIndex++;
                LoopIndexCheck();
                if (InputManager.Instance != null && activeCanvas != null && activeCanvas.name.ToLower().Contains("action")) {
                    InputManager.contextButtonList.ForEach(b => { if (b.index == buttonIndex && b.IsEnabled == false) buttonIndex++; });
                }
                LoopIndexCheck();
            } else if (InputManager.DirectionsReleased(InputManager.deadZone)) {
                isHeld = false;
            }
        }
    }

    //handling horizontal input in menus
    public void NavHorizontalMenu() {
        int index;
        if (!IsInputLocked) {
            if (!InputManager.isPaused) {
                if (InputManager.GoingWest(InputManager.deadZone) && !isHeld) {
                    isHeld = true;
                    if (buttonColumnIndex == 0) {
                        index = buttonIndex;
                        buttonIndex--;
                        LoopIndexCheck();
                    }
                    buttonColumnIndex--;
                    LoopColumnIndexCheck();
                } else if (InputManager.GoingEast(InputManager.deadZone) && !isHeld) {
                    isHeld = true;
                    if (buttonColumnIndex == maxButtonColumnIndex) {
                        index = buttonIndex;
                        buttonIndex++;
                        LoopIndexCheck();
                    }
                    buttonColumnIndex++;
                    LoopColumnIndexCheck();
                } else if (InputManager.DirectionsReleased(InputManager.deadZone)) {
                    isHeld = false;
                }

            }
        }
    }

    public static void ToggleInputLock() {
        IsInputLocked = !IsInputLocked;
    }

    public void LoopIndexCheck() {
        if (buttonIndex != -100) {
            if (buttonIndex > maxButtonIndex) buttonIndex = 0;
            else if (buttonIndex < 0) buttonIndex = maxButtonIndex;
        }
    }

    public void LoopColumnIndexCheck() {
        if (buttonColumnIndex > maxButtonColumnIndex) buttonColumnIndex = 0;
        else if (buttonColumnIndex < 0) buttonColumnIndex = maxButtonColumnIndex;
    }

    #endregion

    public void OnConfirmLoadGame() {
        BattleManager.saveData = saveList[buttonIndex];
        Destroy(activeCanvas);
        StartCoroutine(FadeAndLoadScene("Battle"));
    }

    public void OnSelectLoadGame() {

    }
}
