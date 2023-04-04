using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;
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
    private float aimPreTimer, aimPostTimer;
    private bool aiming = false;
    public float aimLeniencyPreTime, aimLeniencyPostTime;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (carController.respawned)
        {
            if (joint) StopGrapple();
            return;
        }
        if (!joint) Aim();
        if (Input.GetMouseButtonDown(0)) aimPreTimer = aimLeniencyPreTime;
        if (!joint && ((aimPreTimer >= 0 && aiming) || aimPostTimer >= 0) && Input.GetMouseButton(0)) StartGrapple();
        else if (Input.GetMouseButtonUp(0)) StopGrapple();
        if (aimPreTimer >= 0) aimPreTimer -= Time.deltaTime;
        if (aimPostTimer >= 0) aimPostTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

        if (distanceFromPoint > maxGrappleDistance) return;

        carController.grappling = true;
        
        lr.startColor = lr.endColor = Color.black;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;

        joint.anchor = new Vector3(0, 0, 1);

        

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = 1000f;
        joint.damper = 2000f;
        joint.massScale = 1000f;

        lr.positionCount = 2;

        aiming = false;
    }

    void Aim()
    {
        RaycastHit hit;
        if (Physics.Raycast(gunTip.position, gunTip.forward, out hit, maxGrappleDistance, notCarLayers) && (whatIsGrappleable == (whatIsGrappleable | (1 << hit.transform.gameObject.layer))))
        {
            grapplePoint = hit.point;
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
        if (!joint && !aiming) return;

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
