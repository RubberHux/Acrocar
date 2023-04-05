using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMaterialScript : MonoBehaviour
{
    public CarController carController;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            carController.Kill();
        }
    }
}
