using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMaterialScript : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<CarController>().Kill();
        }
    }
}
