using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EditorCamera : MonoBehaviour
{
    public CineMachine3DController camController;
    public float mouseSensitivity; // sensitivity for panning via drag

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Vector3 newCamPos = transform.position;
            newCamPos.y -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            newCamPos.z -= Input.GetAxis("Mouse X") * mouseSensitivity;

            transform.SetPositionAndRotation(newCamPos, transform.rotation);
        }

        camController.Zoom(1);
    }

    public void Zoom(InputAction.CallbackContext context)
    {
        //Allows PlayerInput to zoom
        camController.Zoom(context.ReadValue<Vector2>().y);
    }
}
