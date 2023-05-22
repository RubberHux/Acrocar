using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Goal : MonoBehaviour
{
    UIController uiController;
    
    void Start()
    {
        uiController = GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>();
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<CarController>().moveSound.Stop();
            uiController.SetWin();
        }
    }
}
