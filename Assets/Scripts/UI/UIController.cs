using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu, winMenu;
    [SerializeField] private SettingsHandler settingsHandler;
    private InputAction pause;
    public enum gameState
    {
        Playing,
        Paused,
        Settings,
        Win,
    }
    private gameState state;

    public void SetState(gameState newState)
    {
        state = newState;
    }

    private void OnEnable()
    {
        pause = InputHandler.playerInput.LevelInteraction.Pause;
        pause.Enable();
        pause.performed += PerformPause;
    }

    private void OnDisable()
    {
        pause.performed -= PerformPause;
    }

    private void PerformPause(InputAction.CallbackContext context)
    {
        UpdatePause();
    }

    public void UpdatePause()
    {
        if (state == gameState.Playing)
        {
            state = gameState.Paused;
            Time.timeScale = 0.0f;
            pauseMenu.SetActive(true);
        }
        else if (state == gameState.Paused)
        {
            state = gameState.Playing;
            Time.timeScale = 1.0f;
            pauseMenu.SetActive(false);
        }
        else if (state == gameState.Settings)
        {
            settingsHandler.Back();
        }
    }

    public void SetWin()
    {
        state = gameState.Win;
        Time.timeScale = 0.0f;
        winMenu.SetActive(true);
    }
}
