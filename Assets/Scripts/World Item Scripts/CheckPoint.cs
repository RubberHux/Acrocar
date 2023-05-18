using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    bool activated;
    List<int> activePlayers = new();
    [SerializeField] GameObject flag;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CarController carController = other.GetComponent<CarController>();
            if (activePlayers.Contains(carController.playerIndex)) return;

            activePlayers.Append(carController.playerIndex);
            if (carController.lastCheckPoint != null) carController.lastCheckPoint.UnCheck(carController.playerIndex);
            carController.lastCheckPoint = this;
            carController.respawn2D = carController.is2D;
            if (!activated)
            {
                activated = true;
                flag.SetActive(true);
                flag.GetComponent<ColorChanger>().UpdateColours(carController.playerIndex);
            }
        }
    }

    public void UnCheck(int carIndex)
    {
        activePlayers.Remove(carIndex);
        if (activePlayers.Count == 0)
        {
            activated = false;
            flag.SetActive(false);
        }
    }
}
