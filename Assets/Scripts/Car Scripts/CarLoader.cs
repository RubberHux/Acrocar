using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using System.Linq;

public class CarLoader : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject cameras;
    [SerializeField] GameObject carKeeperPrefab;
    public bool is2D;
    int playerCount;
    bool created;
    public LayerMask[] camLayers;
    private void Start()
    {
        CarKeeper carKeeper = carKeeperPrefab.GetComponent<CarKeeper>();
        Physics.IgnoreLayerCollision(6, 6, true);
        if (!created)
        {
            playerCount = GameMaster.playerCount;
            for (int i = 0; i < playerCount; i++)
            {
                PlayerInput playerInput;
                GameObject currentCar = carKeeper.cars[GameMaster.playerCars[i]].prefab;
                if (playerCount == 1) playerInput = PlayerInput.Instantiate(currentCar);
                else
                {
                    if (GameMaster.devices[i].deviceId == 1) playerInput = PlayerInput.Instantiate(currentCar, controlScheme: "Keyboard&Mouse");
                    else playerInput = PlayerInput.Instantiate(currentCar, pairWithDevice: GameMaster.devices[i]);
                }
                GameObject carInstance = playerInput.gameObject;
                carInstance.name = $"Player {i+1}";
                CarController carController = carInstance.GetComponent<CarController>();
                carController.is2D = is2D;
                carController.isAlone = playerCount >= 1;
                carController.playerIndex = i;
                carInstance.GetComponentsInChildren<ColorChanger>().ToList().ForEach(x => x.UpdateColours(i));

                LevelMetaData lmd = FindObjectOfType<LevelMetaData>();
                if (lmd != null && lmd.stageType == LevelMetaData.StageType.HubWorld && GameMaster.hubWorldReturnPoint != null)
                {
                    carInstance.transform.SetPositionAndRotation((Vector3)GameMaster.hubWorldReturnPoint, GameMaster.hubWorldReturnRotation);
                    carController.startpoint = (Vector3)GameMaster.hubWorldReturnPoint;
                    carController.startRot = GameMaster.hubWorldReturnRotation;
                }
                else carInstance.transform.SetPositionAndRotation(transform.position, transform.rotation);

                
                GameObject camInstance = Instantiate(cameras);
                camInstance.name = $"Cam {i + 1}";
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
        if (playerNumber != 0 || !GameMaster.vr) cam.GetComponentInChildren<XROrigin>().gameObject.SetActive(false);
        
        Camera[] cameras = cam.GetComponentsInChildren<Camera>();
        foreach (Camera camera in cameras)
        {
            if (!camera.gameObject.CompareTag("XRCam"))
            {
                if (playerCount == 2)
                {
                    camera.rect = new Rect(0, (playerNumber == 0 ? 0.5f : 0), 1, 0.5f);
                }
                if (playerCount > 2 && playerCount <= 4)
                {
                    if (playerNumber == 0)
                    {
                        camera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                    }
                    else if (playerNumber == 1) camera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                    else if (playerNumber == 2) camera.rect = new Rect(0, 0, 0.5f, 0.5f);
                    else if (playerNumber == 3) camera.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                }
            }
        }
    }
}
