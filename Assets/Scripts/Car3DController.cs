using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car3DController : MonoBehaviour
{
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private Rigidbody rigidBody;
    private float horizontalInput;
    private float verticalInput;
    private bool isBreaking, jump;
    private float currentBreakForce;
    private float currentSteerAngle;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteeringAngle;

    [SerializeField] private float frontSpinForce, sideSpinForce, shiftSpinForce;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider backLeftWheelCollider;
    [SerializeField] private WheelCollider backRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform backLeftWheelTransform;
    [SerializeField] private Transform backRightWheelTransform;

    public int groundedWheels = 0;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        GetInput();
        HandleMotor();
        if (groundedWheels != 0)
        {
            HandleSteering();
            UpdateWheels();
        }
        AirRotate();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Jump();
        if (Input.GetKeyDown(KeyCode.R)) Respawn();
    }

    private void CheckGrounded()
    {
        groundedWheels = 0;
        if (frontLeftWheelCollider.isGrounded) groundedWheels++;
        if (frontRightWheelCollider.isGrounded) groundedWheels++;
        if (backLeftWheelCollider.isGrounded) groundedWheels++;
        if (backRightWheelCollider.isGrounded) groundedWheels++;
    }


    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;

        currentBreakForce = isBreaking ? breakForce : (verticalInput == 0 ? breakForce / 10 : 0);
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        backLeftWheelCollider.brakeTorque = currentBreakForce;
        backRightWheelCollider.brakeTorque = currentBreakForce;
    }

    private void HandleSteering ()
    {
        currentSteerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }
    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(backLeftWheelCollider, backLeftWheelTransform);
        UpdateSingleWheel(backRightWheelCollider, backRightWheelTransform);
    }

    private void AirRotate()
    {
        if (groundedWheels != 0) return;

        rigidBody.AddTorque(rigidBody.transform.right * frontSpinForce * verticalInput);
        if (!Input.GetKey(KeyCode.LeftShift)) rigidBody.AddTorque(rigidBody.transform.up * sideSpinForce * horizontalInput);
        else rigidBody.AddTorque(rigidBody.transform.forward * shiftSpinForce * horizontalInput);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void Jump()
    {
        if (groundedWheels == 4) rigidBody.AddForce(Vector3.up * 700000);
    }

    private void Respawn()
    {
        rigidBody.MovePosition(new Vector3(0, 1, 0));
        rigidBody.MoveRotation(new Quaternion(0, 0, 0, 0).normalized);
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
    }
}
