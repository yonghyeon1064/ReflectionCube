using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //참조변수
    GameObject gameM;
    GameManager gameManager;
    GameObject boss;
    Rigidbody arrowRig;

    //변수
    Vector3 lastVelocity;

    // Start is called before the first frame update
    void Start()
    {
        gameM = GameObject.Find("GameManager");
        gameManager = gameM.GetComponent<GameManager>();

        boss = GameObject.Find("Boss");

        arrowRig = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameActive)
            lastVelocity = arrowRig.velocity;
        else
            arrowRig.velocity = new Vector3(0, 0, 0);
    }

    //Player 가 호출
    public void ShootArrow(Vector3 aim, float ArrowForce) {
        arrowRig.AddForce(aim.normalized * ArrowForce);
    }

    private void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Nucleus" && boss.GetComponent<Boss>().curState == Boss.CurrentState.weak ) {
            gameManager.Clear();
        }
    }

    private void OnCollisionEnter(Collision col) {
        if(gameManager.gameActive)
            ReflectionHandler(col);
    }

    void ReflectionHandler(Collision col) {
        if (col.gameObject.tag == "ReflectSurface" || col.gameObject.tag == "Boss") {
            arrowRig.velocity = Vector3.Reflect(lastVelocity, col.contacts[0].normal);
        }
    }
}
