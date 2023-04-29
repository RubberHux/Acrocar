using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSelectButton : MonoBehaviour
{
    // Start is called before the first frame update
    [NonSerialized] public int index;
    [NonSerialized] public CarSelectMenu menu;

    public void Select()
    {
        menu.SetCar(index);
        print(index);
    }
}
