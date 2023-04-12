using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu, winMenu;
    [SerializeField] private TextMeshProUGUI[] uiTimeText;
    [SerializeField] private SettingsHandler settingsHandler;
    private double time;
    private InputAction pause;
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Settings,
        Win,
    }
    private GameState gameState;

    public void SetState(GameState newState)
    {
        gameState = newState;
    }

    private void OnEnable()
    {
        pause = InputHandler.playerInput.LevelInteraction.Pause;
        pause.Enable();
        pause.performed += PerformPause;
        if (SceneManager.GetActiveScene().buildIndex == 0) gameState = GameState.MainMenu;
        else gameState = GameState.Playing;
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
        if (gameState == GameState.Playing)
        {
            gameState = GameState.Paused;
            Time.timeScale = 0.0f;
            pauseMenu.SetActive(true);
        }
        else if (gameState == GameState.Paused)
        {
            gameState = GameState.Playing;
            Time.timeScale = 1.0f;
            pauseMenu.SetActive(false);
        }
        else if (gameState == GameState.Settings)
        {
            settingsHandler.Back();
        }
    }

    public void SetWin()
    {
        gameState = GameState.Win;
        Time.timeScale = 0.0f;
        winMenu.SetActive(true);
    }

    private void Update()
    {
        if (gameState == GameState.Playing)
        {
            time += Time.deltaTime;
            string timeString = String.Format("{0:0.00}", time) + "s";
            for (int i = 0; i < uiTimeText.Length; i++)
            {
                uiTimeText[i].text = timeString;
            }
        }
        
    }
}
