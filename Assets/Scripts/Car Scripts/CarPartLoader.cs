using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using UnityEngine;

public class CarPartLoader : MonoBehaviour
{

    public GameMaster.LoadableType type;
    public GameObject carKeeper;
    private int playerIndex;
    private GameObject objectInstance;

    // Start is called before the first frame update
    void Start()
    {
        playerIndex = GetComponentInParent<CarController>().playerIndex;
        LoadPart();
    }

    public void LoadPart()
    {
        if (objectInstance != null) Destroy(objectInstance);
        GameObject prefab = null;

        switch (type)
        {
            case GameMaster.LoadableType.Spoiler:
                prefab = carKeeper.GetComponent<CarKeeper>().spoilers[GameMaster.playerSpoilers[GetComponentInParent<CarController>().playerIndex]].prefab;
                break;
            case GameMaster.LoadableType.RoofAccessory:
                prefab = carKeeper.GetComponent<CarKeeper>().roofAccessories[GameMaster.playerRoofAccessory[GetComponentInParent<CarController>().playerIndex]].prefab;
                break;
            case GameMaster.LoadableType.HoodAccessory:
                prefab = carKeeper.GetComponent<CarKeeper>().hoodAccessories[GameMaster.playerHoodAccessory[GetComponentInParent<CarController>().playerIndex]].prefab;
                break;
            default:
                Debug.LogError("The LoadableType has not been added to LoadPart in CarPartLoader!!!");
                return;
        }

        objectInstance = Instantiate(prefab, transform);
        objectInstance.GetComponentsInChildren<ColorChanger>().ToList().ForEach(x => x.UpdateColours(playerIndex));
    }
}
