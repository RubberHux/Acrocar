using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHandler
{
    public static PlayerInputActions playerInput = new PlayerInputActions();
    private static InputAction schemeCheck;
    public static int currentScheme;
    static InputHandler()
    {
        schemeCheck = playerInput.SchemeChecker.SchemeCheck;
        schemeCheck.Enable();
        schemeCheck.performed += UpdateInput;
    }

    static void UpdateInput(InputAction.CallbackContext context)
    {
       currentScheme = context.control.device.deviceId;
    }
}
