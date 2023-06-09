using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CineMachine3DController : MonoBehaviour
{
    public CinemachineFreeLook groundCam;
    public float groundTimer;
    public CinemachineFreeLook airCam;
    private List<CinemachineVirtualCamera> customCams;
    public CinemachineVirtualCamera sideCam2D;
    private CarController carController;
    private InputAction cameraSwitch;
    [SerializeField] GameObject customWorldUp;
    CinemachineBrain brain;
    int customCamIndex;
    bool frameSinceSwitch = false;
    [SerializeField] private float maxZoom, minZoom, zoomSpeed;
    private enum CamState { 
        follow,
        side,
        custom
    }
    private CamState camState = CamState.follow;
    bool carFound;

    private void Start()
    {
        brain = GetComponent<CinemachineBrain>();
    }

    public void SetCar(CarController car, int camLayer, LayerMask cullingMask, bool p1, PlayerInput playerInput)
    {
        //Set the car
        carController = car;
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
        //Set the hood camera
        
        customCams = car.GetComponentsInChildren<CinemachineVirtualCamera>().ToList();
        customCams.ForEach(x => x.enabled = false);

        Transform trackPoint = car.GetComponentInChildren<CameraTrackPoint>().transform;
        airCam.Follow = trackPoint;
        airCam.LookAt = trackPoint;
        groundCam.Follow = trackPoint;
        groundCam.LookAt = trackPoint;
        sideCam2D.Follow = trackPoint;
        InputActionReference LookRef = ScriptableObject.CreateInstance<InputActionReference>();
        LookRef.Set(playerInput.currentActionMap.FindAction("Look"));
        LookRef.action.Enable();
        
        airCam.gameObject.GetComponent<CinemachineInputProvider>().XYAxis = LookRef;
        groundCam.gameObject.GetComponent<CinemachineInputProvider>().XYAxis = LookRef;

        Camera cam = GetComponent<Camera>();
        if (!p1) cam.GetComponent<AudioListener>().enabled = false;

        cam.cullingMask = cullingMask;
        airCam.gameObject.layer = camLayer;
        groundCam.gameObject.layer = camLayer;
        sideCam2D.gameObject.layer = camLayer;
        customCams.ForEach(x => x.gameObject.layer = camLayer);

        carFound = true;
    }

    private void FixedUpdate()
    {
        if (carController.customGravity != Vector3.zero && carController.localCustomGravity != Vector3.zero) customWorldUp.transform.up = Vector3.up;
        if (!carFound) return;
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
                else
                {
                    groundCam.m_BindingMode = CinemachineTransposer.BindingMode.LockToTarget;
                    if (carController.customGravity == Vector3.zero || carController.localCustomGravity == Vector3.zero) customWorldUp.transform.up = carController.transform.up;
                }
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
            //groundCam.m_YAxisRecentering.m_enabled = true;
            groundCam.m_RecenterToTargetHeading.m_enabled = true;
        }
    }

    public void SwitchCam()
    {
        if (camState == CamState.follow || camState == CamState.side)
        {
            customCamIndex = 0;
            SetState(CamState.custom);
        }
        else if (camState == CamState.custom && customCamIndex < customCams.Count - 1)
        {
            customCamIndex++;
            SetState(CamState.custom);
        }
        else if (camState == CamState.custom && !carController.is2D) SetState(CamState.follow);
        else if (camState == CamState.custom) SetState(CamState.side);
    }
    public void Zoom(float val)
    {
        var offset = sideCam2D.GetCinemachineComponent<CinemachineTransposer>();
        float x = offset.m_FollowOffset.x - val * zoomSpeed;
        offset.m_FollowOffset.x = (x > maxZoom ? maxZoom : (x < minZoom ? minZoom : x));
    }
    public void DimensionSwitch()
    {
        if (carController.is2D) SetState(CamState.side);
        else if (camState == CamState.side) SetState(CamState.follow);
    }
    
    private void SetState(CamState state)
    {
        bool firstPerson = false;
        frameSinceSwitch = true;
        customCams.ForEach(x => x.enabled = false);
        groundCam.enabled = false;
        sideCam2D.enabled = false;
        groundCam.m_Transitions.m_InheritPosition = false;
        airCam.m_Transitions.m_InheritPosition = false;
        airCam.enabled = false;
        camState = state;
        if (camState == CamState.custom)
        {
            customCams[customCamIndex].enabled = true;
            firstPerson = true;
        }
        else if (camState == CamState.follow)
        {
            if (carController.groundedWheels != 4) airCam.enabled = true;
            else groundCam.enabled = true;
            groundCam.m_XAxis.Value = 0;
        }
        else if (camState == CamState.side)
        {
            sideCam2D.enabled = true;
        }
        carController.firstPerson = firstPerson;
    }
}
