using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeMove : MonoBehaviour
{

Rigidbody rig;
public Vector3  dir = Vector3.back;
public float speed = 30.0f;
private Vector3 startPosition;
private Vector3 endPosition;
private bool goToEnd=true;
    // Start is called before the first frame update
    void Start()
    {
        rig = this.GetComponent<Rigidbody>();
        startPosition = transform.position;
        endPosition = new Vector3(startPosition.x, startPosition.y, -250);
    }

    // Update is called once per frame
    void Update()
    {
        if(goToEnd == true)
        {
             rig.velocity = dir * speed;
           
        }
        else
        {
            transform.position = startPosition;
            goToEnd = true;
        }

         if(transform.position.z <= -250  && goToEnd == true)
        {
            goToEnd = false;
        }
 

       
    }
}
