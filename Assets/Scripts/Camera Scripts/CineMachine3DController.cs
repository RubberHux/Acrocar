using Cinemachine;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class CineMachine3DController : MonoBehaviour
{
    public CinemachineFreeLook groundCam;
    public CinemachineFreeLook airCam;
    public CinemachineVirtualCamera hoodCam;
    private CinemachineBrain brain;
    private Car3DController carController;
    private InputAction cameraSwitch;
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
    void Update()
    {
        if (camState == CamState.follow) { 
            if (carController.groundedWheels == 4)
            {
                groundCam.enabled = true;
                airCam.enabled = false;
            }
            else
            {
                groundCam.enabled = false;
                airCam.enabled = true;
            }
        }
    }
    private void FixedUpdate()
    {
        if (groundCam.enabled && groundCam.m_XAxis.Value != 0) groundCam.m_XAxis.Value = Mathf.Lerp(groundCam.m_XAxis.Value, 0, 0.1f);
        if (groundCam.enabled && groundCam.m_YAxis.Value != 0.5f) groundCam.m_YAxis.Value = Mathf.Lerp(groundCam.m_YAxis.Value, 0.5f, 0.1f);
    }

    private void SwitchCam(InputAction.CallbackContext context)
    {
        Debug.Log("Yo da yo!");
        hoodCam.enabled = false;
        groundCam.enabled = false;
        airCam.enabled = false;
        if (camState == CamState.follow) { 
            camState = CamState.hood;
            hoodCam.enabled = true;
        }
        else if (camState == CamState.hood) {
            camState = CamState.follow;
            if (carController.groundedWheels == 4) airCam.enabled = true;
            else groundCam.enabled = true;
        }
    }
}
