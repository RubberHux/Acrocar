using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class CarCustomisationMain : MonoBehaviour
{
    [SerializeField] private GameObject defaultSelect;
    [SerializeField] MultiplayerEventSystem eventSystem;

    private void OnEnable()
    {
        eventSystem.SetSelectedGameObject(defaultSelect);
    }

}
