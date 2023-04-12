using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingGun : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    private Transform grappledObject; // object which the hook is attached to
    private Rigidbody grappledRigidBody; // rigidbody which hook is attached to
    private Vector3 grapplePointDiff; // difference between object pos and grapple point
    public LayerMask whatIsGrappleable;
    public LayerMask notCarLayers;
    public Transform gunTip, player;
    public float maxGrappleDistance;
    private SpringJoint joint;
    private CarController carController;
    private float aimPreTimer = -1, aimPostTimer = -1, grappleBoostTimer = 0;
    private bool aiming = false;
    public float aimLeniencyPreTime, aimLeniencyPostTime, grappleBoostTime;
    private InputAction fireHook, aim;
    public float maxJointDist, minJointDist;
    private Camera cam;

    enum GrappleType
    {
        Swing,
        Boost,
    }
    GrappleType grappleType;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        carController = GetComponent<CarController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        fireHook = InputHandler.playerInput.LevelInteraction.FireHook;
        fireHook.Enable();
        aim = InputHandler.playerInput.Player2D.Aim;
        aim.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (carController.respawned)
        {
            if (joint) StopGrapple();
            return;
        }
        if (!joint && Time.timeScale != 0) Aim();
        if (fireHook.WasPressedThisFrame()) aimPreTimer = aimLeniencyPreTime;
        if (!joint && ((aimPreTimer >= 0 && aiming) || aimPostTimer >= 0) && fireHook.IsPressed()) StartGrapple();
        else if (fireHook.WasReleasedThisFrame()) StopGrapple();

        if (joint && grappledRigidBody != null)
        {
            // make sure to move grapple point if grappled object moves
            grapplePoint = grappledRigidBody.position;
            Debug.Log(grapplePoint);
        }

        if (aimPreTimer >= 0) aimPreTimer -= Time.deltaTime;
        if (aimPostTimer >= 0) aimPostTimer -= Time.deltaTime;
        if (grappleBoostTimer >= 0) grappleBoostTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
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
            joint.connectedAnchor = grapplePoint;
            joint.connectedBody = grappledRigidBody;

            joint.anchor = gunTip.localPosition;

            joint.maxDistance = distanceFromPoint * maxJointDist;
            joint.minDistance = distanceFromPoint * minJointDist;

            if (grappledRigidBody != null)
            {
                joint.spring = 1000f;
                joint.damper = 10000f;
                joint.massScale = 1000f;
                joint.connectedMassScale = 1000f;
                joint.tolerance = 1f;

                joint.maxDistance = maxGrappleDistance;
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

    void Aim()
    {
        RaycastHit hit;
        
        Vector3 rayDirection = gunTip.forward;
        if (SettingsHandler.easyAim && carController.GetType() == typeof(Car2DController))
        {
            if (InputHandler.currentScheme <= 2) rayDirection = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.x)) - gunTip.position;
            else
            {
                Vector2 aimDir = aim.ReadValue<Vector2>();
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
            if (distanceFromPoint < maxGrappleDistance * 0.8f) lr.startColor = lr.endColor = Color.yellow;
            else if (distanceFromPoint < maxGrappleDistance * 0.9f) lr.startColor = lr.endColor = new Color(1, 0.5f, 0);
            else lr.startColor = lr.endColor = Color.red;
            aiming = true;
            lr.positionCount = 2;
            aimPostTimer = aimLeniencyPostTime;

            grappledObject = hit.transform;
            grapplePointDiff = hit.transform.position - grapplePoint;
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
    }

    public void StopGrapple()
    {
        carController.grappling = false;
        aimPreTimer = aimPostTimer = -1;
        carController.grappling = false;
        lr.positionCount = 0;
        Destroy(joint);
        grappledRigidBody = null;
    }

    public void ChangeLength(float direction)
    {
        float newDistance = joint.maxDistance - direction * 1f * Time.deltaTime * 60;
        if (newDistance > maxGrappleDistance) joint.maxDistance = maxGrappleDistance;
        else joint.maxDistance = newDistance;
    }
}
