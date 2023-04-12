using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;

public class Car3DController : CarController
{
    /*
    private void Update()
    {
        if (grappling) grapplingGun.ChangeLength(InputHandler.playerInput.Player3D.GrappleLengthControl.ReadValue<Vector2>().y);
        if (respawned)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                respawned = false;
                respawnTimer = 0;
                rigidBody.isKinematic = false;
            }
            else return;
        }
        if (grappling) grapplingGun.ChangeLength(grapplingLengthControl.ReadValue<Vector2>().y);
        if (rigidBody.velocity.sqrMagnitude < stationaryTolerance * stationaryTolerance
            && rigidBody.transform.up.y <= 10e-5 && !grappling)
        {
            rigidBody.AddForce(Vector3.up * 200000 * Time.deltaTime * 180);
            rigidBody.AddTorque(rigidBody.transform.right * maxRotationTorque * 100);
        }
    }

    private void AirRotate()
    {
        if (groundedWheels != 0 || grappling) return;

        Vector2 Rotate = rotate.ReadValue<Vector2>();

        //Front swinging
        rigidBody.AddTorque(rigidBody.transform.right * frontSpinForce * Rotate.y);

        //Side swinging
        if (rotateMod.IsPressed()) rigidBody.AddTorque(-rigidBody.transform.forward * shiftSpinForce * Rotate.x);
        else rigidBody.AddTorque(rigidBody.transform.up * sideSpinForce * Rotate.x);
    }

    private void Swing()
    {
        Vector2 swingDirection = swing.ReadValue<Vector2>();
        rigidBody.AddForce((firstPerson ? -mainCamera.transform.up : mainCamera.transform.forward) * swingDirection.y * swingForce * Time.deltaTime);
        rigidBody.AddForce(mainCamera.transform.right * swingDirection.x * swingForce * Time.deltaTime);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
    */
}
