using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    [SerializeField] Transform direction;
    [SerializeField] float power;
    [SerializeField] bool scaleByWorldGravity;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<CarController>().SetCustomGravity(direction.transform.up * power * (scaleByWorldGravity ? Physics.gravity.magnitude : 1));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<CarController>().SetCustomGravity(null);
        }
    }
}
