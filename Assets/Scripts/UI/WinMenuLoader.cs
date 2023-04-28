using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WinMenuLoader : MonoBehaviour
{
    [SerializeField] private GameObject defaultSelect;
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(defaultSelect);
    }
}
