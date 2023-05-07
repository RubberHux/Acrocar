using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    private int mainMenu = 0, hubWorld = 1;

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

    public void LoadMainMenu() 
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(mainMenu);
        GameMaster.hubWorldReturnPoint = null;
    }

    public void LoadHubWorld() { 
        Time.timeScale = 1.0f; 
        SceneManager.LoadScene(hubWorld); 
    }

    public void Reload()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
