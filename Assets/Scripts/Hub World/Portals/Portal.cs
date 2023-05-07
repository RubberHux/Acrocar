using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public int sceneIndex;
    public Transform returnPoint;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameMaster.SetHubWorldReturnPoint(returnPoint);
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
