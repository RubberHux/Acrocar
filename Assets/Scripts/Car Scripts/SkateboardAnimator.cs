using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardAnimator : MonoBehaviour
{
    [SerializeField] GameObject board, frontTruck, backTruck;
    [SerializeField] CarController carController;
    [SerializeField] Vector3 rightRotBoard, rightRotFrontTruck, rightRotBackTruck;
    float lerpSpeed = 0.005f;

    // Update is called once per frame
    void Update()
    {
        float direction = carController.groundedWheels > 2 ? -carController.moveDir.x : 0;
        board.transform.localRotation = Quaternion.Lerp(board.transform.localRotation, Quaternion.Euler(rightRotBoard * direction), lerpSpeed);
        frontTruck.transform.localRotation = Quaternion.Lerp(frontTruck.transform.localRotation, Quaternion.Euler(rightRotFrontTruck * direction), lerpSpeed);
        backTruck.transform.localRotation = Quaternion.Lerp(backTruck.transform.localRotation, Quaternion.Euler(rightRotBackTruck * direction), lerpSpeed);
    }
}
