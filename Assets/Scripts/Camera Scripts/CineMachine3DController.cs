using Cinemachine;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

public class CineMachine3DController : MonoBehaviour
{
    public CinemachineFreeLook groundCam;
    public float groundTimer;
    public CinemachineFreeLook airCam;
    private CinemachineVirtualCamera hoodCam;
    public CinemachineVirtualCamera sideCam2D;
    private CarController carController;
    private InputAction cameraSwitch;
    bool frameSinceSwitch = false;
    [SerializeField] private float maxZoom, minZoom, zoomSpeed;
    private InputAction zoom;
    private enum CamState { 
        follow,
        side,
        hood
    }
    private CamState camState = CamState.follow;
    bool carFound;

    private void Awake()
    {
        zoom = InputHandler.playerInput.Player2D.Zoom;
        zoom.Enable();
        FindCar();
    }

    private void OnEnable()
    {
        cameraSwitch = InputHandler.playerInput.Player3D.CameraSwitch;
        cameraSwitch.Enable();
        cameraSwitch.performed += SwitchCam;
    }

    private void OnDisable()
    {
        cameraSwitch.performed -= SwitchCam;
    }

    private void FindCar()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            carController = GameObject.FindGameObjectWithTag("Player").GetComponent<CarController>();
            if (carController.is2D)
            {
                camState = CamState.side;
                groundCam.enabled = false;
                airCam.enabled = false;
            }
            else
            {
                camState = CamState.follow;
                sideCam2D.enabled = false;
            }
        }
        if (GameObject.FindGameObjectWithTag("HoodCam") != null)
        {
            hoodCam = GameObject.FindGameObjectWithTag("HoodCam").GetComponent<CinemachineVirtualCamera>();
            hoodCam.enabled = false;
        }
        if (GameObject.FindGameObjectWithTag("CameraTrackPoint") != null)
        {
            Transform trackPoint = GameObject.FindGameObjectWithTag("CameraTrackPoint").transform;
            airCam.Follow = trackPoint;
            airCam.LookAt = trackPoint;
            groundCam.Follow = trackPoint;
            groundCam.LookAt = trackPoint;
            Camera2DFollow.target = trackPoint;
            sideCam2D.Follow = trackPoint;
        }
        if (carController != null && hoodCam != null && airCam.Follow != null) carFound = true;
    }

    // Update is called once per frame

    private void Update()
    {
        HandleZoom();
    }

    private void FixedUpdate()
    {
        if (!carFound)
        {
            FindCar();
            return;
        }
        if (camState == CamState.follow)
        {
            if (!frameSinceSwitch)
            {
                groundCam.m_Transitions.m_InheritPosition = true;
                airCam.m_Transitions.m_InheritPosition = true;
            }
            if (carController.groundedWheels == 4)
            {
                if (carController.gravRoadPercent < 0.7f) groundCam.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
                else groundCam.m_BindingMode = CinemachineTransposer.BindingMode.LockToTarget;
                groundTimer += Time.deltaTime;
                groundCam.enabled = true;
                airCam.enabled = false;
            }
            else
            {
                groundTimer = 0;
                groundCam.enabled = false;
                airCam.enabled = true;
            }
        }
        frameSinceSwitch = false;
        Vector3 carVelocity = transform.InverseTransformDirection(carController.gameObject.GetComponent<Rigidbody>().velocity);
        carVelocity.y = 0;
        if (carVelocity.magnitude < 0.1 || groundTimer < 0.2f)
        {
            groundCam.m_YAxisRecentering.m_enabled = false;
            groundCam.m_RecenterToTargetHeading.m_enabled = false;
        }
        else
        {
            groundCam.m_YAxisRecentering.m_enabled = true;
            groundCam.m_RecenterToTargetHeading.m_enabled = true;
        }
    }
        
    private void SwitchCam(InputAction.CallbackContext context)
    {
        bool firstPerson = false;
        frameSinceSwitch = true;
        hoodCam.enabled = false;
        groundCam.enabled = false;
        sideCam2D.enabled = false;
        groundCam.m_Transitions.m_InheritPosition = false;
        airCam.m_Transitions.m_InheritPosition = false;
        airCam.enabled = false;
        if (camState == CamState.follow || camState == CamState.side) {
            camState = CamState.hood;
            hoodCam.enabled = true;
            firstPerson = true;
        }
        else if (camState == CamState.hood && !carController.is2D) {
            camState = CamState.follow;
            if (carController.groundedWheels == 4) airCam.enabled = true;
            else groundCam.enabled = true;
            groundCam.m_XAxis.Value = 0;
        }
        else if (camState == CamState.hood)
        {
            camState = CamState.side;
            sideCam2D.enabled = true;
        }
        carController.firstPerson = firstPerson;
    }
    private void HandleZoom()
    {
        var offset = sideCam2D.GetCinemachineComponent<CinemachineTransposer>();
        float x = offset.m_FollowOffset.x - zoom.ReadValue<Vector2>().y * zoomSpeed;
        offset.m_FollowOffset.x = (x > maxZoom ? maxZoom : (x < minZoom ? minZoom : x));
    }
}
