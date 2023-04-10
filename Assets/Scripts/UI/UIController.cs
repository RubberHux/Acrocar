using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public Canvas uiCanvas;
    public CarController carController;
    private InputAction pause;

    private bool _paused = false;
    
    private void Start()
    {
        uiCanvas.enabled = false;
    }

    private void OnEnable()
    {
        pause = InputHandler.playerInput.LevelInteraction.Pause;
        pause.Enable();
        pause.performed += UpdatePause;
    }

    private void OnDisable()
    {
        pause.performed -= UpdatePause;
    }


    private void UpdatePause(InputAction.CallbackContext context)
    {
        _paused = !_paused;
        uiCanvas.enabled = _paused;
        Time.timeScale = _paused ? 0.0f : 1.0f;
    }
}
