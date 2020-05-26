using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
public class IndexButton : MonoBehaviour {

    //public static IndexButton lastButton;
    public int index;
    private Animator animator;
    public bool isConfirmable;
    [SerializeField]
    private bool isEnabled;
    public UnityEvent confirm;
    private Color faceColor;
    private Color outlineColor;

    public bool IsEnabled {
        get => isEnabled;
        set {
            //if disabled, dim the color
            if (value) {
                gameObject.GetComponent<TextMeshProUGUI>().faceColor = faceColor;
                gameObject.GetComponent<TextMeshProUGUI>().outlineColor = outlineColor;
            } else {
                gameObject.GetComponent<TextMeshProUGUI>().faceColor = faceColor * new Color(.5f, .5f, .5f, 1f);
                gameObject.GetComponent<TextMeshProUGUI>().outlineColor = outlineColor * new Color(.5f, .5f, .5f, 1f);
                if (animator == null) animator = gameObject.GetComponent<Animator>();
                animator.SetBool("isSelected", false);
            }
            isEnabled = value;
        }
    }

    private void Start() {
        animator = gameObject.GetComponent<Animator>();
        faceColor = gameObject.GetComponent<TextMeshProUGUI>().faceColor;
        outlineColor = gameObject.GetComponent<TextMeshProUGUI>().outlineColor;
    }
    void Update() {
        if (animator.GetBool("isConfirmed")) {
            animator.SetBool("isConfirmed", false);
        }
        if (IsEnabled) {
            if (CanvasManager.Instance.buttonIndex == index) {
                animator.SetBool("isSelected", true);
                if (Input.GetButtonDown("Submit") && isConfirmable) {
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
