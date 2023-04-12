using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

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
public class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    [NonSerialized] public int groundedWheels = 0;
    [NonSerialized] public bool grappling;
    public float gravCheckDistance;
    [NonSerialized]  public float respawnTimer = 0;
    [NonSerialized] public bool respawned = false;
    [NonSerialized] public CheckPoint lastCheckPoint = null;
    [SerializeField] internal Transform[] roadCheckers;
    CineMachine3DController camController;
    private float currentBreakForce;
    private float currentSteerAngle;

    internal InputAction move, swing, rotate, rotateMod, breaking, reset, jump, grapplingLengthControl, dimensionSwitch;
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
    }

    private void SetConstraints()
    {
        if (is2D) rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        else rigidBody.constraints = RigidbodyConstraints.None;
    }

    private void OnEnable()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        camController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CineMachine3DController>();
        SetInput();
    }

    private void OnDisable()
    {
        reset.performed -= Reset;
        jump.performed -= Jump;
        dimensionSwitch.performed -= DoDimensionSwitch;
    }

    private void SetInput()
    {
        if (reset == null)
        {
            dimensionSwitch = InputHandler.playerInput.Debug.DimensionSwitch;
            dimensionSwitch.Enable();
            dimensionSwitch.performed += DoDimensionSwitch;
            reset = InputHandler.playerInput.LevelInteraction.Reset;
            reset.Enable();
            reset.performed += Reset;
        }
        if (is2D)
        {
            move = InputHandler.playerInput.Player2D.Move;
            move.Enable();
            swing = InputHandler.playerInput.Player2D.Swing;
            swing.Enable();
            grapplingLengthControl = InputHandler.playerInput.Player2D.GrappleLengthControl;
            grapplingLengthControl.Enable();
            rotate = InputHandler.playerInput.Player2D.Rotate;
            rotate.Enable();
            breaking = InputHandler.playerInput.Player2D.Break;
            breaking.Enable();
            jump = InputHandler.playerInput.Player3D.Jump;
            jump.Enable();
            jump.performed += Jump;
        }
        else
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
            if (jump != null) jump.performed -= Jump;
            jump = InputHandler.playerInput.Player3D.Jump;
            jump.Enable();
            jump.performed += Jump;
        }
    }

    private void Update()
    {
        if (grappling) grapplingGun.ChangeLength(grapplingLengthControl.ReadValue<Vector2>().y);
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
            rigidBody.AddTorque(rigidBody.transform.right * frontSpinForce * 100);
        }
    }

    private void FixedUpdate()
    {
        if (respawned) return;
        GetInput();
        CheckGrounded();
        HandleMotor();
        if (!is2D && groundedWheels != 0)
        {
            HandleSteering();
            UpdateWheels();
        }
        AirRotate();
        if (grappling) Swing();
        CustomGravity();
    }

    void GetInput()
    {
        if (is2D)
        {
            moveDir = new Vector2(0, firstPerson ? move.ReadValue<Vector2>().y : move.ReadValue<Vector2>().y);
            rotateDir = new Vector2(0, firstPerson ? rotate.ReadValue<Vector2>().y : rotate.ReadValue<Vector2>().x);
            swingDir = new Vector2(0, (firstPerson ? swing.ReadValue<Vector2>().y : swing.ReadValue<Vector2>().x));
        }
        else
        {
            moveDir = move.ReadValue<Vector2>();
            rotateDir = rotate.ReadValue<Vector2>();
            swingDir = swing.ReadValue<Vector2>();
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
            if (axle.motor)
            {
                axle.leftWheel.motorTorque = driveDir * motorForce;
                axle.rightWheel.motorTorque = driveDir * motorForce;
                rpm += axle.leftWheel.rpm + axle.rightWheel.rpm;
                motorAmount++;
            }
        }
        rpm /= motorAmount;
        currentBreakForce = (breaking.IsPressed() || (driveDir > 0 && rpm < -1) || (driveDir < 0 && rpm > 1)) ? breakForce : (driveDir == 0 ? breakForce / 10 : 0);
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

    internal void Jump(InputAction.CallbackContext context)
    {
        if (groundedWheels == 4 && Time.timeScale != 0.0f) rigidBody.AddForce(rigidBody.transform.up * 700000);
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
        if (gravity != null)
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

    internal void Reset(InputAction.CallbackContext context)
    {
        Respawn();
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
            if (rotateMod.IsPressed()) rigidBody.AddTorque(-rigidBody.transform.forward * shiftSpinForce * rotateDir.x);
            else rigidBody.AddTorque(rigidBody.transform.up * sideSpinForce * rotateDir.x);
        }
    }

    private void Swing()
    {
        rigidBody.AddForce((firstPerson ? -mainCamera.transform.up : mainCamera.transform.forward) * swingDir.y * swingForce * Time.deltaTime);
        rigidBody.AddForce(mainCamera.transform.right * swingDir.x * swingForce * Time.deltaTime);
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
        SetInput();
        SetConstraints();
        rigidBody.angularVelocity = Vector3.zero;
        if (is2D)
        {
            rigidBody.MoveRotation(new Quaternion(0, 0, 0, 0).normalized);
        }
        camController.DimensionSwitch();
    }
}
