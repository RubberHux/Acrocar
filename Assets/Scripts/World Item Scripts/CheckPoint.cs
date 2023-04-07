using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private Renderer renderMaker;

    void Start()
    {
        renderMaker = GetComponent<Renderer>();
        renderMaker.material.color = Color.cyan;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CarController carController = other.GetComponent<CarController>();
            if (carController.lastCheckPoint != null) carController.lastCheckPoint.UnColour();
            carController.lastCheckPoint = this;
            renderMaker.material.color = Color.blue;
        }
    }

    public void UnColour()
    {
        renderMaker.material.color = Color.cyan;
    }
}
