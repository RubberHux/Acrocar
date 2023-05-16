using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

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
        Invoke("LoadScene",2.0f);
    }

    public void LoadScene(int index)
    {
        Time.timeScale = 1.0f;
        //SceneManager.LoadScene(index);
        SceneManager.LoadSceneAsync(index);
    }

    public void Reload()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
