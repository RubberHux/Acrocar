using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    public static bool easyAim;
    private static bool easyAimNew;
    private static bool startedByPause = false;
    [SerializeField] private GameObject pause, backDoubleCheck;
    [SerializeField] private Toggle easyAimToggle;
    [SerializeField] private Button SaveButton;
    public UIController uiController;

    private void Awake()
    {
        easyAim = PlayerPrefs.GetInt("easyAim", 0) == 1;
        easyAimNew = easyAim;
        easyAimToggle.isOn = easyAim;
        gameObject.SetActive(false);
    }

    public void StartFromPause()
    {
        startedByPause = true;
        uiController.SetState(UIController.gameState.Settings);
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
            uiController.SetState(UIController.gameState.Paused);
        }
        backDoubleCheck.SetActive(false);
        startedByPause = false;
    }

    public void SetEasyAim(bool easyAimValue)
    {
        easyAimNew = easyAimValue;
        CheckSame();
    }
}
