using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

 [System.Serializable]
public class Loadable
{
    public GameObject prefab;
    public string name;
    public Texture2D thumbnail;
}

public class CarKeeper : MonoBehaviour
{
    public Loadable[] cars, spoilers;
}
