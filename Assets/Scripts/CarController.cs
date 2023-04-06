using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
// Class from Unity documentation, with slight modification (due to no need for steering):
// https://docs.unity3d.com/Manual/WheelColliderTutorial.html
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
}

public class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // list of axle infos, including wheel colliders
    public int maxTorque; // maximum torque
    public bool grappling;
    public float respawnTime = 1;
    public float respawnTimer = 0;
    public bool respawned = false;
    public CheckPoint lastCheckPoint;
    public PlayerInputActions playerControls;
    private InputAction move, fireHook, breaking, reset;


    public int maxRotationTorque; // maximum rotation torque
    public int swingForce; // the force with which to swing when grappled
    public int grappleBoostForce;
    public float maxGrappleDist;

    private float torque; // current torque
    private Rigidbody rigidBody; // rigid body of the car
    private float stationaryTolerance;

    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }

    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();
        breaking = playerControls.Player.Break;
        breaking.Enable();
        fireHook = playerControls.Player.FireHook;
        fireHook.Enable();
        reset = playerControls.Player.Reset;
        reset.Enable();
        reset.performed += Reset;
    }

    private void OnDisable()
    {
        move.Disable();
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        stationaryTolerance = 0.0005f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection = move.ReadValue<Vector2>();
        if (grappling)
        {
            // swing car back and forth (horizontal input, same as rotating)
            Vector3 dirVector = rigidBody.transform.up * -1;
            rigidBody.AddForce(dirVector * moveDirection.x * swingForce * Time.deltaTime * 60);

            // retract or extend grappling hook (vertical input, same as driving)
            SpringJoint joint = GetComponent<SpringJoint>();
            joint.maxDistance -= Input.GetAxisRaw("Vertical") * 0.1f * Time.deltaTime * 60;
            if (joint.maxDistance > maxGrappleDist) joint.maxDistance = maxGrappleDist;

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
        float movDir = moveDirection.y;
        // if direction is changed: brake and then accelerate
        torque = maxTorque * (2 * movDir);

        bool brake = Input.GetKey(KeyCode.Space); // should car brake?

        // drive forward or backwards based on input
        foreach (AxleInfo aInfo in axleInfos)
        {
            if (aInfo.motor)
            {
                // if car should brake, set the brake torque
                if (brake)
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
        rigidBody.AddTorque(Vector3.right * maxRotationTorque * Input.GetAxisRaw("Horizontal") * 300 * Time.deltaTime);

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

    private void Reset(InputAction.CallbackContext context)
    {
        Respawn();
    }

    private void Respawn()
    {
        if (lastCheckPoint == null) rigidBody.MovePosition(new Vector3(0, 1, 0));
        else
        {
            Vector3 checkpointPosition = lastCheckPoint.gameObject.transform.position;
            rigidBody.MovePosition(new Vector3(checkpointPosition.x, checkpointPosition.y + 1, checkpointPosition.z));
        }

        rigidBody.MoveRotation(new Quaternion(0, 0, 0, 0).normalized);
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        foreach (AxleInfo aInfo in axleInfos)
        {
            aInfo.leftWheel.brakeTorque = float.MaxValue;
            aInfo.rightWheel.brakeTorque = float.MaxValue;

            aInfo.leftWheel.motorTorque = 0;
            aInfo.rightWheel.motorTorque = 0;
        }
        respawnTimer = 0.1f;
        respawned = true;
        grappling = false;
    }

    public void Kill()
    {
        Respawn();
    }

    public void GrappleBoost(Vector3 target)
    {
        rigidBody.AddForce(Vector3.Normalize(target - this.transform.position) * grappleBoostForce);
    }
}
