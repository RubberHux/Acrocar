using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera3DFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Car3DController carController;

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
        if (carController.groundedWheels == 4)
        {
            //var direction = target.position - transform.position;
            var direction = target.forward;
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation =  Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }
    }
}
