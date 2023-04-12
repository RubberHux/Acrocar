using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    public static bool easyAim;
    private static bool easyAimNew;
    private static bool startedByPause = false;
    [SerializeField] private GameObject pause, backDoubleCheck, lastSelected;
    [SerializeField] private Toggle easyAimToggle;
    [SerializeField] private Button SaveButton;
    [SerializeField] private EventSystem eventSystem;
    public UIController uiController;

    private void Awake()
    {
        easyAim = PlayerPrefs.GetInt("easyAim", 0) == 1;
        easyAimNew = easyAim;
        easyAimToggle.isOn = easyAim;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        lastSelected = eventSystem.currentSelectedGameObject;
        eventSystem.SetSelectedGameObject(easyAimToggle.gameObject);
    }

    public void StartFromPause()
    {
        startedByPause = true;
        uiController.SetState(UIController.GameState.Settings);
    }

    public void Save()
    {
        easyAim = easyAimNew;
        PlayerPrefs.SetInt("easyAim", easyAim ? 1 : 0);
        Exit();
    }

    public void Back()
    {
        if (!CheckSame()) backDoubleCheck.SetActive(true);
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
        if (startedByPause)
        {
            pause.SetActive(true);
            uiController.SetState(UIController.GameState.Paused);
        }
        else uiController.SetState(UIController.GameState.MainMenu);
        backDoubleCheck.SetActive(false);
        startedByPause = false;
        eventSystem.SetSelectedGameObject(lastSelected);
    }

    public void SetEasyAim(bool easyAimValue)
    {
        easyAimNew = easyAimValue;
        CheckSame();
    }
}
