using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] SceneType sceneType;
    [SerializeField] private GameObject pauseMenu, winMenu, settingsMenu, gameUI, addPlayerMenu, mainMenu, editorWinMenu;
    private GameObject pauseMenuInstance, addPlayerMenuInstance;
    [NonSerialized] public GameObject settingsInstance;
    private List<TextMeshProUGUI> uiTimeText = new List<TextMeshProUGUI>();
    Transform follow = null;
    private EventSystem eventSystem;
    private double time;
    private InputAction pause, uiNavigate;
    private GameObject lastObject;
    public GameObject winInstance;
    public enum SceneType
    {
        MainMenu,
        HubWorld,
        Level,
        CarCustomization,
        LevelEditor
    }
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Settings,
        Win,
        CarCustomization,
        LevelEditor
    }
    public GameState gameState { get; private set; }
    bool vrCamTryGet = false;

    public AudioSource winsound;

    public void SetState(GameState newState)
    {
        if (gameState == GameState.MainMenu && newState != GameState.MainMenu)
        {
            foreach (Button button in GetComponentInChildren<MainMenu>().gameObject.GetComponentsInChildren<Button>())
            {
                print(button);
                button.interactable = false;
            }
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
        else if (sceneType == SceneType.LevelEditor) gameState = GameState.LevelEditor;
        else gameState = GameState.Playing;

        if (sceneType == SceneType.HubWorld || sceneType == SceneType.Level)
        {
            pauseMenuInstance = Instantiate(pauseMenu, transform);
            if (sceneType == SceneType.HubWorld) pauseMenuInstance.GetComponentInChildren<HubWorldButton>().gameObject.SetActive(false);
            Instantiate(gameUI, transform).GetComponentsInChildren<TextMeshProUGUI>().ToList().ForEach(text => { if (text.gameObject.CompareTag("TimeText")) uiTimeText.Append(text); });
        }
        else if (sceneType == SceneType.MainMenu) Instantiate(mainMenu, transform);
        settingsInstance = Instantiate(settingsMenu, transform);

        if (GameMaster.vr && Camera.main != null)
        {
            SetVRMode();
        }
        else if (GameMaster.vr) vrCamTryGet = true;
    }

    private void OnDisable()
    {
        pause.performed -= PerformPause;
        uiNavigate.performed -= UINavFix;
    }

    void SetVRMode()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.current;
        follow = Camera.main.transform;
        canvas.transform.localScale = new Vector3(0.003709593f, 0.003709593f, 0.003709593f);
        vrCamTryGet = false;
    }

    private void PerformPause(InputAction.CallbackContext context)
    {
        UpdatePause();
    }

    private void UINavFix(InputAction.CallbackContext context)
    {
        if (eventSystem.currentSelectedGameObject == null)
        {
            if (lastObject != null) eventSystem.SetSelectedGameObject(lastObject);
            else if (gameState != GameState.Playing && GetComponentInChildren<Button>() != null) eventSystem.SetSelectedGameObject(GetComponentInChildren<Button>().gameObject);
        }
    }

    public void UpdatePause()
    {
        if (gameState == GameState.Playing)
        {
            gameState = GameState.Paused;
            Time.timeScale = 0.0f;
            pauseMenuInstance.SetActive(true);
        }
        else if (gameState == GameState.Paused)
        {
            gameState = GameState.Playing;
            Time.timeScale = 1.0f;
            pauseMenuInstance.SetActive(false);
        }
    }

    public void BackTo(GameState state, GameObject lastSelected = null)
    {
        if (state == GameState.Paused)
        {
            pauseMenuInstance.SetActive(true);
            SetState(GameState.Paused);
        }
        if (state == GameState.MainMenu)
        {
            foreach (Button button in GetComponentInChildren<MainMenu>().gameObject.GetComponentsInChildren<Button>()) button.interactable = true;
            SetState(GameState.MainMenu);
        }
        if (lastSelected != null) eventSystem.SetSelectedGameObject(lastSelected);
    }

    public void OpenAddPlayers()
    {
        if (addPlayerMenuInstance == null) addPlayerMenuInstance = Instantiate(addPlayerMenu, transform);
        addPlayerMenuInstance.SetActive(true);
    }

    public void SetWin()
    {
        gameState = GameState.Win;
        Time.timeScale = 0.0f;

        if (winInstance == null)
        {
            if (sceneType == SceneType.LevelEditor) winInstance = Instantiate(editorWinMenu, transform);
            else winInstance = Instantiate(winMenu, transform);
        }
        winInstance.GetComponentsInChildren<TextMeshProUGUI>().ToList().ForEach(x => x.text = x.gameObject.CompareTag("TimeText") ? String.Format("{0:0.00}", time) + "s" : x.text);
        time = 0;
        winsound.Play();
 

    }

    private void Update()
    {
        if (vrCamTryGet && Camera.main != null) SetVRMode();
        if (eventSystem.currentSelectedGameObject != null) lastObject = eventSystem.currentSelectedGameObject;
        if (gameState == GameState.Playing)
        {
            time += Time.deltaTime;
            string timeString = String.Format("{0:0.00}", time) + "s";
            //for (int i = 0; i < uiTimeText.Length; i++)
            {
                //uiTimeText[i].text = timeString;
            }
        }
        if (follow != null)
        {
            transform.position = follow.position + transform.forward * 4;
            transform.rotation = follow.rotation;
        }
    }
}
