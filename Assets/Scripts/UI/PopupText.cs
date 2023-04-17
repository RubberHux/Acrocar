using System;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class PopupText : MonoBehaviour
{
    private TextMesh _textMesh;
    private Camera _lookingCamera;
    private void OnEnable()
    {
        _textMesh = gameObject.GetComponent<TextMesh>();
        _lookingCamera = Camera.current;
    }

    public void SetText(String textInput)
    {
        _textMesh.text = textInput;
    }

    public void SetColor(Color colorInput)
    {
        _textMesh.color = colorInput;
    }

    public void SetScale(int scale)
    {
        _textMesh.fontSize *= scale;
    }

    public void SetPosition(Vector3 position)
    {
        this.gameObject.transform.position = position;
    }

    private void Update()
    {
        // text always face camera
        transform.forward = _lookingCamera.transform.forward;
    }
}