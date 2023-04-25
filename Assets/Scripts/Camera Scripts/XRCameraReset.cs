using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class XRCameraReset : MonoBehaviour
{
    [SerializeField] GameObject rotationOffset, offset;
    bool moved = false;
    InputAction resetAction;

    // Start is called before the first frame update
    void OnEnable()
    {
        resetAction = InputHandler.playerInput.XRSpecific.XRReset;
        resetAction.Enable();
        resetAction.performed += DoResetOffset;
    }

    private void OnDisable()
    {
        resetAction.performed -= DoResetOffset;
    }

    // Update is called once per frame
    void Update()
    {
        if (moved) return;
        if (gameObject.transform.position != Vector3.zero) ResetOffset();
    }

    void DoResetOffset(InputAction.CallbackContext context)
    {
        ResetOffset();
    }

    private void ResetOffset()
    {
        rotationOffset.transform.localRotation = Quaternion.Euler(0, -transform.localRotation.eulerAngles.y, 0);
        offset.transform.localPosition = -transform.localPosition;
        moved = true;
    }
}
