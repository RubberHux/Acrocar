using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public GrapplingGun grapplingGun;


    public int maxRotationTorque; // maximum rotation torque
    public int swingForce; // the force with which to swing when grappled
    public int grappleBoostForce;
    public float maxGrappleDist;

    private float torque; // current torque
    private Rigidbody rigidBody; // rigid body of the car

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (grappling)
        {
            // swing car back and forth (horizontal input, same as rotating)
            Vector3 dirVector = rigidBody.transform.up * -1;
            rigidBody.AddForce(dirVector * Input.GetAxisRaw("Horizontal") * swingForce);

            // retract or extend grappling hook (vertical input, same as driving)
            SpringJoint joint = GetComponent<SpringJoint>();
            joint.maxDistance -= Input.GetAxisRaw("Vertical") * 0.2f;
            if (joint.maxDistance > maxGrappleDist) joint.maxDistance = maxGrappleDist;

            return;
        }

        // reset car position and rotation when R is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
            return;
        }
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
        float movDir = Input.GetAxis("Vertical");
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
                    if (Input.GetKeyUp(KeyCode.Space)) Input.ResetInputAxes();

                    aInfo.leftWheel.brakeTorque = 0;
                    aInfo.rightWheel.brakeTorque = 0;

                    aInfo.leftWheel.motorTorque = torque;
                    aInfo.rightWheel.motorTorque = torque;
                }
            }
        }

        // rotate car via horizontal movement inputs (along x-axis)
        rigidBody.AddTorque(Vector3.right * maxRotationTorque * Input.GetAxisRaw("Horizontal"));

        
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
