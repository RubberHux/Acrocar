using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [NonSerialized] public Vector3 leftOrigin, rightOrigin;
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
    [SerializeField] Transform centerOfMass;
    [NonSerialized] public float respawnTimer = 0;
    [NonSerialized] public bool respawned = false;
    [NonSerialized] public CheckPoint lastCheckPoint = null;
    [SerializeField] internal Transform[] roadCheckers;
    CineMachine3DController camController;
    private float currentBreakForce;
    private float currentSteerAngle;
    public int playerIndex = 0;
    public bool isAlone = true;
    bool breaking, rotateMod;
    internal InputAction dimensionSwitch;
    public int swingForce; // the force with which to swing when grappled
    public int grappleBoostForce;
    public int jumpForce;
    [NonSerialized] public Vector3 startPoint;
    [NonSerialized] public Quaternion startRot;
    internal GrapplingGun grapplingGun;
    internal float stationaryTolerance;
    internal Rigidbody rigidBody; // rigid body of the car
    public Vector3? gravity = null;
    public Vector2 rotateDir, moveDir, swingDir;
    public LayerMask gravRoadLayer;
    public LayerMask notCarLayers;
    PlayerInput playerInput;
    [NonSerialized] public float gravRoadPercent;
    [NonSerialized] public bool is2D;
    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteeringAngle;
    [SerializeField] private float frontSpinForce, sideSpinForce, shiftSpinForce;
    float xPos;

    private Camera mainCamera;
    [NonSerialized] public bool firstPerson = false;
    float jumpTimer = 0;
    float jumpTime = 0.1f;

    private void Awake()
    {
        grapplingGun = GetComponent<GrapplingGun>();
    }

    void OnEnable()
    {
        rigidBody = GetComponent<Rigidbody>();
        if (centerOfMass != null) rigidBody.centerOfMass = centerOfMass.position;

        firstPerson = false;
        stationaryTolerance = 0.001f;
        playerInput = GetComponent<PlayerInput>();
        
        SetConstraints();
        dimensionSwitch = InputHandler.playerInput.Debug.DimensionSwitch;
        dimensionSwitch.Enable();
        dimensionSwitch.performed += DoDimensionSwitch;
        SetActionMap();
        foreach(AxleInfo axle in axleInfos)
        {
            axle.leftOrigin = axle.leftTransform.localPosition;
            axle.rightOrigin = axle.rightTransform.localPosition;
        }
    }

    private void Start()
    {
        startPoint = transform.localPosition;
        startRot = transform.localRotation;
    }

    private void SetConstraints()
    {
        if (is2D)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            xPos = rigidBody.position.x;
        }
        else rigidBody.constraints = RigidbodyConstraints.None;
    }

    public void SetCam(GameObject cameras, int playNum)
    {
        playerIndex = playNum;
        mainCamera = cameras.GetComponentInChildren<Camera>();
        camController = cameras.GetComponentInChildren<CineMachine3DController>();
    }

    private void OnDisable()
    {
        if (dimensionSwitch != null)
        dimensionSwitch.performed -= DoDimensionSwitch;
    }

    private void FixedUpdate()
    {
        UpdateDirections();
        if (camController == null) return; 
        if (respawned)
        {
            RespawnLock();
            return;
        }
        CheckGrounded();
        HandleMotor();
        if (!is2D && groundedWheels != 0) HandleSteering();
        UpdateWheels();
        AirRotate();
        if (grappling) Swing();
        CustomGravity();
        FlipCar();
        UpdateTimers();
        ConstraintsFix();
    }

    void ConstraintsFix()
    {
        //Unity's physics engine is a bit broken and let's the car drift out of its constraints. This fixes that by resetting the constrained axes.
        if (is2D)
        {
            rigidBody.angularVelocity = new Vector3(rigidBody.angularVelocity.x, 0, 0);
            rigidBody.rotation = new Quaternion(rigidBody.rotation.x, 0, 0, rigidBody.rotation.w).normalized;
            rigidBody.position = new Vector3(xPos, rigidBody.position.y, rigidBody.position.z);
        }
    }

    void UpdateTimers()
    {
        //Updates Timers
        jumpTimer -= Time.deltaTime;
        respawnTimer -= Time.deltaTime;
    }

    void RespawnLock()
    {
        //Makes sure that the momentum of the car is removed on a respawn
        if (respawned)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                respawned = false;
                rigidBody.isKinematic = false;
            }
            else return;
        }
    }

    void FlipCar()
    {
        //Flips the car over if it has landed on its back/side.
        if (respawnTimer < -1 && rigidBody.velocity.sqrMagnitude < stationaryTolerance * stationaryTolerance
            && groundedWheels != 4 && !grappling)
        {
            rigidBody.AddForce(rigidBody.transform.up * 200000 * Time.deltaTime * 180);
            rigidBody.AddTorque(rigidBody.transform.right * frontSpinForce * 100);
        }
    }

    public void UpdateDirections()
    {
        //Gets the value for movement from PlayerInput
        //Vector2 move = context.ReadValue<Vector2>();
        Vector2 move = playerInput.actions["Move"].ReadValue<Vector2>();
        if (is2D) moveDir = new Vector2(0, move.y);
        else moveDir = move;
    
        //Gets the value for rotation from PlayerInput
        Vector2 val = playerInput.actions["Rotate"].ReadValue<Vector2>();
        if (is2D) rotateDir = new Vector2(0, firstPerson ? val.y : val.x);
        else rotateDir = val;
    
        //Gets the value for swinging from PlayerInput
        val = playerInput.actions["Swing"].ReadValue<Vector2>();
        if (is2D) swingDir = new Vector2(0, firstPerson ? val.y : val.x);
        else swingDir = val;
    }

    public void SetBreak(InputAction.CallbackContext context)
    {
        //Gets the value for breaking from PlayerInput
        if (context.phase == InputActionPhase.Started) breaking = true;
        else if (context.phase == InputActionPhase.Canceled) breaking = false;
    }

    public void SetRotateMod(InputAction.CallbackContext context)
    {
        //Gets the value for rotateMod from PlayerInput
        if (context.phase == InputActionPhase.Started) rotateMod = true;
        else if (context.phase == InputActionPhase.Canceled) rotateMod = false;
    }

    public void ChangeCam(InputAction.CallbackContext context)
    {
        //Allows PlayerInput to change the camera
        if (context.phase == InputActionPhase.Started && camController != null) camController.SwitchCam();
    }

    public void Zoom(InputAction.CallbackContext context)
    {
        //Allows PlayerInput to zoom
        if (is2D && camController != null) camController.Zoom(context.ReadValue<Vector2>().y);
    }

    void SetActionMap()
    {
        //Changes between using the controls for 2D or 3D when dimensions change
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
        // Updates the wheel transforms to match with the wheel colliders
        foreach (AxleInfo axle in axleInfos)
        {
            UpdateSingleWheel(axle.leftWheel, axle.leftTransform, axle.leftOrigin);
            UpdateSingleWheel(axle.rightWheel, axle.rightTransform, axle.rightOrigin);
        }
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform, Vector3 origin)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        if (wheelCollider.isGrounded) wheelTransform.position = pos;
        else wheelTransform.localPosition = Vector3.Lerp(wheelTransform.localPosition, origin, 0.999f);
        wheelTransform.rotation = rot;
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
        if (lastCheckPoint == null)
        {
            rigidBody.MovePosition(startPoint);
            rigidBody.MoveRotation(startRot);
        }
        else
        {
            rigidBody.MovePosition(lastCheckPoint.gameObject.transform.position);
            rigidBody.MoveRotation(lastCheckPoint.gameObject.transform.rotation);
        }
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        foreach (AxleInfo aInfo in axleInfos)
        {
            aInfo.leftWheel.brakeTorque = float.MaxValue;
            aInfo.rightWheel.brakeTorque = float.MaxValue;

            aInfo.leftWheel.motorTorque = 0;
            aInfo.rightWheel.motorTorque = 0;
        }
        xPos = rigidBody.position.x;
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
        rigidBody.AddForce((firstPerson ? -mainCamera.transform.up : mainCamera.transform.forward) * swingDir.y * swingForce * Time.deltaTime);
        rigidBody.AddForce(mainCamera.transform.right * swingDir.x * swingForce * Time.deltaTime);
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
