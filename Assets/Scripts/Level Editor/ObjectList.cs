using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectList : MonoBehaviour
{
    public List<GameObject> objects;
    public List<Toggle> toggles;
    private GameObject currentObject;

    private void Start()
    {
        currentObject = objects[0];
        toggles[0].SetIsOnWithoutNotify(true);
    }

    public void SetCurrentObject()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn) currentObject = objects[i];
        }
    }

    public GameObject GetCurrentObject()
    {
        return currentObject;
    }
}
