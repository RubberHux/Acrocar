using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BackDoubleCheckLoader : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject defaultButton;
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(defaultButton);
    }
}
