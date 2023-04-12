using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLoader : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject car, carInstance;
    public bool is2D;

    private void OnEnable()
    {
        if (carInstance == null)
        {
            carInstance = Instantiate(car, transform);
            carInstance.GetComponent<CarController>().is2D = is2D;
        }
    }
}
