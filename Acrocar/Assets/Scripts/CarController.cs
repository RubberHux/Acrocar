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
    public List<AxleInfo> axleInfos; // list of axle infos, including wheel colliders
    public int maxTorque; // maximum torque
    private float torque; // current torque
    public bool grappling;

    // Update is called once per frame
    void Update()
    {
        if (!grappling)
        { 
            float movDir = Input.GetAxis("Vertical");
            // if direction is changed: brake and then accelerate
            torque = maxTorque * (2 * movDir);

            bool brake = Input.GetKey(KeyCode.Space); // should car brake?

                // drive forward or backwards based on input
                foreach (AxleInfo aInfo in axleInfos)
                {
                    if (aInfo.motor)
                    {
                        // if car should brake, set the brake torque
                        if (brake)
                        {
                            aInfo.leftWheel.brakeTorque = maxTorque * 10;
                            aInfo.rightWheel.brakeTorque = maxTorque * 10;

                        }
                        // if not braking, set brake torque to 0 and motor torque to current torque
                        else
                        {
                            // reset input axes so we don't go too fast after breaking
                            if (Input.GetKeyUp(KeyCode.Space)) Input.ResetInputAxes();

                            aInfo.leftWheel.brakeTorque = 0;
                            aInfo.rightWheel.brakeTorque = 0;

                            aInfo.leftWheel.motorTorque = torque;
                            aInfo.rightWheel.motorTorque = torque;
                        }
                    }
                }

            // rotate car via horizontal movement inputs (along x-axis)
            transform.Rotate(Input.GetAxis("Horizontal"), 0, 0);

            // make sure car cannot be moved or rotated outside 2D perspective
            Vector3 newPos = new Vector3(0, transform.position.y, transform.position.z);
            Quaternion newRot = new Quaternion(transform.rotation.x, 0, 0, transform.rotation.w);

            // reset car position and rotation when R is pressed
            if (Input.GetKeyDown(KeyCode.R))
            {
                newPos = new Vector3(0, 1, 0);
                newRot = new Quaternion(0, 0, 0, 0);
            }

            // update position and rotation based on above
            transform.SetPositionAndRotation(newPos, newRot);
        }
    }
}
