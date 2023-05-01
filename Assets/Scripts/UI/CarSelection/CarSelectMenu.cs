using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class CarSelectMenu : MonoBehaviour
{
    [SerializeField] GameObject buttonPrefab, carKeeperPrefab;
    GameObject[] carButtons;

    [SerializeField] CustomiseCar car;
    [SerializeField] MultiplayerEventSystem eventSystem;

    void Awake()
    {
        if (carButtons != null) return;
        Car[] cars = carKeeperPrefab.GetComponent<CarKeeper>().cars;
        carButtons = new GameObject[cars.Length];
        int index = 0;
        foreach (Car car in cars)
        {
            GameObject button = Instantiate(buttonPrefab, transform);
            Texture2D texture = car.thumbnail;
            print(button);
            CarSelectButton buttonScript = button.gameObject.GetComponent<CarSelectButton>();
            buttonScript.menu = this;
            buttonScript.index = index;
            button.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            carButtons[index] = button;
            if (index == 0) eventSystem.SetSelectedGameObject(button);
            index++;
        }
    }

    private void OnEnable()
    {
        if (carButtons != null) eventSystem.SetSelectedGameObject(carButtons[0]);
    }

    public void SetCar(int index)
    {
        GameMaster.SetPlayerCar(GetComponentInParent<Customizer9001>().playerIndex, index);
        car.InstantiateCar(index);
    }
}
