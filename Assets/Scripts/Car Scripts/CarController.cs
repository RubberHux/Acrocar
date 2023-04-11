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
    public Transform leftTransform, rightTransform;
    public bool motor; // is this wheel attached to motor?
    public TurnType turnType;
    public enum TurnType
    {
        None,
        Normal,
        Inverted,
    }
}
public abstract class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    [NonSerialized] public bool grappling;
    public float respawnTime = 1;
    public float respawnTimer = 0;
    [NonSerialized] public bool respawned = false;
    [NonSerialized] public CheckPoint lastCheckPoint = null;
    [SerializeField] internal Transform[] roadCheckers;

    public int maxRotationTorque; // maximum rotation torque
    public int swingForce; // the force with which to swing when grappled
    public int grappleBoostForce;
    internal Vector3 startpoint;
    internal GrapplingGun grapplingGun;
    internal float stationaryTolerance;
    internal Rigidbody rigidBody; // rigid body of the car
    public Vector3? gravity = null;
    public LayerMask gravRoadLayer;
    public LayerMask notCarLayers;

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

    public void SetCustomGravity(Vector3? gravityDirection)
    {
        gravity = gravityDirection;
        if (gravity == null) rigidBody.useGravity = true;
        else rigidBody.useGravity = false;
    }

    internal void customGravity()
    {
        if (gravity != null)
        {
            rigidBody.AddForce((Vector3)gravity * rigidBody.mass);
        }
    }

    internal void CheckGravRoad()
    {
        foreach (Transform roadChecker in roadCheckers) { 
            
        }
    }

    public void GrappleBoost(Vector3 target)
    {
        rigidBody.AddForce(Vector3.Normalize(target - this.transform.position) * grappleBoostForce);
    }
}
