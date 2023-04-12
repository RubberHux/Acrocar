using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UIController;

public class SettingsHandler : MonoBehaviour
{
    public static bool easyAim;
    private static bool easyAimNew;
    [SerializeField] private GameObject backDoubleCheck, backDoubleCheckDefault;
    private GameObject lastSelected;
    [SerializeField] private Toggle easyAimToggle;
    [SerializeField] private Button SaveButton;
    public UIController uiController;
    private GameState lastState;


    private void Awake()
    {
        easyAim = PlayerPrefs.GetInt("easyAim", 0) == 1;
        easyAimNew = easyAim;
        easyAimToggle.isOn = easyAim;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        lastSelected = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(easyAimToggle.gameObject);
        lastState = uiController.gameState;
        uiController.SetState(GameState.Settings);
        setInteractive(true);
    }

    public void Save()
    {
        easyAim = easyAimNew;
        PlayerPrefs.SetInt("easyAim", easyAim ? 1 : 0);
        Exit();
    }

    public void Back()
    {
        if (!CheckSame())
        {
            backDoubleCheck.SetActive(true);
            EventSystem.current.SetSelectedGameObject(backDoubleCheckDefault);
            setInteractive(false);
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
    }

    public bool CheckSame()
    {
        bool allSame = true;
        if (easyAim != easyAimNew) allSame = false;
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
