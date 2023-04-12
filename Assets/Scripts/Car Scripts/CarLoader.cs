using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLoader : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject car;
    public bool is2D;

    private void OnEnable()
    {
        car = Instantiate(car, transform);
        car.GetComponent<CarController>().is2D = is2D;
    }
}
