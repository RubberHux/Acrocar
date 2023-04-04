using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxZoom, minZoom, zoomSpeed;

    private void FixedUpdate()
    {
        HandleTranslation();
        HandleRotation();
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
        float x = transform.position.x + Input.mouseScrollDelta.y * zoomSpeed;
        Debug.Log(x);
        transform.position = new Vector3((x > maxZoom ? maxZoom : (x < minZoom ? minZoom : x)), transform.position.y, transform.position.z);
    }
}