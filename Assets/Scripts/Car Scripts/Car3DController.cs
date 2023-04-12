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
    private float currentBreakForce;
    private float currentSteerAngle;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteeringAngle;

    [SerializeField] private float frontSpinForce, sideSpinForce, shiftSpinForce;
    private InputAction move, rotate, swing, jump, fireHook, breaking, reset, rotateMod, grapplingLengthControl;

    [SerializeField] private Camera mainCamera;
    [NonSerialized] public bool firstPerson = false;

    private void OnEnable()
    {
        move = InputHandler.playerInput.Player3D.Move;
        move.Enable();
        rotate = InputHandler.playerInput.Player3D.Rotate;
        rotate.Enable();
        rotateMod = InputHandler.playerInput.Player3D.RotateMod;
        rotateMod.Enable();
        swing = InputHandler.playerInput.Player3D.Swing;
        swing.Enable();
        grapplingLengthControl = InputHandler.playerInput.Player3D.GrappleLengthControl;
        grapplingLengthControl.Enable();
        breaking = InputHandler.playerInput.Player3D.Break;
        breaking.Enable();
        fireHook = InputHandler.playerInput.LevelInteraction.FireHook;
        fireHook.Enable();
        reset = InputHandler.playerInput.LevelInteraction.Reset;
        reset.Enable();
        reset.performed += Reset;
        jump = InputHandler.playerInput.Player3D.Jump;
        jump.Enable();
        jump.performed += DoJump;
    }

    private void OnDisable()
    {
        jump.performed -= DoJump;
        reset.performed -= Reset;
    }

    private void Start()
    {
        stationaryTolerance = 0.0005f;
        rigidBody = GetComponent<Rigidbody>();
        startpoint = transform.position;
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        HandleMotor();
        if (groundedWheels != 0)
        {
            HandleSteering();
            UpdateWheels();
        }
        AirRotate();
        if (grappling) Swing();
        CustomGravity();
    }

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

    private void HandleMotor()
    {
        float driveDir = move.ReadValue<Vector2>().y;
        float rpm = 0;
        int motorAmount = 0;
        foreach (AxleInfo axle in axleInfos)
        {
            if (axle.motor)
            {
                axle.leftWheel.motorTorque = driveDir * motorForce;
                axle.rightWheel.motorTorque = driveDir * motorForce;
                rpm += axle.leftWheel.rpm + axle.rightWheel.rpm;
                motorAmount++;
            }
        }
        rpm /= motorAmount;
        currentBreakForce = (breaking.IsPressed() || (driveDir > 0 && rpm < -1) || (driveDir < 0 && rpm > 1)) ? breakForce : (driveDir == 0 ? breakForce / 10000 : 0);
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        foreach (AxleInfo axle in axleInfos)
        {
            axle.leftWheel.brakeTorque = currentBreakForce;
            axle.rightWheel.brakeTorque = currentBreakForce;
        }
    }

    private void HandleSteering ()
    {
        float steerDir = move.ReadValue<Vector2>().x;
        currentSteerAngle = maxSteeringAngle * steerDir;
        foreach (AxleInfo axle in axleInfos)
        {
            if (axle.turnType == AxleInfo.TurnType.Normal)
            {
                axle.leftWheel.steerAngle = currentSteerAngle;
                axle.rightWheel.steerAngle = currentSteerAngle;
            }
            else if (axle.turnType == AxleInfo.TurnType.Inverted)
            {
                axle.leftWheel.steerAngle = - currentSteerAngle;
                axle.rightWheel.steerAngle = - currentSteerAngle;
            }
        }
    }
    private void UpdateWheels()
    {
        foreach (AxleInfo axle in axleInfos)
        {

            UpdateSingleWheel(axle.leftWheel, axle.leftTransform);
            UpdateSingleWheel(axle.rightWheel, axle.rightTransform);
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
}
