using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GrapplingGun : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public LayerMask notCarLayers;
    public Transform gunTip, player;
    public float maxGrappleDistance;
    private SpringJoint joint;
    public CarController carController;
    private float aimPreTimer = -1, aimPostTimer = -1, grappleBoostTimer = 0;
    private bool aiming = false;
    public float aimLeniencyPreTime, aimLeniencyPostTime, grappleBoostTime;
    private InputAction fireHook;
    enum GrappleType
    {
        Swing,
        Boost,
    }
    GrappleType grappleType;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fireHook == null) fireHook = carController.playerControls.LevelInteraction.FireHook;
        if (carController.respawned)
        {
            if (joint) StopGrapple();
            return;
        }
        if (!joint) Aim();
        if (fireHook.WasPressedThisFrame()) aimPreTimer = aimLeniencyPreTime;
        if (!joint && ((aimPreTimer >= 0 && aiming) || aimPostTimer >= 0) && fireHook.IsPressed()) StartGrapple();
        else if (fireHook.WasReleasedThisFrame()) StopGrapple();
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
        float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

        if (distanceFromPoint > maxGrappleDistance) return;

        if (grappleType == GrappleType.Boost)
        {
            Debug.Log(grappleBoostTimer);
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

            joint.anchor = new Vector3(0, 0, 1);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 1000f;
            joint.damper = 2000f;
            joint.massScale = 1000f;
        }

        lr.startColor = lr.endColor = Color.black;
        lr.positionCount = 2;
        aiming = false;
    }

    void Aim()
    {
        RaycastHit hit;
        if (Physics.Raycast(gunTip.position, gunTip.forward, out hit, maxGrappleDistance, notCarLayers) && (whatIsGrappleable == (whatIsGrappleable | (1 << hit.transform.gameObject.layer))))
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
        aimPreTimer = aimPostTimer = -1;
        carController.grappling = false;
        lr.positionCount = 0;
        Destroy(joint);
    }
}
