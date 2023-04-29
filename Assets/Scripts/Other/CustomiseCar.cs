using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomiseCar : MonoBehaviour
{
    [SerializeField] private int playerNumber;
    [SerializeField] GameObject carKeeperPrefab;
    private CarKeeper carKeeper;
    private GameObject carInstance;

    void Start()
    {
        carKeeper = carKeeperPrefab.GetComponent<CarKeeper>();
        InstantiateCar(GameMaster.playerCars[playerNumber]);
    }

    public void InstantiateCar(int carIndex)
    {
        if (carInstance != null) Destroy(carInstance);
        GameObject currentCar = carKeeper.cars[carIndex].prefab;
        carInstance = Instantiate(currentCar);
        carInstance.GetComponent<CarController>().enabled = false;
        carInstance.GetComponent<GrapplingGun>().enabled = false;
        carInstance.transform.SetPositionAndRotation(transform.position, transform.rotation);
    }
}
