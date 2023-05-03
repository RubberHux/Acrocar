using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject defaultSelect;
    GameObject settings;
    public void OpenSettings()
    {
        if (settings == null) settings = gameObject.GetComponentInParent<UIController>().settingsInstance;
        settings.SetActive(true);
    }
    
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(defaultSelect);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
