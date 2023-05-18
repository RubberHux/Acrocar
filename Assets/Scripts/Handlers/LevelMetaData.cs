using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMetaData : MonoBehaviour
{
    public enum StageType
    {
        Level,
        HubWorld,
    }
    public string ID;
    public StageType stageType;
    public double bronzeTime, silverTime, goldTime;
    public bool UseCustomGravity;
    public Vector3 customGravity;
}
