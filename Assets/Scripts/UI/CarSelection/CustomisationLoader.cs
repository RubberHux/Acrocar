using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomisationLoader : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject customiseMenu;
    [SerializeField] Transform[] startPositions;
    int playerCount;
    bool created;
    
    private void Start()
    {
        Physics.IgnoreLayerCollision(6, 6, true);
        if (!created)
        {
            playerCount = GameMaster.playerCount;
            for (int i = 0; i < playerCount; i++)
            {
                PlayerInput playerInput;
                if (playerCount == 1) playerInput = PlayerInput.Instantiate(customiseMenu);
                else
                {
                    if (GameMaster.devices[i].deviceId == 1) playerInput = PlayerInput.Instantiate(customiseMenu, controlScheme: "Keyboard&Mouse");
                    else playerInput = PlayerInput.Instantiate(customiseMenu, pairWithDevice: GameMaster.devices[i]);
                }
                GameObject menuInstance = playerInput.gameObject;
                menuInstance.name = $"Player {i + 1}";
                menuInstance.GetComponent<Customizer9001>().playerIndex = i;
                menuInstance.transform.SetPositionAndRotation(startPositions[i].position, startPositions[i].rotation);
                if (i != 0) menuInstance.GetComponentsInChildren<AudioListener>().ToList().ForEach(x => x.enabled = false);
                Camera[] cameras = menuInstance.GetComponentsInChildren<Camera>();
                if (i != GameMaster.vrPlayerIndex || !GameMaster.vr) menuInstance.GetComponentInChildren<XROrigin>().gameObject.SetActive(false);
                foreach (Camera camera in cameras)
                {
                    if (!camera.gameObject.CompareTag("XRCam"))
                    {
                        if (playerCount == 2)
                        {
                            camera.rect = new Rect(0, (i == 0 ? 0.5f : 0), 1, 0.5f);
                        }
                        if (playerCount > 2 && playerCount <= 4)
                        {
                            if (i == 0)
                            {
                                camera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                            }
                            else if (i == 1) camera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                            else if (i == 2) camera.rect = new Rect(0, 0, 0.5f, 0.5f);
                            else if (i == 3) camera.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                        }
                    }
                }
            }
            created = true;
        }
    }
}
