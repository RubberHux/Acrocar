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
        pause = carController.playerControls.LevelInteraction.Pause;
        pause.Enable();
    }
    
    private void Update()
    {
        if (pause.WasPressedThisFrame())
        {
            _paused = !_paused;
            uiCanvas.enabled = _paused;
            Time.timeScale = _paused ? 0.0f : 1.0f;
        }
    }
}
