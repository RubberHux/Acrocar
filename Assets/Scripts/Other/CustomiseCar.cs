using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CustomiseCar : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] GameObject carKeeperPrefab;
    private CarKeeper carKeeper;
    private GameObject carInstance;
    private List<ColorChanger> colorChangers;

    void Start()
    {
        carKeeper = carKeeperPrefab.GetComponent<CarKeeper>();
        InstantiateCar(GameMaster.playerCars[playerIndex]);
    }

    public void InstantiateCar(int carIndex)
    {
        if (carInstance != null) Destroy(carInstance);
        GameObject currentCar = carKeeper.cars[carIndex].prefab;
        carInstance = Instantiate(currentCar);
        colorChangers = carInstance.GetComponentsInChildren<ColorChanger>().ToList();
        carInstance.GetComponent<CarController>().enabled = false;
        carInstance.GetComponent<GrapplingGun>().enabled = false;
        carInstance.transform.SetPositionAndRotation(transform.position, transform.rotation);
    }

    public void ChangeMainColor(Color color)
    {
        GameMaster.playerCarMainColours[playerIndex] = color;
        colorChangers.ForEach(x => x.UpdateColours(playerIndex));
    }
}
