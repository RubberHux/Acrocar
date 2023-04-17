using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class PopupText : MonoBehaviour
{
    private TextMesh _textMesh;
    private Camera _lookingCamera;
    private Animator _animator;
    
    private static readonly int Hide = Animator.StringToHash("Hide");

    private void OnEnable()
    {
        _textMesh = gameObject.GetComponent<TextMesh>();
        _animator = gameObject.GetComponent<Animator>();
        _animator.SetBool(Hide, false);
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

    public async Task HideText()
    {
        _animator.SetBool(Hide, true);
        await Task.Delay(1000);
    }

    private void Update()
    {
        // text always face camera
        transform.forward = _lookingCamera.transform.forward;
    }
}