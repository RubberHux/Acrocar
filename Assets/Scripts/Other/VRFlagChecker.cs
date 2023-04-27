using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Management;

//Code Credit: Nicholas Dechert https://gamedev.stackexchange.com/questions/195127/unity-2020-3-15f2-choose-between-vr-and-desktop-at-launch

public class VRFlagChecker : MonoBehaviour
{
    public void Awake()
    {
        StartCoroutine(StartXRCoroutine());
    }

    // From unity docs
    // https://docs.unity3d.com/Packages/com.unity.xr.management@4.0/manual/EndUser.html
    public IEnumerator StartXRCoroutine()
    {
        // Only run the code block when we want VR
        Debug.Log("Looking if VR should enable");
        if (System.Environment.GetCommandLineArgs().Contains("--enable-vr"))
        {
            Debug.Log("Initializing XR...");
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
            }
            else
            {
                Debug.Log("Starting XR...");
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                GameMaster.vr = true;
            }
        }
        else
        {
            Debug.Log("Did not find VR arg, starting in 2D");
        }
    }
}