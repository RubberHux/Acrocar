using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UIController;
using UnityEngine.InputSystem.XR;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.SocialPlatforms;
using UnityEngine.Experimental.AI;
using UnityEngine.InputSystem;

public class CarLoader : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject car, cameras; 
    public bool is2D;
    public int amount;
    bool created;
    public LayerMask[] camLayers;
    private void OnEnable()
    {
        Physics.IgnoreLayerCollision(6, 6, true);
        if (!created)
        {
            for (int i = 0; i < amount; i++)
            {
                PlayerInput playerInput;
                if (amount == 1) playerInput = PlayerInput.Instantiate(car);
                else playerInput = PlayerInput.Instantiate(car, controlScheme: (i == 0 ? "Keyboard&Mouse" : "Gamepad") );
                GameObject carInstance = playerInput.gameObject;
                carInstance.transform.position = transform.position;
                CarController carController = carInstance.GetComponent<CarController>();
                carController.is2D = is2D;
                carController.isAlone = amount >= 1;
                GameObject camInstance = Instantiate(cameras);
                LinkCarAndCam(carController, camInstance, i, playerInput);
            }
            created = true;
        }
    }

    private void LinkCarAndCam(CarController carController, GameObject cam, int playerNumber, PlayerInput playerInput)
    {
        LayerMask camLayerMask = ~0;
        for (int i = 0; i < camLayers.Length; i++) if (playerNumber != i) camLayerMask -= camLayers[i];
        cam.GetComponentInChildren<CineMachine3DController>().SetCar(carController, 28 + playerNumber, camLayerMask, playerNumber == 0, playerInput);
        carController.SetCam(cam, playerNumber);
        carController.gameObject.GetComponent<GrapplingGun>().SetCam(cam);
        Camera camera = cam.GetComponentInChildren<Camera>();
        if (amount == 2)
        {
            camera.rect = new Rect((playerNumber == 0 ? 0 : 0.5f), 0, 0.5f, 1);
        }
        if (amount > 2 && amount <= 4)
        {
            if (playerNumber == 0) camera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
            else if (playerNumber == 1) camera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            else if (playerNumber == 2) camera.rect = new Rect(0, 0, 0.5f, 0.5f);
            else if (playerNumber == 3) camera.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
        }
    }
}
