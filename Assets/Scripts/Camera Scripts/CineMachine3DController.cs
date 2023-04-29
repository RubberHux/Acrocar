using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private enum CamState { 
        follow,
        side,
        hood
    }
    private CamState camState = CamState.follow;
    bool carFound;

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
        
        hoodCam = car.GetComponentInChildren<CinemachineVirtualCamera>();
        hoodCam.enabled = false;

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
        hoodCam.gameObject.layer = camLayer;

        carFound = true;
    }

    private void FixedUpdate()
    {
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

    public void SwitchCam()
    {
        if (camState == CamState.follow || camState == CamState.side) SetState(CamState.hood);
        else if (camState == CamState.hood && !carController.is2D) SetState(CamState.follow);
        else if (camState == CamState.hood) SetState(CamState.side);
    }
    public void Zoom(float val)
    {
        Debug.Log("hello???");
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
        hoodCam.enabled = false;
        groundCam.enabled = false;
        sideCam2D.enabled = false;
        groundCam.m_Transitions.m_InheritPosition = false;
        airCam.m_Transitions.m_InheritPosition = false;
        airCam.enabled = false;
        camState = state;
        if (camState == CamState.hood)
        {
            hoodCam.enabled = true;
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
