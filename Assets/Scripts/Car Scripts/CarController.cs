using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using UnityEngine.Windows;

[System.Serializable]
// Class from Unity documentation, with slight modification (due to no need for steering):
// https://docs.unity3d.com/Manual/WheelColliderTutorial.html

public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public Transform leftTransform, rightTransform;
    public MotorType motorType; // is this wheel attached to motor?
    public enum MotorType
    {
        None,
        Only2D,
        Only3D,
        Both
    }
    public TurnType turnType;
    public enum TurnType
    {
        None,
        Normal,
        Inverted,
    }
}
public class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    [NonSerialized] public int groundedWheels = 0;
    [NonSerialized] public bool grappling;
    public float gravCheckDistance;
    [NonSerialized] public float respawnTimer = 0;
    [NonSerialized] public bool respawned = false;
    [NonSerialized] public CheckPoint lastCheckPoint = null;
    [SerializeField] internal Transform[] roadCheckers;
    CineMachine3DController camController;
    private float currentBreakForce;
    private float currentSteerAngle;
    float timeSinceCreation = 0;
    public int playerNumber = 0;
    public bool isAlone = true;
    bool breaking, rotateMod;
    internal InputAction dimensionSwitch;
    public int swingForce; // the force with which to swing when grappled
    public int grappleBoostForce;
    public int jumpForce;
    internal Vector3 startpoint;
    internal GrapplingGun grapplingGun;
    internal float stationaryTolerance;
    internal Rigidbody rigidBody; // rigid body of the car
    public Vector3? gravity = null;
    private Vector2 rotateDir, moveDir, swingDir;
    public LayerMask gravRoadLayer;
    public LayerMask notCarLayers;
    [NonSerialized] public float gravRoadPercent;
    [NonSerialized] public bool is2D;
    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteeringAngle;
    [SerializeField] private float frontSpinForce, sideSpinForce, shiftSpinForce;

    private Camera mainCamera;
    [NonSerialized] public bool firstPerson = false;
    float jumpTimer = 0;
    float jumpTime = 0.1f;

    private void Awake()
    {
        grapplingGun = GetComponent<GrapplingGun>();
    }

    private void Start()
    {
        firstPerson = false;
        stationaryTolerance = 0.0005f;
        rigidBody = GetComponent<Rigidbody>();
        startpoint = transform.position;
        SetConstraints();
        dimensionSwitch = InputHandler.playerInput.Debug.DimensionSwitch;
        dimensionSwitch.Enable();
        dimensionSwitch.performed += DoDimensionSwitch;
        SetActionMap();
    }

    private void SetConstraints()
    {
        if (is2D) rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        else rigidBody.constraints = RigidbodyConstraints.None;
    }

    public void SetCam(GameObject cameras, int playNum)
    {
        playerNumber = playNum;
        mainCamera = cameras.GetComponentInChildren<Camera>();
        camController = cameras.GetComponentInChildren<CineMachine3DController>();
    }

    private void OnDisable()
    {
        dimensionSwitch.performed -= DoDimensionSwitch;
    }

    private void FixedUpdate()
    {
        if (camController == null) return; 
        if (respawned)
        {
            RespawnLock();
            return;
        }
        CheckGrounded();
        HandleMotor();
        if (!is2D && groundedWheels != 0) HandleSteering();
        if (groundedWheels != 0) UpdateWheels();
        AirRotate();
        if (grappling) Swing();
        CustomGravity();
        FlipCar();
        UpdateTimers();
    }

    void UpdateTimers()
    {
        jumpTimer -= Time.fixedDeltaTime;
    }

    void RespawnLock()
    {
        if (respawned)
        {
            respawnTimer -= Time.fixedDeltaTime;
            if (respawnTimer <= 0)
            {
                respawned = false;
                respawnTimer = 0;
                rigidBody.isKinematic = false;
            }
            else return;
        }
    }

    void FlipCar()
    {
        if (timeSinceCreation > 1 && rigidBody.velocity.sqrMagnitude < stationaryTolerance * stationaryTolerance
            && groundedWheels != 4 && !grappling)
        {
            rigidBody.AddForce(rigidBody.transform.up * 200000 * Time.fixedDeltaTime * 180);
            rigidBody.AddTorque(rigidBody.transform.right * frontSpinForce * 100);
        }
        timeSinceCreation += Time.fixedDeltaTime;
    }

    public void UpdateMoveDir(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();
        if (is2D) moveDir = new Vector2(0, move.y);
        else moveDir = move;
    }

    public void UpdateRotateDir(InputAction.CallbackContext context)
    {
        Vector2 val = context.ReadValue<Vector2>();
        if (is2D) rotateDir = new Vector2(0, firstPerson ? val.y : val.x);
        else rotateDir = val;
    }

    public void UpdateSwingDir(InputAction.CallbackContext context)
    {
        Vector2 val = context.ReadValue<Vector2>();
        if (is2D) swingDir = new Vector2(0, firstPerson ? val.y : val.x);
        else swingDir = val;
    }

    public void SetBreak(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started) breaking = true;
        else if (context.phase == InputActionPhase.Canceled) breaking = false;
    }

    public void SetRotateMod(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started) rotateMod = true;
        else if (context.phase == InputActionPhase.Canceled) rotateMod = false;
    }

    public void ChangeCam(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started) camController.SwitchCam();
    }

    public void Zoom(InputAction.CallbackContext context)
    {
        if (is2D) camController.Zoom(context.ReadValue<Vector2>().y);
    }

    void SetActionMap()
    {
        if (is2D)
        {
            gameObject.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player2D");
            gameObject.GetComponent<PlayerInput>().defaultActionMap = "Player2D";
        }
        else
        {
            gameObject.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player3D");
            gameObject.GetComponent<PlayerInput>().defaultActionMap = "Player3D";
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

    private void HandleMotor()
    {
        float driveDir = moveDir.y;
        float rpm = 0;
        int motorAmount = 0;
        foreach (AxleInfo axle in axleInfos)
        {
            if (axle.motorType == AxleInfo.MotorType.Both || (is2D && axle.motorType == AxleInfo.MotorType.Only2D) || (!is2D && axle.motorType == AxleInfo.MotorType.Only3D))
            {
                axle.leftWheel.motorTorque = driveDir * motorForce;
                axle.rightWheel.motorTorque = driveDir * motorForce;
                rpm += axle.leftWheel.rpm + axle.rightWheel.rpm;
                motorAmount++;
            }
        }
        rpm /= motorAmount;
        currentBreakForce = (breaking || (driveDir > 0 && rpm < -1) || (driveDir < 0 && rpm > 1)) ? breakForce : (driveDir == 0 ? breakForce / 10 : 0);
        ApplyBreaking();
    }

    private void HandleSteering()
    {
        float steerDir = moveDir.x;
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
                axle.leftWheel.steerAngle = -currentSteerAngle;
                axle.rightWheel.steerAngle = -currentSteerAngle;
            }
        }
    }

    private void ApplyBreaking()
    {
        foreach (AxleInfo axle in axleInfos)
        {
            axle.leftWheel.brakeTorque = currentBreakForce;
            axle.rightWheel.brakeTorque = currentBreakForce;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && jumpTimer <= 0 && groundedWheels == 4 && Time.timeScale != 0.0f)
        {
            rigidBody.AddForce(rigidBody.transform.up * 700000);
            jumpTimer = jumpTime;
        }
    }

    public void SetCustomGravity(Vector3? gravityDirection)
    {
        gravity = gravityDirection;
    }

    internal void CustomGravity()
    {
        bool noGravChanges = true;
        CheckGravRoad();
        if (gravRoadPercent > 0.7)
        {
            rigidBody.AddForce(Physics.gravity.magnitude * rigidBody.mass * -transform.up);
            noGravChanges = false;
        }
        else if (gravity != null)
        {
            noGravChanges = false;
            rigidBody.AddForce((Vector3)gravity * rigidBody.mass);
        }
        if (noGravChanges) rigidBody.useGravity = true;
        else rigidBody.useGravity = false;
    }

    internal void CheckGravRoad()
    {
        gravRoadPercent = 0;
        int notGravRoadAmount = 0;
        RaycastHit hit;
        foreach (Transform roadChecker in roadCheckers) {
            if (Physics.Raycast(roadChecker.position, -roadChecker.transform.up, out hit, gravCheckDistance, notCarLayers) && (gravRoadLayer == (gravRoadLayer | (1 << hit.transform.gameObject.layer)))) {
                gravRoadPercent++;
            }
            else notGravRoadAmount++;
        }
        gravRoadPercent /= gravRoadPercent + notGravRoadAmount;
    }

    internal void CheckGrounded()
    {
        groundedWheels = 0;
        foreach (AxleInfo axle in axleInfos)
        {
            if (axle.leftWheel.isGrounded) groundedWheels++;
            if (axle.rightWheel.isGrounded) groundedWheels++;
        }
    }

    public void GrappleBoost(Vector3 target)
    {
        rigidBody.AddForce(Vector3.Normalize(target - this.transform.position) * grappleBoostForce);
    }

    public void Reset(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started) Respawn();
    }

    public void Kill()
    {
        Respawn();
    }
    
    internal void Respawn()
    {
        if (Time.timeScale == 0.0f) return;
        if (lastCheckPoint == null) rigidBody.MovePosition(startpoint);
        else
        {
            Vector3 checkpointPosition = lastCheckPoint.gameObject.transform.position;
            rigidBody.MovePosition(new Vector3(checkpointPosition.x, checkpointPosition.y, checkpointPosition.z));
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

    private void AirRotate()
    {
        if (groundedWheels != 0 || grappling) return;

        //Front rotate
        rigidBody.AddTorque(rigidBody.transform.right * frontSpinForce * rotateDir.y);

        //Side rotate
        if (!is2D)
        {
            if (rotateMod) rigidBody.AddTorque(-rigidBody.transform.forward * shiftSpinForce * rotateDir.x);
            else rigidBody.AddTorque(rigidBody.transform.up * sideSpinForce * rotateDir.x);
        }
    }

    private void Swing()
    {
        rigidBody.AddForce((firstPerson ? -mainCamera.transform.up : mainCamera.transform.forward) * swingDir.y * swingForce * Time.fixedDeltaTime);
        rigidBody.AddForce(mainCamera.transform.right * swingDir.x * swingForce * Time.fixedDeltaTime);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void DoDimensionSwitch(InputAction.CallbackContext context)
    {
        DimensionSwitch(!is2D);
    }

    public void DimensionSwitch(bool to2D)
    {
        is2D = to2D;
        SetActionMap();
        SetConstraints();
        rigidBody.angularVelocity = Vector3.zero;
        if (is2D)
        {
            rigidBody.MoveRotation(new Quaternion(0, 0, 0, 0).normalized);
        }
        camController.DimensionSwitch();
    }
}
