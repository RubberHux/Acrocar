using Cinemachine;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class CineMachine2DController : MonoBehaviour
{
    public CinemachineVirtualCamera followCam;
    public CinemachineVirtualCamera hoodCam;
    private InputAction cameraSwitch;
    private Car2DController carController;

    private enum CamState { 
        follow,
        hood
    }
    private CamState camState = CamState.follow;

    private void Awake()
    {
        carController = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Car2DController>();
        
    }

    private void OnEnable()
    {
        cameraSwitch = InputHandler.playerInput.Player2D.CameraSwitch;
        cameraSwitch.Enable();
        cameraSwitch.performed += SwitchCam;
    }

    private void OnDisable()
    {
        cameraSwitch.performed -= SwitchCam;
    }

    private void SwitchCam(InputAction.CallbackContext context)
    {
        hoodCam.enabled = false;
        followCam.enabled = false;
        if (camState == CamState.follow) { 
            camState = CamState.hood;
            hoodCam.enabled = true;
            carController.firstPerson = true;
        }
        else if (camState == CamState.hood) {
            camState = CamState.follow;
            followCam.enabled = true;
            carController.firstPerson = false;
        }
    }
}
