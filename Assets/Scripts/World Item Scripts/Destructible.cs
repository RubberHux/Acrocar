using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public float breakForce; // the force needed to destroy the object

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody colliding = collision.rigidbody;
        if (colliding != null)
        {
            float collidingVelocity = Mathf.Max(Mathf.Abs(colliding.velocity.y), Mathf.Abs(colliding.velocity.y));
            float mass = colliding.mass;
            float collisionForce = collision.relativeVelocity.magnitude * mass / 2;
            if (collisionForce >= breakForce) Destroy(gameObject);
            Debug.Log(collisionForce);
        }
    }
}
