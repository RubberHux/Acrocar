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
        InstantiateCar(GameMaster.playerCars[GetComponentInParent<Customizer9001>().playerIndex]);
    }

    public void InstantiateCar(int carIndex)
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

    public void ChangeMainColor(Color color)
    {
        GameMaster.playerCarMainColours[GetComponentInParent<Customizer9001>().playerIndex] = color;
        colorChangers.ForEach(x => x.UpdateColours(GetComponentInParent<Customizer9001>().playerIndex));
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
