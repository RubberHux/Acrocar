using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleBackAndForth : MonoBehaviour
{
    public float speed = 1.0f;
    public float scaleAmount = 0.5f;

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        float pingPong = Mathf.PingPong(Time.time * speed, scaleAmount);
        transform.localScale = initialScale + new Vector3(pingPong, pingPong, pingPong);
    }
}

