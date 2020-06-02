using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
public class IndexButton : MonoBehaviour {

    //public static IndexButton lastButton;
    public int index;
    public int columnIndex;
    private Animator animator;
    [SerializeField]
    private bool isEnabled;
    [SerializeField]
    private bool isDimmed;
    [SerializeField]
    private bool isConfirmable;
    public UnityEvent confirm = new UnityEvent();
    public UnityEvent select = new UnityEvent();

    public bool IsEnabled {
        get => isEnabled;
        set {
            //if disabled, dim the color
            if (value) {
                gameObject.GetComponent<TextMeshProUGUI>().fontMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF +103");
            } else {
                gameObject.GetComponent<TextMeshProUGUI>().fontMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF Grey +103");
                if (animator == null) animator = gameObject.GetComponent<Animator>();
                animator.SetBool("isSelected", false);
            }
            isEnabled = value;
        }
    }

    public bool IsDimmed {
        get => isDimmed;
        set {
            if (value) {
                gameObject.GetComponent<TextMeshProUGUI>().fontMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF Grey +103");
            } else {
                gameObject.GetComponent<TextMeshProUGUI>().fontMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF +103");
            }
            isDimmed = value;
        }
    }

    void Awake() {
    }

    void Start() {
        animator = gameObject.GetComponent<Animator>();
    }

    void Update() {
        if (animator.GetBool("isConfirmed")) {
            animator.SetBool("isConfirmed", false);
        }
        if (IsEnabled) {
            if (CanvasManager.Instance.buttonIndex == index && CanvasManager.Instance.buttonColumnIndex == columnIndex) {
                animator.SetBool("isSelected", true);
                select.Invoke();
                if (Input.GetButtonDown("Submit") && !IsDimmed && isConfirmable && gameObject.GetComponentInParent<CanvasGroup>().alpha == 1) {
                    animator.SetBool("isConfirmed", true);
                    confirm.Invoke();
                } else if (Input.GetButtonDown("Submit") && !isConfirmable) {
                    confirm.Invoke();
                } else {
                    animator.SetBool("isConfirmed", false);
                }
            } else {
                animator.SetBool("isSelected", false);
            }
        }
    }
}
