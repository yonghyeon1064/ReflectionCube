using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //참조변수
    Rigidbody arrowRig;

    //변수
    Vector3 lastVelocity;

    // Start is called before the first frame update
    void Start()
    {
        arrowRig = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        lastVelocity = arrowRig.velocity;
    }

    public void ShootArrow(Vector3 aim, float ArrowForce) {
        arrowRig.AddForce(aim.normalized * ArrowForce);
    }
    
    private void OnCollisionEnter(Collision col) {
        ReflectionHandler(col);
    }

    void ReflectionHandler(Collision col) {
        if (col.gameObject.tag == "ReflectSurface") {
            arrowRig.velocity = Vector3.Reflect(lastVelocity, col.contacts[0].normal);
        }
    }
}
