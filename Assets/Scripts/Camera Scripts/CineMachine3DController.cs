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
    public CinemachineVirtualCamera hoodCam;
    private CinemachineBrain brain;
    private Car3DController carController;
    private InputAction cameraSwitch;
    bool frameSinceSwitch = false;
    private enum CamState { 
        follow,
        hood
    }
    private CamState camState = CamState.follow;

    private void Awake()
    {
        carController = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Car3DController>();
        brain = GetComponent<CinemachineBrain>();
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

    // Update is called once per frame

    private void FixedUpdate()
    {
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
        //if (groundCam.enabled && groundCam.m_XAxis.Value != 0) groundCam.m_XAxis.Value = Mathf.Lerp(groundCam.m_XAxis.Value, 0, 0.1f);
        //if (groundCam.enabled && groundCam.m_YAxis.Value != 0.5f) groundCam.m_YAxis.Value = Mathf.Lerp(groundCam.m_YAxis.Value, 0.5f, 0.1f);
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
        frameSinceSwitch = true;
        hoodCam.enabled = false;
        groundCam.enabled = false;
        groundCam.m_Transitions.m_InheritPosition = false;
        airCam.m_Transitions.m_InheritPosition = false;
        airCam.enabled = false;
        if (camState == CamState.follow) {
            camState = CamState.hood;
            hoodCam.enabled = true;
            carController.firstPerson = true;
        }
        else if (camState == CamState.hood) {
            camState = CamState.follow;
            if (carController.groundedWheels == 4) airCam.enabled = true;
            else groundCam.enabled = true;
            carController.firstPerson = false;
            groundCam.m_XAxis.Value = 0;
        }
    }
}
