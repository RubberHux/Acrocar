using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingGun : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    private Rigidbody grappledRigidBody; // rigidbody which hook is attached to
    public LayerMask whatIsGrappleable;
    public LayerMask notCarLayers;
    GameObject grapplePointObj, grappledObj;
    public Transform gunTip, player;
    public float maxGrappleDistance;
    public List<GameObject> hookParts;
    private SpringJoint joint;
    private CarController carController;
    private float aimPreTimer = -1, aimPostTimer = -1, grappleBoostTimer = 0;
    private bool aiming = false;
    public float aimLeniencyPreTime, aimLeniencyPostTime, grappleBoostTime;
    public float maxJointDist, minJointDist;
    private Camera cam;
    public float grapplingChange;
    public float grapplingChangeSpeed;
    bool fireHook = false;
    Vector2 aim;

    enum GrappleType
    {
        Swing,
        Boost,
    }
    GrappleType grappleType;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount= 0;
        carController = GetComponent<CarController>();
    }

    public void SetCam(GameObject cameras)
    {
        cam = cameras.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (joint && !joint.autoConfigureConnectedAnchor) joint.connectedAnchor = grapplePointObj.transform.position;
        if (cam == null) return;
        if (carController.respawned)
        {
            if (joint) StopGrapple();
            return;
        }
        if (!joint && Time.timeScale != 0) Aim();

        if (!joint && ((aimPreTimer >= 0 && aiming) || aimPostTimer >= 0) && fireHook) StartGrapple();
        if (joint) ChangeLength(grapplingChange);


        if (aimPreTimer >= 0) aimPreTimer -= Time.deltaTime;
        if (aimPostTimer >= 0) aimPostTimer -= Time.deltaTime;
        if (grappleBoostTimer >= 0) grappleBoostTimer -= Time.deltaTime;

        if (joint && grappledRigidBody != null)
        {
            // make sure to move grapple point if grappled object moves
            grapplePoint = grappledRigidBody.transform.position;
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        if (Time.timeScale == 0.0f) return;
        float distanceFromPoint = Vector3.Distance(gunTip.position, grapplePoint);

        if (distanceFromPoint > maxGrappleDistance) return;

        if (grappleType == GrappleType.Boost)
        {
            if (grappleBoostTimer <= 0)
            {
                carController.GrappleBoost(grapplePoint);
                grappleBoostTimer = grappleBoostTime;
            }
        }
        else
        {
            carController.grappling = true;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;

            if (grapplePointObj != null) Destroy(grapplePointObj);
            grapplePointObj = Instantiate(new GameObject(), grappledObj.transform);
            grapplePointObj.transform.position = grapplePoint;

            joint.connectedAnchor = grapplePointObj.transform.position;
            joint.connectedBody = grappledRigidBody;
            hookParts.ForEach(part => part.SetActive(false));
            joint.anchor = gunTip.localPosition;

            joint.maxDistance = distanceFromPoint * maxJointDist;
            joint.minDistance = distanceFromPoint * minJointDist;

            if (grappledObj.gameObject.CompareTag("MovableGrappleBlock"))
            {
                joint.spring = 1000f;
                joint.damper = 10000f;
                joint.massScale = 1000f;
                joint.connectedMassScale = 1000f;
                joint.tolerance = 1f;

                joint.autoConfigureConnectedAnchor = true;
            }
            else
            {
                joint.spring = 1000f;
                joint.damper = 2000f;
                joint.massScale = 1000f;
            }
        }

        lr.startColor = lr.endColor = Color.black;
        lr.positionCount = 2;
        aiming = false;
    }

    public void UpdateAim(InputAction.CallbackContext context) => aim = context.ReadValue<Vector2>();
    public void SetGrapplingChange(InputAction.CallbackContext context) => grapplingChange = context.ReadValue<Vector2>().y;

    public void SetFireHook(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            aimPreTimer = aimLeniencyPreTime;
            fireHook = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            fireHook = false;
            StopGrapple();
        }
    }

    void Aim()
    {
        RaycastHit hit;
        
        Vector3 rayDirection = gunTip.forward;
        if (SettingsHandler.easyAim && carController.GetType() == typeof(Car2DController))
        {
            if (InputHandler.currentDevice <= 2) rayDirection = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.x)) - gunTip.position;
            else
            {
                Vector2 aimDir = aim;
                rayDirection = new Vector3(transform.position.x, aimDir.y, aimDir.x);
            }
        }
        if (Physics.Raycast(gunTip.position, rayDirection, out hit, maxGrappleDistance, notCarLayers) && (whatIsGrappleable == (whatIsGrappleable | (1 << hit.transform.gameObject.layer))))
        {
            if (hit.transform.CompareTag("GrappleBoost"))
            {
                grappleType = GrappleType.Boost;
                grapplePoint = hit.transform.position;
            }
            else {
                grappleType = GrappleType.Swing;
                grapplePoint = hit.point;
            }
            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
            Color lrColor = Color.yellow;
            if (distanceFromPoint > maxGrappleDistance * 0.9f) lrColor = Color.red; 
            else if (distanceFromPoint > maxGrappleDistance * 0.8f) lrColor = new Color(1, 0.5f, 0);
            lrColor.a = 0.5f;
            lr.startColor = lr.endColor = lrColor;
            aiming = true;
            lr.positionCount = 2;
            aimPostTimer = aimLeniencyPostTime;

            grappledObj = hit.transform.gameObject;
            grappledRigidBody = hit.rigidbody;
        }
        else
        {
            aiming = false;
            lr.positionCount = 0;
        }
    }

    void DrawRope()
    {
        if (lr.positionCount == 0) return;

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
        if (joint && !joint.autoConfigureConnectedAnchor) 
            lr.SetPosition(1, grapplePointObj.transform.position);
    }

    public void StopGrapple()
    {
        if (Time.timeScale == 0.0f) return;
        carController.grappling = false;
        aimPreTimer = aimPostTimer = -1;
        carController.grappling = false;
        lr.positionCount = 0;
        Destroy(joint);
        grappledRigidBody = null;
        hookParts.ForEach(part => part.SetActive(true));
    }

    public void ChangeLength(float direction)
    {
        float newDistance = joint.maxDistance - direction * grapplingChangeSpeed * Time.deltaTime * 60;
        if (newDistance > maxGrappleDistance) joint.maxDistance = maxGrappleDistance;
        else joint.maxDistance = newDistance;
    }
}
