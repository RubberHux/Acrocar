using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WinMenuLoader : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private GameObject defaultSelect;
    // Start is called before the first frame update
    private void OnEnable()
    {
        eventSystem.SetSelectedGameObject(defaultSelect);
    }
}
