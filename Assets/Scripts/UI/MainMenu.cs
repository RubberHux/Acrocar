using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject defaultSelect;
    GameObject settings;
    // Start is called before the first frame update
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
