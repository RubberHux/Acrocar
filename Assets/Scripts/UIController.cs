using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public Canvas uiCanvas;

    private bool _paused = false;
    
    private void Start()
    {
        uiCanvas.enabled = false;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _paused = !_paused;
            uiCanvas.enabled = _paused;
            Time.timeScale = _paused ? 0.0f : 1.0f;
        }
    }
}
