using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialMessage
{
    public String message = "This is a tutorial message";
    public Color color = Color.white;
    public float scale = 1.0f;
    public float extraYSpacing = 0.0f;
}

[RequireComponent(typeof(BoxCollider))]
public class TutorialTextTrigger : MonoBehaviour
{
    public bool followsPlayer;
    public bool displayOnlyOnce;
    
    public TutorialMessage[] messages;

    public float ySpacing = 2.0f;
    public float xOffset = 0.0f;

    private List<PopupText> _activatedPT = new List<PopupText>();

    private bool _triggered;
    private Transform _playerTransform;
    
    private void OnTriggerEnter(Collider other)
    {
        if (_triggered)
        {
            return;
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Car"))
        {
            _triggered = true;
            _playerTransform = other.transform;
            DisplayTutorialMessages();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_triggered)
        {
            return;
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Car"))
        {
            HideTutorialMessage();
            _triggered = false;
            if (displayOnlyOnce)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void DisplayTutorialMessages()
    {
        Vector3 position = transform.position;
        position.x += xOffset;
        foreach (var m in messages)
        {
            position.y += m.extraYSpacing;  // allow more flexible control
            _activatedPT.Add(followsPlayer
                ? PopupTextGenerator.Instance.Generate(m.message, m.color, position, m.scale, _playerTransform)
                : PopupTextGenerator.Instance.Generate(m.message, m.color, position, m.scale));
            position.y += ySpacing; // vertical offset
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
