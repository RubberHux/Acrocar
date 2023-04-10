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
    [NonSerialized] public bool grappling;
    public float respawnTime = 1;
    public float respawnTimer = 0;
    [NonSerialized] public bool respawned = false;
    [NonSerialized] public CheckPoint lastCheckPoint = null;

    public int maxRotationTorque; // maximum rotation torque
    public int swingForce; // the force with which to swing when grappled
    public int grappleBoostForce;
    internal Vector3 startpoint;
    internal GrapplingGun grapplingGun;
    internal float stationaryTolerance;
    internal Rigidbody rigidBody; // rigid body of the car


    private void Awake()
    {
        grapplingGun = GetComponent<GrapplingGun>();
    }

    // Update is called once per frame

    internal void Reset(InputAction.CallbackContext context)
    {
        Respawn();
    }

    internal void DoJump(InputAction.CallbackContext context)
    {
        Jump();
    }

    internal abstract void Jump();

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