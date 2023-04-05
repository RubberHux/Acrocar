using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHorizontal : MonoBehaviour
{
     public float moveDistance=1.0f;
    public float moveSpeed=0.5f;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool goToEnd=true;
 


// Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        endPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z + moveDistance);
    }


private void FixedUpdate()
    {
        if (goToEnd == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPosition, moveSpeed * Time.deltaTime); 
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime); 
        }

        if(Vector3.Distance(transform.position, endPosition)<=0.001  && goToEnd == true)
        {
            goToEnd = false;
        }
        else if(Vector3.Distance(transform.position, startPosition) <= 0.001 && goToEnd == false)
        {
            goToEnd = true;
        }
    }
}
