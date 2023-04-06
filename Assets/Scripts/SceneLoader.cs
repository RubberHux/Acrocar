using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    public void LoadScene(int index)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(index);
    }
}
