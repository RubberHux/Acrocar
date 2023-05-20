using System;
using UnityEngine;

[Serializable]
public class colorKeeper
{
    public enum ColorType
    {
        main,
        sub,
    }
    public ColorType type;
    public int index;
}

public class ColorChanger : MonoBehaviour
{
    [SerializeField] colorKeeper[] colors;
    

    public void UpdateColours(int playerIndex)
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        foreach (colorKeeper color in colors) {
            if (color.type == colorKeeper.ColorType.main) mr.materials[color.index].color = GameMaster.playerCarMainColours[playerIndex];
        }
    }
}
