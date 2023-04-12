using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu, winMenu, mainMenuButtons;
    [SerializeField] private GameObject pauseDefualtButton, winDefualtButton, settingsDefualtButton, mainMenuDefualtButton;
    [SerializeField] private TextMeshProUGUI[] uiTimeText;
    [SerializeField] private SettingsHandler settingsHandler;
    private EventSystem eventSystem;
    private double time;
    private InputAction pause, uiNavigate;
    private GameObject lastObject;
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Settings,
        Win,
    }
    public GameState gameState { get; private set; }

    public void SetState(GameState newState)
    {
        if (gameState == GameState.MainMenu)
        {
            foreach (Button button in mainMenuButtons.GetComponentsInChildren<Button>()) button.interactable = false;
        }
        gameState = newState;
    }

    private void OnEnable()
    {
        eventSystem = EventSystem.current;
        pause = InputHandler.playerInput.LevelInteraction.Pause;
        pause.Enable();
        pause.performed += PerformPause;
        uiNavigate = InputHandler.playerInput.UI.Navigate;
        uiNavigate.Enable();
        uiNavigate.performed += UINavFix;
        //eventSystem = gameObject.GetComponent<EventSystem>();

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

    private void UINavFix(InputAction.CallbackContext context)
    {
        if (eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(lastObject);
        }
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

    public void BackTo(GameState state, GameObject lastSelected = null)
    {
        if (state == GameState.Paused)
        {
            pauseMenu.SetActive(true);
            SetState(GameState.Paused);
        }
        if (state == GameState.MainMenu)
        {
            foreach(Button button in mainMenuButtons.GetComponentsInChildren<Button>()) button.interactable = true;
            SetState(GameState.MainMenu);
        }
        if (lastSelected != null) eventSystem.SetSelectedGameObject(lastSelected);
    }

    public void SetWin()
    {
        gameState = GameState.Win;
        Time.timeScale = 0.0f;
        winMenu.SetActive(true);
    }

    private void Update()
    {
        if (eventSystem.currentSelectedGameObject != null) lastObject = eventSystem.currentSelectedGameObject;
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
