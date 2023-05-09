using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardAnimator : MonoBehaviour
{
    [SerializeField] GameObject board, frontTruck, backTruck;
    [SerializeField] CarController carController;
    [SerializeField] Vector3 rightRotBoard, rightRotFrontTruck, rightRotBackTruck;
    [SerializeField] GameObject frontRightWheel, frontLeftWheel, backRightWheel, backLeftWheel;
    float lerpSpeed = 0.005f;

    // Update is called once per frame
    void Update()
    {
        float direction = carController.groundedWheels > 2 ? -carController.moveDir.x : 0;
        board.transform.localRotation = Quaternion.Lerp(board.transform.localRotation, Quaternion.Euler(rightRotBoard * direction), lerpSpeed);
        frontTruck.transform.localRotation = Quaternion.Lerp(frontTruck.transform.localRotation, Quaternion.Euler(rightRotFrontTruck * direction), lerpSpeed);
        backTruck.transform.localRotation = Quaternion.Lerp(backTruck.transform.localRotation, Quaternion.Euler(rightRotBackTruck * direction), lerpSpeed);
        Vector3 rot = frontLeftWheel.transform.localRotation.eulerAngles;
        frontLeftWheel.transform.localRotation = Quaternion.Euler(rot.x, rot.y, rot.z + carController.axleInfos[0].rightWheel.rpm * Time.deltaTime);
        rot = frontRightWheel.transform.localRotation.eulerAngles;
        frontRightWheel.transform.localRotation = Quaternion.Euler(rot.x, rot.y, rot.z - carController.axleInfos[0].leftWheel.rpm * Time.deltaTime);
        rot = backLeftWheel.transform.localRotation.eulerAngles;
        backLeftWheel.transform.localRotation = Quaternion.Euler(rot.x, rot.y, rot.z + carController.axleInfos[1].leftWheel.rpm * Time.deltaTime);
        rot = backRightWheel.transform.localRotation.eulerAngles;
        backRightWheel.transform.localRotation = Quaternion.Euler(rot.x, rot.y, rot.z - carController.axleInfos[1].rightWheel.rpm * Time.deltaTime);
    }
}
