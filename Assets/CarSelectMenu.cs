using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectMenu : MonoBehaviour
{
    [SerializeField] GameObject buttonPrefab, carKeeperPrefab;
    List<GameObject> carButtons;
    void Start()
    {
        if (carButtons != null) return;
        carButtons = new List<GameObject>();
        foreach (Car car in carKeeperPrefab.GetComponent<CarKeeper>().cars)
        {
            GameObject button = Instantiate(buttonPrefab, transform);
            Texture2D texture = car.thumbnail;
            print(button);
            button.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            carButtons.Append(button);
        }
    }

    public void SetCar(int index)
    {
        FindObjectOfType<CustomiseCar>().InstantiateCar(index);
    }
}
