using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMaster : MonoBehaviour
{
    [NonSerialized] public static int playerCount = 1;
    [NonSerialized] public static InputDevice[] devices;
    [NonSerialized] public static bool vr = false;
    [NonSerialized] public static Vector3? hubWorldReturnPoint;
    [NonSerialized] public static Quaternion hubWorldReturnRotation;
    [NonSerialized] public static int[] playerCars, playerSpoilers, playerRoofAccessory, playerHoodAccessory;
    [NonSerialized] public static Color[] playerCarMainColours = null;
    [NonSerialized] public const int maxPlayerCount = 4;
    [NonSerialized] public static int vrPlayerIndex = 0;

    public enum LoadableType
    {
        Car,
        Spoiler,
        RoofAccessory,
        HoodAccessory,
    }

    static GameMaster()
    {
        playerCars = new int[maxPlayerCount];
        playerSpoilers = new int[maxPlayerCount];
        playerRoofAccessory = new int[maxPlayerCount];
        playerHoodAccessory = new int[maxPlayerCount];
        playerCarMainColours = new Color[maxPlayerCount];
        for (int i = 0; i < playerCars.Length; i++)
        {
            playerCars[i] = PlayerPrefs.GetInt($"p{i + 1}Car", 0);
            playerSpoilers[i] = PlayerPrefs.GetInt($"p{i + 1}Spoiler", 0);
            playerRoofAccessory[i] = PlayerPrefs.GetInt($"p{i + 1}RoofAccessory", 0);
            playerHoodAccessory[i] = PlayerPrefs.GetInt($"p{i + 1}HoodAccessory", 0);
            string color = PlayerPrefs.GetString($"p{i + 1}CarColorMain", "1,0,0,1");
            string[] colorVals = color.Split(",");

            if (colorVals.Length == 4 && 
                float.TryParse(colorVals[0], out float r) && r >= 0 && r <= 1 &&
                float.TryParse(colorVals[1], out float g) && g >= 0 && g <= 1 &&
                float.TryParse(colorVals[2], out float b) && b >= 0 && b <= 1 &&
                float.TryParse(colorVals[3], out float a) && a >= 0 && a <= 1) playerCarMainColours[i] = new Color(r, g, b, a);
            else
            {
                Debug.LogError($"The player pref for the main colour of player {i + 1} contains bad values");
                playerCarMainColours[i] = Color.red;
            }
        }
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

    public static void SetPlayerPart(LoadableType type, int playerIndex, int objIndex) {
        if (playerIndex >= 0 && playerIndex < maxPlayerCount)
        {
            switch(type)
            {
                case LoadableType.Car:
                    playerCars[playerIndex] = objIndex;
                    PlayerPrefs.SetInt($"p{playerIndex + 1}Car", objIndex);
                    break;
                case LoadableType.Spoiler:
                    playerSpoilers[playerIndex] = objIndex;
                    PlayerPrefs.SetInt($"p{playerIndex + 1}Spoiler", objIndex);
                    break;
                case LoadableType.RoofAccessory:
                    playerRoofAccessory[playerIndex] = objIndex;
                    PlayerPrefs.SetInt($"p{playerIndex + 1}RoofAccessory", objIndex);
                    break;
                case LoadableType.HoodAccessory:
                    playerHoodAccessory[playerIndex] = objIndex;
                    PlayerPrefs.SetInt($"p{playerIndex + 1}HoodAccessory", objIndex);
                    break;
            }
        }
    }

    public static void SetPlayerCarMainColor(int playerIndex, Color color)
    {
        if (playerIndex >= 0 && playerIndex < maxPlayerCount)
        {
            playerCarMainColours[playerIndex] = color;
            PlayerPrefs.SetString($"p{playerIndex + 1}CarColorMain", $"{color.r},{color.g},{color.b},{color.a}");
        }
    }
}
