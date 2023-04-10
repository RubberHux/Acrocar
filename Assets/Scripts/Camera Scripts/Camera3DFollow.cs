using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Camera3DFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;
    [SerializeField] private float translateSpeed, rotationSpeed, lookSpeedX, lookSpeedY;
    [SerializeField] private float lookYMin, lookYMax;
    private float currentLookX, currentLookY;
    [SerializeField] private Car3DController carController;
    private InputAction look;

    private void Start()
    {
        look = InputHandler.playerInput.Player3D.Look;
        look.Enable();
        carController = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Car3DController>();
    }

    private void FixedUpdate()
    {
        HandleTranslation();
        HandleRotation();
    }

    private void HandleTranslation()
    {
        var targetPosition = target.TransformPoint(offset);
        //transform.position = Vector3.SmoothDamp(transform.position, targetPosition);
        transform.position = Vector3.Lerp(transform.position, targetPosition, translateSpeed * Time.deltaTime);
    }
    private void HandleRotation()
    {
        Vector2 lookVal = look.ReadValue<Vector2>();
        currentLookY = currentLookY - lookVal.y * Time.deltaTime * lookSpeedY;
        Mathf.Clamp(currentLookY, lookYMax, lookYMin);
        currentLookX = currentLookX + lookVal.x * Time.deltaTime * lookSpeedX;
        Quaternion rotationOffset = Quaternion.LookRotation(Vector3.up, new Vector3(currentLookY, currentLookX, currentLookY));
        if (carController.groundedWheels == 4)
        {
            var direction = target.forward;
            var rotation = Quaternion.LookRotation(direction, Vector3.up);

            var transformedRotation = new Quaternion(rotation.x + rotationOffset.x, rotation.y + rotationOffset.y, rotation.z + rotationOffset.z, rotation.w + rotationOffset.w);
            transform.rotation = Quaternion.Lerp(transform.rotation, transformedRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
