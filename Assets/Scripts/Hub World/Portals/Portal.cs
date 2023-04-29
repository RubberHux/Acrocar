using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public SceneAsset scene;
    public Transform returnPoint;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameMaster.SetHubWorldReturnPoint(returnPoint);
            print(GameMaster.hubWorldReturnPoint);
            SceneManager.LoadScene(scene.name);
        }
    }
}
