using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomiseCar : MonoBehaviour
{
    [SerializeField] GameObject carKeeperPrefab;
    private CarKeeper carKeeper;
    private GameObject carInstance;
    private List<ColorChanger> colorChangers;
    bool coloured = false;

    void Start()
    {
        carKeeper = carKeeperPrefab.GetComponent<CarKeeper>();
        LoadPart(GameMaster.LoadableType.Car, GameMaster.playerCars[GetComponentInParent<Customizer9001>().playerIndex]);
    }

    public void LoadPart(GameMaster.LoadableType type, int carIndex)
    {
        if (type == GameMaster.LoadableType.Car)
        {
            if (carInstance != null) Destroy(carInstance);
            carInstance = Instantiate(carKeeper.cars[carIndex].prefab);
            colorChangers = carInstance.GetComponentsInChildren<ColorChanger>().ToList();
            carInstance.GetComponent<CarController>().enabled = false;
            carInstance.GetComponent<GrapplingGun>().enabled = false;
            carInstance.GetComponent<PlayerInput>().enabled = false;
            colorChangers.ForEach(x => x.UpdateColours(GetComponentInParent<Customizer9001>().playerIndex));
            carInstance.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
        else
        {
            carInstance.GetComponentsInChildren<CarPartLoader>().ToList().ForEach(x=> { if (x.type == type) x.LoadPart(); });
            colorChangers = carInstance.GetComponentsInChildren<ColorChanger>().ToList();
        }
    }

    public void ChangeMainColor(Color color)
    {
        if (GetComponentInParent<Customizer9001>().playerIndex == -1) return;
        GameMaster.playerCarMainColours[GetComponentInParent<Customizer9001>().playerIndex] = color;
        carInstance.GetComponentsInChildren<ColorChanger>().ToList().ForEach(x => x.UpdateColours(GetComponentInParent<Customizer9001>().playerIndex));
    }

    private void Update()
    {
        int playerIndex = GetComponentInParent<Customizer9001>().playerIndex;
        if (!coloured && playerIndex != -1)
        {
            colorChangers.ForEach(x => x.UpdateColours(playerIndex));
            coloured = true;
            print($" P{playerIndex + 1} Coloured!");
        }
    }
}
