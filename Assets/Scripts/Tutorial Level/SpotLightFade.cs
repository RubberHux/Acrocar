using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpotLightFade : MonoBehaviour
{
    public float duration;

    private Light _spotLight;
    private float _oriIntensity;

    private void Start()
    {
        _spotLight = GetComponent<Light>();
        _oriIntensity = _spotLight.intensity;
    }

    private async void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Car"))
        {
            await SwitchOn();
        }
    }

    private async void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Car"))
        {
            await SwitchOff();
        }
    }


    async Task SwitchOn()
    {
        _spotLight.enabled = true;
        int numSteps = Mathf.FloorToInt(duration / 0.02f);
        float deltaInt = _oriIntensity / numSteps;
        _spotLight.intensity = 0f;
        for (int i = 0; i < numSteps; i++)
        {
            _spotLight.intensity = i * deltaInt;
            await Task.Delay(20);  // 0.02 seconds
        }
    }
    
    async Task SwitchOff()
    {
        int numSteps = Mathf.FloorToInt(duration / 0.02f);
        float deltaInt = _oriIntensity / numSteps;
        _spotLight.intensity = 0f;
        for (int i = 0; i < numSteps; i++)
        {
            _spotLight.intensity = _oriIntensity - i * deltaInt;
            await Task.Delay(20);  // 0.02 seconds
        }

        _spotLight.enabled = false;
    }
}
