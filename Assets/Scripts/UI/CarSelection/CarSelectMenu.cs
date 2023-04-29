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
    [SerializeField] int playerIndex;

    void Start()
    {
        if (carButtons != null) return;
        carButtons = new List<GameObject>();
        int index = 0;
        foreach (Car car in carKeeperPrefab.GetComponent<CarKeeper>().cars)
        {
            GameObject button = Instantiate(buttonPrefab, transform);
            Texture2D texture = car.thumbnail;
            print(button);
            CarSelectButton buttonScript = button.gameObject.GetComponent<CarSelectButton>();
            buttonScript.menu = this;
            buttonScript.index = index;
            button.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            carButtons.Append(button);
            index++;
        }
    }

    public void SetCar(int index)
    {
        GameMaster.SetPlayerCar(playerIndex, index);
        FindObjectOfType<CustomiseCar>().InstantiateCar(index);
    }
}
