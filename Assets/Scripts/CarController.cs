using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
// Class from Unity documentation, with slight modification (due to no need for steering):
// https://docs.unity3d.com/Manual/WheelColliderTutorial.html


public abstract class CarController : MonoBehaviour
{
    public bool grappling;
    public float respawnTime = 1;
    public float respawnTimer = 0;
    public bool respawned = false;
    [NonSerialized] public CheckPoint lastCheckPoint = null;
    public PlayerInputActions playerControls;

    public int maxRotationTorque; // maximum rotation torque
    public int swingForce; // the force with which to swing when grappled
    public int grappleBoostForce;
    public float maxGrappleDist;
    internal Vector3 startpoint;

    internal Rigidbody rigidBody; // rigid body of the car


    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }

    // Update is called once per frame
    void Update()
    {
        // Make sure that the car doesn't drive off when 
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
    }

    private void Reset(InputAction.CallbackContext context)
    {
        Respawn();
    }

    internal abstract void Respawn();

    public void Kill()
    {
        Respawn();
    }

    public void GrappleBoost(Vector3 target)
    {
        rigidBody.AddForce(Vector3.Normalize(target - this.transform.position) * grappleBoostForce);
    }
}
