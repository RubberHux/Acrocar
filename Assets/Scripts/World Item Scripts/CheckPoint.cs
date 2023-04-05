using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public CarController carController;
    public Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        renderer.material.color = Color.cyan;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (carController.lastCheckPoint != null) carController.lastCheckPoint.UnColour();
            carController.lastCheckPoint = this;
            renderer.material.color = Color.blue;
        }
    }

    public void UnColour()
    {
        renderer.material.color = Color.cyan;
    }
}
