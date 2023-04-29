using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.Management;

public class GameMaster : MonoBehaviour
{
    public static int playerCount = 1;
    public static InputDevice[] devices;
    public static bool vr = false;
    public static Vector3? hubWorldReturnPoint;
    public static Quaternion hubWorldReturnRotation;

    public void Exit()
    {
        Application.Quit();
    }

    public static void SetHubWorldReturnPoint(Transform point)
    {
        hubWorldReturnPoint = point.position;
        hubWorldReturnRotation = point.rotation;
    }

    private void Update()
    {
        print(hubWorldReturnPoint);
    }
}
