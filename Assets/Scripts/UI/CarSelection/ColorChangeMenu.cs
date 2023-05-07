using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class ColorChangeMenu : MonoBehaviour
{
    [SerializeField] Slider sliderRed, sliderGreen, sliderBlue;
    private Color color = Color.red;
    [SerializeField] CustomiseCar car;
    [SerializeField] private GameObject defaultSelect;
    [SerializeField] MultiplayerEventSystem eventSystem;

    private void OnEnable()
    {
        eventSystem.SetSelectedGameObject(defaultSelect);
    }

    void Start()
    {
        color = GameMaster.playerCarMainColours[GetComponentInParent<Customizer9001>().playerIndex];
        if (color == null) return;
        float r = color.r;
        if (sliderRed != null) sliderRed.value = r;
        if (sliderGreen != null) sliderGreen.value = color.g;
        if (sliderBlue != null) sliderBlue.value = color.b;
    }

    public void SetRed(float value) 
    {
        if (GameMaster.playerCarMainColours == null) return;
        color.r = value;
        car.ChangeMainColor(color);
    }
    public void SetGreen(float value) 
    {
        if (GameMaster.playerCarMainColours == null) return;
        color.g = value;
        car.ChangeMainColor(color);
    }
    public void SetBlue(float value) 
    {
        if (GameMaster.playerCarMainColours == null) return;
        color.b = value;
        car.ChangeMainColor(color);
    }
    public void SetAlpha(float value) 
    {
        //Just in case we want it later
        if (GameMaster.playerCarMainColours == null) return;
        color.a = value;
        car.ChangeMainColor(color);
    } 

    public void SetColour()
    {
        GameMaster.SetPlayerCarMainColor(GetComponentInParent<Customizer9001>().playerIndex, color);
    }
}
