using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    GameObject settings, addPlayers;
    // Start is called before the first frame update
    public void OpenSettings()
    {
        if (settings == null) settings = gameObject.GetComponentInParent<UIController>().settingsInstance;
        settings.SetActive(true);
        gameObject.SetActive(false);
    }
    public void OpenAddPlayers()
    {
        gameObject.GetComponentInParent<UIController>().OpenAddPlayers();
        gameObject.SetActive(false);
    }

    public void Back()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<CarController>().moveSound.Play();
        this.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
    }
}
