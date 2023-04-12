using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
// Class from Unity documentation, with slight modification (due to no need for steering):
// https://docs.unity3d.com/Manual/WheelColliderTutorial.html

public class Car2DController : CarController
{
    // list of axle infos, including wheel colliders
    public int maxTorque; // maximum torque
    private InputAction move, swing, rotate, fireHook, breaking, reset, jump;
    [NonSerialized] public bool firstPerson;

    private float torque; // current torque

    private void OnEnable()
    {
        move = InputHandler.playerInput.Player2D.Move;
        move.Enable();
        swing = InputHandler.playerInput.Player2D.Swing;
        swing.Enable();
        rotate = InputHandler.playerInput.Player2D.Rotate;
        rotate.Enable();
        breaking = InputHandler.playerInput.Player2D.Break;
        breaking.Enable();
        fireHook = InputHandler.playerInput.LevelInteraction.FireHook;
        fireHook.Enable();
        reset = InputHandler.playerInput.LevelInteraction.Reset;
        reset.Enable();
        reset.performed += Reset;
        jump = InputHandler.playerInput.Player2D.Jump;
        jump.Enable();
        jump.performed += DoJump;
    }

    private void OnDisable()
    {
        reset.performed -= Reset;
        jump.performed -= DoJump;
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        stationaryTolerance = 0.0005f;
        startpoint = transform.position;
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        CustomGravity();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection = move.ReadValue<Vector2>();
        Vector2 swingDirection = swing.ReadValue<Vector2>();
        Vector2 rotateDirection = rotate.ReadValue<Vector2>();
        if (grappling)
        {
            // swing car back and forth (horizontal input, same as rotating)
            Vector3 dirVector = rigidBody.transform.up * -1;
            if (!firstPerson) rigidBody.AddForce(dirVector * swingDirection.x * swingForce * Time.deltaTime);
            else rigidBody.AddForce(dirVector * swingDirection.y * swingForce * Time.deltaTime);

            grapplingGun.ChangeLength(moveDirection.y);
            return;
        }

        // reset car position and rotation when R is pressed
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
        // if direction is changed: brake and then accelerate
        torque = maxTorque * (2 * moveDirection.y);

         // should car brake?

        // drive forward or backwards based on input
        foreach (AxleInfo aInfo in axleInfos)
        {
            if (aInfo.motor)
            {
                // if car should brake, set the brake torque
                if (breaking.IsPressed())
                {
                    aInfo.leftWheel.brakeTorque = maxTorque * 10;
                    aInfo.rightWheel.brakeTorque = maxTorque * 10;
                }
                // if not braking, set brake torque to 0 and motor torque to current torque
                else
                {
                    // reset input axes so we don't go too fast after breaking
                    if (breaking.IsPressed()) Input.ResetInputAxes();

                    aInfo.leftWheel.brakeTorque = 0;
                    aInfo.rightWheel.brakeTorque = 0;

                    aInfo.leftWheel.motorTorque = torque;
                    aInfo.rightWheel.motorTorque = torque;
                }
            }
        }

        // rotate car via horizontal movement inputs (along x-axis)
        if (!firstPerson) rigidBody.AddTorque(Vector3.right * maxRotationTorque * rotateDirection.x * Time.deltaTime);
        else rigidBody.AddTorque(Vector3.right * maxRotationTorque * rotateDirection.y * Time.deltaTime);

        // if player car gets stuck on its back, you can flip it back up
        if (rigidBody.velocity.sqrMagnitude < stationaryTolerance * stationaryTolerance
            && rigidBody.transform.up.y <= 10e-5 && !grappling)
        {
            // reset torque of wheels so you don't drive off immediately after bouncing back up
            foreach (AxleInfo aInfo in axleInfos)
            {
                if (aInfo.motor)
                {
                    aInfo.leftWheel.motorTorque = 0;
                    aInfo.rightWheel.motorTorque = 0;
                }
            }

            // apply explosion force and rotation to car to get it back up
            rigidBody.AddExplosionForce(100000 * Time.deltaTime * 180, rigidBody.transform.position, 5, 5);
            rigidBody.AddTorque(Vector3.right * maxRotationTorque * 100);
        }
    }
}
