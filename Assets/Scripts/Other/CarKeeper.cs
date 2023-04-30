using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Car
{
    public GameObject prefab;
    public string name;
    public Texture2D thumbnail;
}

public class CarKeeper : MonoBehaviour
{
    public Car[] cars;
}
