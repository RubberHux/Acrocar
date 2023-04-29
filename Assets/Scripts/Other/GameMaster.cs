using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.Management;

public class GameMaster : MonoBehaviour
{
    [NonSerialized] public static int playerCount = 1;
    [NonSerialized] public static InputDevice[] devices;
    [NonSerialized] public static bool vr = false;
    [NonSerialized] public static Vector3? hubWorldReturnPoint;
    [NonSerialized] public static Quaternion hubWorldReturnRotation;
    [NonSerialized] public static int[] playerCars = null;

    static GameMaster()
    {
        playerCars = new int[4];
        for (int i = 0; i < playerCars.Length; i++) playerCars[i] = PlayerPrefs.GetInt($"p{i + 1}Car", 0);
        Debug.Log(PlayerPrefs.GetInt($"p{1}Car", 0));
    }
    

    public static void Exit()
    {
        Application.Quit();
    }

    public static void SetHubWorldReturnPoint(Transform point)
    {
        hubWorldReturnPoint = point.position;
        hubWorldReturnRotation = point.rotation;
    }

    public static void SetPlayerCar(int playerIndex, int carIndex) {
        playerCars[playerIndex - 1] = carIndex;
        PlayerPrefs.SetInt($"p{playerIndex}Car", carIndex);
    }
}
