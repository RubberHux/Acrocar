using Cinemachine;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class CineMachine3DController : MonoBehaviour
{
    public CinemachineFreeLook groundCam;
    public CinemachineFreeLook airCam;
    private CinemachineBrain brain;
    private Car3DController carController;

    private void Awake()
    {
        carController = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Car3DController>();
        brain = GetComponent<CinemachineBrain>();
    }

    // Update is called once per frame
    void Update()
    {
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
    private void FixedUpdate()
    {
        if (groundCam.enabled && groundCam.m_XAxis.Value != 0) groundCam.m_XAxis.Value = Mathf.Lerp(groundCam.m_XAxis.Value, 0, 0.1f);
    }
}
