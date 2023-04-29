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
    public StageType stageType;
    public double bronzeTime, silverTime, goldTime;
}
