using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Management;

public class GameMaster : MonoBehaviour
{
    public static int playerCount = 1;
    public static InputDevice[] devices;
    public static bool vr = false;

    public void Exit()
    {
        Application.Quit();
    }
}
