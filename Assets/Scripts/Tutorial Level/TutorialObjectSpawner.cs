using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TutorialObjectSpawner : MonoBehaviour
{
    [Serializable]
    public class SpawnedObject
    {
        public enum SpawnType
        {
            Spawn,
            DeSpawn
        }

        public SpawnType spawnType;
        public GameObject gameObj;
        public bool spawnOnlyOnce = true;
        public float autoDespawnAfterSeconds = -1;
        [HideInInspector] public bool spawned;
    }
    
    public SpawnedObject[] spawnOnTriggerEnter;
    public SpawnedObject[] spawnOnTriggerExit;

    private async void DespawnObj(GameObject obj, TimeSpan timeSpan)
    {
        await Task.Delay(timeSpan);
        if (obj != null) obj.SetActive(false);
    }
    
    private async void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Car"))
        {
            for (int i = 0; i < spawnOnTriggerEnter.Length; i++)
            {
                if(spawnOnTriggerEnter[i].spawnOnlyOnce && spawnOnTriggerEnter[i].spawned) continue;
                bool activeState = spawnOnTriggerEnter[i].spawnType == SpawnedObject.SpawnType.Spawn;
                if (spawnOnTriggerEnter[i].gameObj.activeSelf == activeState) continue;
                spawnOnTriggerEnter[i].gameObj.SetActive(activeState);
                spawnOnTriggerEnter[i].spawned = true;
                if (spawnOnTriggerEnter[i].autoDespawnAfterSeconds > 0)
                {
                    DespawnObj(spawnOnTriggerEnter[i].gameObj, TimeSpan.FromSeconds(spawnOnTriggerEnter[i].autoDespawnAfterSeconds));
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Car"))
        {
            for (int i = 0; i < spawnOnTriggerExit.Length; i++)
            {
                if(spawnOnTriggerExit[i].spawnOnlyOnce && spawnOnTriggerExit[i].spawned) continue;
                    bool activeState = spawnOnTriggerExit[i].spawnType == SpawnedObject.SpawnType.Spawn;
                if (spawnOnTriggerExit[i].gameObj.activeSelf == activeState) continue;
                spawnOnTriggerExit[i].gameObj.SetActive(activeState);
                spawnOnTriggerExit[i].spawned = true;
            }
        }
    }
}
