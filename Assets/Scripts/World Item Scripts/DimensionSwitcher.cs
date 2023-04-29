using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimensionSwitcher : MonoBehaviour
{
    public bool to2D;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<CarController>().DimensionSwitch(to2D);
        }
    }
}
