using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InputManager;
using static CanvasManager;

public class MainMenuManager : MonoBehaviour {

    public static MainMenuManager Instance { get; private set; } = null;

    void Awake() {
        //Singleton Check
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        if (InputManager.Instance==null) {

        }

    }

    void Update() {
        CanvasManager.Instance.NavMenu();
        CanvasManager.Instance.NavOptions();
    }

    public void Quit() {
        StartCoroutine(CanvasManager.Instance.FadeAndQuit());
    }
}
