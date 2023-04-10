using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    public static bool easyAim;
    private static bool easyAimNew;
    [SerializeField] private GameObject settings, pause, backDoubleCheck;
    [SerializeField] private Toggle easyAimToggle;
    [SerializeField] private Button SaveButton;

    private void Awake()
    {
        easyAim = PlayerPrefs.GetInt("easyAim", 0) == 1;
        easyAimNew = easyAim;
        easyAimToggle.isOn = easyAim;
        gameObject.SetActive(false);
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
        settings.SetActive(false);
        pause.SetActive(true);
        backDoubleCheck.SetActive(false);
    }

    public void SetEasyAim(bool easyAimValue)
    {
        easyAimNew = easyAimValue;
        CheckSame();
    }
}
