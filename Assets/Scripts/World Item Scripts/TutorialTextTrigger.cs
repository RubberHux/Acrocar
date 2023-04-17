using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialMessage
{
    public String message = "This is a tutorial message";
    public Color color = Color.white;
    public int scale = 1;
}

[RequireComponent(typeof(BoxCollider))]
public class TutorialTextTrigger : MonoBehaviour
{
    
    public TutorialMessage[] messages;

    private List<PopupText> _activatedPT = new List<PopupText>();

    private bool _triggered;
    
    private void OnTriggerEnter(Collider other)
    {
        if (_triggered)
        {
            return;
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Car"))
        {
            DisplayTutorialMessages();
            _triggered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Car"))
        {
            HideTutorialMessage();
            _triggered = false;
        }
    }

    private void DisplayTutorialMessages()
    {
        foreach (var m in messages)
        {
            _activatedPT.Add(PopupTextGenerator.Instance.Generate(m.message, m.color, transform, m.scale));
        }
    }

    private void HideTutorialMessage()
    {
        foreach (var pt in _activatedPT)
        {
            PopupTextGenerator.Instance.HidePopupText(pt);
        }
    }
    
}
