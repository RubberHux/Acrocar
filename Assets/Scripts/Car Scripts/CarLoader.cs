using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class CarLoader : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject car, cameras; 
    public bool is2D;
    int Playercount;
    bool created;
    public LayerMask[] camLayers;
    private void OnEnable()
    {
        Physics.IgnoreLayerCollision(6, 6, true);
        if (!created)
        {
            Playercount = GameMaster.playerCount;
            for (int i = 0; i < Playercount; i++)
            {
                PlayerInput playerInput;
                if (Playercount == 1) playerInput = PlayerInput.Instantiate(car);
                else
                {
                    if (GameMaster.devices[i].deviceId == 1) playerInput = PlayerInput.Instantiate(car, controlScheme: "Keyboard&Mouse");
                    else playerInput = PlayerInput.Instantiate(car, pairWithDevice: GameMaster.devices[i]);
                }
                GameObject carInstance = playerInput.gameObject;
                carInstance.name = $"Player {i+1}";

                LevelMetaData lmd = FindObjectOfType<LevelMetaData>();
                print(GameMaster.hubWorldReturnPoint);
                if (lmd != null && lmd.stageType == LevelMetaData.StageType.HubWorld && GameMaster.hubWorldReturnPoint != null) 
                    carInstance.transform.SetPositionAndRotation((Vector3)GameMaster.hubWorldReturnPoint, GameMaster.hubWorldReturnRotation);
                else carInstance.transform.SetPositionAndRotation(transform.position, transform.rotation);

                CarController carController = carInstance.GetComponent<CarController>();
                carController.is2D = is2D;
                carController.isAlone = Playercount >= 1;
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
                if (Playercount == 2)
                {
                    camera.rect = new Rect((playerNumber == 0 ? 0 : 0.5f), 0, 0.5f, 1);
                }
                if (Playercount > 2 && Playercount <= 4)
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
