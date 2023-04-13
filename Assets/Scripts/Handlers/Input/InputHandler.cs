using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHandler
{
    public static PlayerInputActions playerInput = new PlayerInputActions();
    private static InputAction deviceCheck;
    public static int currentDevice;
    static InputHandler()
    {
        deviceCheck = playerInput.SchemeChecker.SchemeCheck;
        deviceCheck.Enable();
        deviceCheck.performed += UpdateInput;
    }

    static void UpdateInput(InputAction.CallbackContext context)
    {
       currentDevice = context.control.device.deviceId;
    }
}
