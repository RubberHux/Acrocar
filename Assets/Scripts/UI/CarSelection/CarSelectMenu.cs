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
    GameObject[] objButtons;

    [SerializeField] CustomiseCar car;
    [SerializeField] MultiplayerEventSystem eventSystem;
    CarKeeper carKeeper;


    public GameMaster.LoadableType? loadType = null;

    public void Awake()
    {
        carKeeper = carKeeperPrefab.GetComponent<CarKeeper>();
    }

    public void LoadCar() { LoadObjects(GameMaster.LoadableType.Car); }
    public void LoadSpoiler() { LoadObjects(GameMaster.LoadableType.Spoiler); }
    public void LoadRoofAccessory() { LoadObjects(GameMaster.LoadableType.RoofAccessory); }
    public void LoadHoodAccessory() { LoadObjects(GameMaster.LoadableType.HoodAccessory); }

    public void LoadObjects(GameMaster.LoadableType type)
    {
        if (type == loadType)
        {
            eventSystem.SetSelectedGameObject(objButtons[0]);
            return;
        }
        loadType = type;
        Loadable[] loadables = null;
        if (loadType == GameMaster.LoadableType.Car) loadables = carKeeper.cars;
        else if (loadType == GameMaster.LoadableType.Spoiler) loadables = carKeeper.spoilers;
        else if (loadType == GameMaster.LoadableType.RoofAccessory) loadables = carKeeper.roofAccessories;
        else if (loadType == GameMaster.LoadableType.HoodAccessory) loadables = carKeeper.hoodAccessories;
        if (objButtons != null) objButtons.ToList().ForEach(x => Destroy(x));
        objButtons = new GameObject[loadables.Length];
        int index = 0;
        foreach (Loadable obj in loadables)
        {
            GameObject button = Instantiate(buttonPrefab, transform);
            Texture2D texture = obj.thumbnail;
            print(button);
            CarSelectButton buttonScript = button.gameObject.GetComponent<CarSelectButton>();
            buttonScript.menu = this;
            buttonScript.index = index;
            button.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            objButtons[index] = button;
            if (index == 0) eventSystem.SetSelectedGameObject(button);
            index++;
        }
    }

    private void OnEnable()
    {
        if (objButtons != null) eventSystem.SetSelectedGameObject(objButtons[0]);
    }

    public void SetCar(int index)
    {
        if (loadType != null)
        {
            GameMaster.SetPlayerPart((GameMaster.LoadableType) loadType, GetComponentInParent<Customizer9001>().playerIndex, index);
            car.LoadPart((GameMaster.LoadableType)loadType, index);
        }
    }
}
