using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //참조변수
    GameObject gameM;
    GameManager gameManager;
    GameObject boss;
    GameObject player;
    Rigidbody arrowRig;
    Renderer arrowColor;

    //변수
    Vector3 lastVelocity;
    int colCount = 2;

    //Arrow state
    public enum CurrentArrowState {
        idle, power
    };
    public CurrentArrowState arrowState = CurrentArrowState.idle;

    // Start is called before the first frame update
    void Start()
    {
        gameM = GameObject.Find("GameManager");
        gameManager = gameM.GetComponent<GameManager>();

        boss = GameObject.Find("Boss");
        player = GameObject.Find("Player");

        arrowRig = gameObject.GetComponent<Rigidbody>();
        arrowColor = gameObject.GetComponent<Renderer>();

        StartCoroutine(ChangeArrowState());
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

    float powerSpeed = 10f;
    IEnumerator ChangeArrowState() {
        while (gameManager.gameActive) {
            //Debug.Log(arrowRig.velocity.magnitude);
            if(arrowRig.velocity.magnitude > powerSpeed && colCount < 2) {
                arrowState = CurrentArrowState.power;
                arrowColor.material.color = Color.red;
            }
            else {
                arrowState = CurrentArrowState.idle;
                arrowColor.material.color = Color.blue;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Nucleus" && boss.GetComponent<Boss>().curState == Boss.CurrentState.weak && arrowState == CurrentArrowState.power) {
            gameManager.Clear();
        }
    }

    private void OnCollisionEnter(Collision col) {
        if (gameManager.gameActive) {
            if (col.gameObject.tag == "ReflectSurface" || col.gameObject.tag == "Boss") {
                ReflectionHandler(col);
            }
        }
            
    }

    void ReflectionHandler(Collision col) {
        if (colCount > 0) {
            arrowRig.velocity = Vector3.Reflect(lastVelocity, col.contacts[0].normal);
            colCount--;
        }
        else {
            player.GetComponent<Player>().GetArrowBack();
            colCount = 2;
        }
    }
}
