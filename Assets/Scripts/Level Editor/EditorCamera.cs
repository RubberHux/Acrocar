using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCamera : MonoBehaviour
{
    public GameObject editorCamera; // camera for level editor (separate from when playing)
    public float mouseSensitivity; // sensitivity for panning via drag

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Vector3 newCamPos = editorCamera.transform.position;
            newCamPos.y -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            newCamPos.z -= Input.GetAxis("Mouse X") * mouseSensitivity;

            editorCamera.transform.SetPositionAndRotation(newCamPos, editorCamera.transform.rotation);
        }
    }
}
