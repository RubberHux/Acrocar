using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    public SceneAsset mainMenu, hubWorld;

    // Singleton 
    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadScene(int index)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(index);
    }

    public void LoadSceneByAsset(SceneAsset scene)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(scene.name);
    }

    public void LoadMainMenu() 
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(mainMenu.name);
        GameMaster.hubWorldReturnPoint = null;
    }

    public void LoadHubWorld() { 
        Time.timeScale = 1.0f; 
        SceneManager.LoadScene(hubWorld.name); 
    }

    public void Reload()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
