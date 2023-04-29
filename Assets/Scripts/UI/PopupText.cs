using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class PopupText : MonoBehaviour
{
    private TextMesh _textMesh;
    private Camera _lookingCamera;
    private Animator _animator;
    
    private float _minDistance = 2.0f;
    private float _maxDistance = 15.0f;

    private bool PopupAnimPlayed
    {
        get
        {
            return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f;
        }
    }
    
    private static readonly int Hide = Animator.StringToHash("Hide");

    private void OnEnable()
    {
        _textMesh = gameObject.GetComponent<TextMesh>();
        _animator = gameObject.GetComponent<Animator>();
        _animator.SetBool(Hide, false);
        _lookingCamera = Camera.current;
    }

    private void Start()
    {
        transform.forward = _lookingCamera.transform.forward;
    }

    public void SetText(String textInput)
    {
        _textMesh.text = textInput;
    }

    public void SetColor(Color colorInput)
    {
        _textMesh.color = colorInput;
    }

    public void SetScale(float scale)
    {
        _textMesh.fontSize = Mathf.FloorToInt(_textMesh.fontSize * scale);
    }

    public void SetPosition(Vector3 position)
    {
        this.gameObject.transform.position = position;
    }

    public async Task HideText()
    {
        _animator.SetBool(Hide, true);
        await Task.Delay(1000);
        _textMesh.fontSize = 15;    // resize the font size back to default...
    }

    private void AdjustSizeByDistance()
    {
        float disToCam = Vector3.Distance(transform.position, _lookingCamera.transform.position);
        float t = Mathf.Max((disToCam - _minDistance) / _maxDistance, 0);
        transform.localScale = Vector3.one * Mathf.Lerp(0.0f, 1.0f, t);
    }
    
    private void LateUpdate()
    {
        // text always face camera
        transform.forward = _lookingCamera.transform.forward;
        
        // Trying to adjust size by distance in 3D mode, Not working great currently
        //if (PopupAnimPlayed)
        //{
        //    AdjustSizeByDistance();
        //}
    }
}