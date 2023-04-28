using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class DeviceSetup : MonoBehaviour
{
    // Start is called before the first frame update
    List<InputDevice> devices = new List<InputDevice>();
    private static InputAction DeviceAdd;
    [SerializeField] private TextMeshProUGUI[] playerTexts;
    [SerializeField] private SceneLoader sceneLoader;
    private void OnEnable()
    {
        devices.Clear();
        DeviceAdd = InputHandler.playerInput.UI.Join;
        DeviceAdd.Enable();
        DeviceAdd.performed += UpdateInput;
    }

    private void OnDisable()
    {
        DeviceAdd.performed -= UpdateInput;
    }

    // Update is called once per frame
    void UpdateInput(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;
        if (!devices.Contains(device))
        {
            devices.Add(device);
            Debug.Log(devices.Count);
            playerTexts[devices.Count - 1].text = $"Player {devices.Count}\nJoined";
        }
        else
        {
            gameObject.SetActive(false);
            GameMaster.devices = devices.ToArray();
            GameMaster.playerCount = devices.Count;
            sceneLoader.Reload();
        }
    }
}
