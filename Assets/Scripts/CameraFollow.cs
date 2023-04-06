using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxZoom, minZoom, zoomSpeed;
    [SerializeField] private CarController carController;
    private InputAction zoom;

    private void Start()
    {
        zoom = carController.playerControls.Player2D.Zoom;
        zoom.Enable();
    }

    private void FixedUpdate()
    {
        HandleTranslation();
        HandleRotation();
    }
    private void Update()
    {
        HandleZoom();
    }


    private void HandleTranslation()
    {
        var targetPosition = target.TransformPoint(offset);
        //transform.position = Vector3.SmoothDamp(transform.position, targetPosition);
        transform.position = Vector3.Lerp(transform.position, targetPosition, translateSpeed * Time.deltaTime);
    }
    private void HandleRotation()
    {
        var direction = target.position - transform.position;
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
    private void HandleZoom()
    {
        float x = offset.x - zoom.ReadValue<Vector2>().y * zoomSpeed;
        offset.x = (x > maxZoom ? maxZoom : (x < minZoom ? minZoom : x));
    }
}