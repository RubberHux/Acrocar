using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UIController;

public class SettingsHandler : MonoBehaviour
{
    public static bool easyAim, mpCollisions;
    private static bool easyAimNew, mpCollisionsNew;
    [SerializeField] private GameObject backDoubleCheck;
    private GameObject lastSelected;
    [SerializeField] private Toggle easyAimToggle, mpCollisionToggle;
    [SerializeField] private Button SaveButton;
    private UIController uiController;
    private GameState lastState;
    private InputAction back;


    private void Awake()
    {
        easyAim = PlayerPrefs.GetInt("easyAim", 0) == 1;
        easyAimNew = easyAim;
        easyAimToggle.isOn = easyAim;
        mpCollisions = PlayerPrefs.GetInt("mpCollisions", 0) == 1;
        mpCollisionsNew = mpCollisions;
        mpCollisionToggle.isOn = mpCollisions;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        uiController = GetComponentInParent<UIController>();
        lastSelected = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(easyAimToggle.gameObject);
        lastState = uiController.gameState;
        uiController.SetState(GameState.Settings);
        setInteractive(true);
        back = InputHandler.playerInput.LevelInteraction.Pause;
        back.Enable();
        back.performed += goBack;
    }

    private void OnDisable()
    {
        if (back != null) back.performed -= goBack;
    }

    public void Save()
    {
        easyAim = easyAimNew;
        mpCollisions = mpCollisionsNew;
        PlayerPrefs.SetInt("easyAim", easyAim ? 1 : 0);
        PlayerPrefs.SetInt("mpCollisions", mpCollisions ? 1 : 0);
        Exit();
    }

    void goBack(InputAction.CallbackContext context) { Back(); }

    public void Back()
    {
        if (!CheckSame())
        {
            setInteractive(false);
            backDoubleCheck.SetActive(true);
        }
        else
        {
            Exit();
        }
    }

    public void ResetNewVals()
    {
        easyAimNew = easyAim;
        easyAimToggle.isOn = easyAim;
        mpCollisionsNew = mpCollisions;
        mpCollisionToggle.isOn = mpCollisions;
    }

    public bool CheckSame()
    {
        bool allSame = true;
        if (easyAim != easyAimNew) allSame = false;
        if (mpCollisions != mpCollisionsNew) allSame = false;
        SaveButton.interactable = !allSame;
        return allSame;
    }

    public void Exit()
    {
        SaveButton.interactable = false;
        this.gameObject.SetActive(false);
        backDoubleCheck.SetActive(false);
        uiController.BackTo(lastState, lastSelected);
    }

    public void SetEasyAim(bool easyAimValue)
    {
        easyAimNew = easyAimValue;
        CheckSame();
    }

    public void SetMultiplayerCollisions(bool doCollide)
    {
        mpCollisionsNew = doCollide;
        CheckSame();
    }

    public void setInteractive(bool interactive)
    {
        foreach (Button button in gameObject.GetComponentsInChildren<Button>()) button.interactable = interactive;
        foreach (Toggle toggle in gameObject.GetComponentsInChildren<Toggle>()) toggle.interactable = interactive;
        if (interactive) CheckSame();
    }

    public void resetSelected()
    {
        EventSystem.current.SetSelectedGameObject(easyAimToggle.gameObject);
    }
}
