using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
// Class from Unity documentation, with slight modification (due to no need for steering):
// https://docs.unity3d.com/Manual/WheelColliderTutorial.html
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
}

public class CarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public int maxTorque;

    // Update is called once per frame
    void Update()
    {
        float torque = maxTorque * (2 * Input.GetAxis("Vertical"));

        // drive forward or backwards based on input
        foreach (AxleInfo aInfo in axleInfos)
        {
            if (aInfo.motor)
            {
                aInfo.leftWheel.motorTorque = torque;
                aInfo.rightWheel.motorTorque = torque;
            }
        }

        // rotate car via horizontal movement inputs
        transform.Rotate(Input.GetAxis("Horizontal"), 0, 0);

        Vector3 newPos = transform.position;
        Quaternion newRot = new Quaternion(transform.rotation.x, 0, 0, transform.rotation.w);
        if (Input.GetKeyDown(KeyCode.R))
        {
            newPos = new Vector3(0, 1, 0);
            newRot = new Quaternion(0, 0, 0, 0);
        }
        transform.SetPositionAndRotation(newPos, newRot);
    }
}
