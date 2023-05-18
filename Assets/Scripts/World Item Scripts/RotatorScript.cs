using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorScript : MonoBehaviour
{
    [SerializeField] Vector3 rotationVector;
    [SerializeField] float speed;
    Transform transform;
    Quaternion rotationQuaternion;

    private void Start()
    {
        transform = GetComponent<Transform>();
        rotationQuaternion = Quaternion.Euler(rotationVector);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate((rotationVector * speed * Time.deltaTime));
    }
}
