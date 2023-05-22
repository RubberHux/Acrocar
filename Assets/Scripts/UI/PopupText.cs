using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class PopupText : MonoBehaviour
{
    private TextMesh _textMesh;
    private Camera _lookingCamera;
    private Animator _animator;
    
    private float _minDistance = 2.0f;
    private float _maxDistance = 15.0f;

    private bool _shouldFollowPlayer;
    private Transform _playerTransform;
    
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
    }

    private void Start()
    {
        transform.forward = new Vector3(1, 0, 0);
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

    public void SetFollowTarget(Transform trans)
    {
        _shouldFollowPlayer = true;
        _playerTransform = trans;
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
        _lookingCamera = Camera.main;
        transform.forward = _lookingCamera.transform.forward;

        if (_shouldFollowPlayer)
        {
            Vector3 curPosition = transform.position;
            transform.position = new Vector3(curPosition.x, curPosition.y, _playerTransform.position.z);
        }
    }
}