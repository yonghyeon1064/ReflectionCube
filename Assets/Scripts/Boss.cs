using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    //전역 참조변수
    public GameObject camera;
    public GameObject gameM;
    GameManager gameManager;
    GameObject bossAni;
    GameObject bossRotate;
    GameObject player;
    Animator anim;
    public GameObject laserScale;
    public LayerMask layerMask;

    //전역변수

    //이동 관련
    bool isBossMoving = false;
    bool isRotate = false;
    float speed = 2.0f; //speed == 1 일때 1초동안 한번의 이동이 일어남
    public float moveMotionTime = 1.0f;

    //laser 관련
    bool isLaserFire = false;
    float laserTime = 0.8f;
    float laserTopTime = 0.5f;
    float laserMotionTime = 1.0f;
    bool isAirRotate = false;
    bool canStartAirRotate = false;
    bool laserFireEnd = false;

    //이동, laser 둘다 사용
    float remainAngle = 90f;
    float currentTimeSum = 0.0f;
    public float shakeTime = 0.1f;

    //Boss state
    public enum CurrentState {
        idle, attack, dead, weak
    };
    public CurrentState curState = CurrentState.idle;

    IEnumerator coroutine;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = gameM.GetComponent<GameManager>();
        player = GameObject.Find("Player");

        bossAni = transform.GetChild(0).gameObject;
        bossRotate = bossAni.transform.GetChild(0).gameObject;
        bossRotate.GetComponent<Collider>().enabled = false;
        anim = bossAni.GetComponent<Animator>();
        anim.SetBool("isFin", true);

        coroutine = RepeatChasing();
        StartCoroutine(FindPlayer());

    }

    // Update is called once per frame
    void Update()
    {
        BossMove();
        calculateLaserTime();
    }

    bool isBossAttacked = false;
    IEnumerator FindPlayer() {
        while(curState == CurrentState.idle) {
            yield return new WaitForSeconds(0.5f);
            if (isBossAttacked) {
                GetComponent<Collider>().enabled = false;
                bossRotate.GetComponent<Collider>().enabled = true;
                StartCoroutine(coroutine);
            }
        }
    }



    public float waitTime = 0.4f;
    IEnumerator RepeatChasing() {
        while (gameManager.gameActive) {
            yield return new WaitForSeconds(waitTime);
            ChasingPlayer();
        }
    }

    public void BossDied() {
        StopCoroutine(coroutine);
        curState = CurrentState.dead;
    }

    Vector3 playerDir;
    float moveCount = 4f; //레이저 발사 후, 이동 후 관리
    //Boss 행동패턴 AI (4번 이동 1번 레이저 반복)
    void ChasingPlayer() {
        if(moveCount == 0) {
            FireLaser();
        }
        else {
            //move
            playerDir = (player.transform.position - transform.position).normalized;
            if (Mathf.Abs(playerDir.x) >= Mathf.Abs(playerDir.z)) {
                if (playerDir.x >= 0)
                    SetDest("right");
                else
                    SetDest("left");
            }
            else {
                if (playerDir.z >= 0)
                    SetDest("up");
                else
                    SetDest("down");
            }
        }
    }

    Vector3 destPosition;
    Vector3 destAngleAxis;
    //Boss가 움직일 방향 지정 (이동 신호)
    void SetDest(string dir) {
        if(!isBossMoving && !isLaserFire) {
            if (dir == "right") {
                destPosition = transform.position + Vector3.right * 4;
                destAngleAxis = new Vector3(0, 0, -1);
            }
            else if (dir == "left") {
                destPosition = transform.position + Vector3.left * 4;
                destAngleAxis = new Vector3(0, 0, 1);
            }
            else if (dir == "up") {
                destPosition = transform.position + Vector3.forward * 4;
                destAngleAxis = new Vector3(11, 0, 0);
            }
            else if (dir == "down") {
                destPosition = transform.position + Vector3.back * 4;
                destAngleAxis = new Vector3(-1, 0, 0);
            }
            else {
                destPosition = transform.position;
                Debug.Log("Wrong Input");
            }
            anim.SetTrigger("moveSignal");

            curState = CurrentState.attack;
            isRotate = true;
            isBossMoving = true;

        }
    }

    //SetDest가 실행되면 발동, Boss를 움직임 (이동 관리)
    void BossMove() {
        if (isBossMoving) {
            currentTimeSum += Time.deltaTime;

            if (transform.position == destPosition && !isRotate && curState == CurrentState.attack) {
                curState = CurrentState.weak;
                camera.GetComponent<CameraWork>().ShakeCameraForTime(shakeTime);
            }

            if(transform.position == destPosition && !isRotate && (currentTimeSum >= moveMotionTime)) {
                currentTimeSum = 0f;
                isBossMoving = false;
                moveCount--;
                return;
            }
            
            //위치 (1초에 4 이동)
            if(transform.position != destPosition)
                transform.position = Vector3.MoveTowards(transform.position, destPosition, 4 * Time.deltaTime * speed);

            //각도 (1초에 90도 회전)
            if (isRotate) {
                float stepAngle = 90.0f * Time.deltaTime * speed;
                if ((remainAngle - stepAngle) >= 0) {
                    bossRotate.transform.Rotate(destAngleAxis, stepAngle, Space.World);
                    remainAngle -= stepAngle;
                }
                    
                else {
                    bossRotate.transform.Rotate(destAngleAxis, remainAngle, Space.World);
                    remainAngle = 90;
                    isRotate = false;
                }
            }
        }
    }

    //레이저 발사 신호
    void FireLaser() {
        if (!isLaserFire && !isBossMoving) {
            //레이저 발사
            RaycastHit hitResult;
            if (Physics.Raycast(transform.position, -1 * bossRotate.transform.forward, out hitResult, 100.0f, layerMask)) {
                if(hitResult.normal == Vector3.up) {
                    laserTime = 0.4f;
                    laserScale.transform.localScale = new Vector3(1, 10, 1); //아래방향 발사
                    anim.SetTrigger("laserDown");
                    isAirRotate = true;
                }
                    
                else
                    laserScale.transform.localScale = new Vector3(1, hitResult.distance/2, 1);
            }
            else
                laserScale.transform.localScale = new Vector3(1, 10, 1); //위 방향 발사
            isLaserFire = true;
            camera.GetComponent<CameraWork>().ShakeCameraForTime(shakeTime);
        }
    }

    //레이저 발사 관리
    void calculateLaserTime() {
        if (isLaserFire) {
            //레이저 발사 경과시간 계산
            currentTimeSum += Time.deltaTime;


            //시간경과 시 레이저 종료
            if (currentTimeSum >= laserTime && !laserFireEnd) {
                laserScale.transform.localScale = new Vector3(1, 0, 1);
                laserFireEnd = true;
            }

            // 공중에 뜬 경우
            if (isAirRotate) {
                //Boss가 최고점에 올라감을 체크
                if ((currentTimeSum >= laserTopTime - 0.005f) && !canStartAirRotate) { //오차 대비 0.05
                    canStartAirRotate = true;
                }
                //AirRotate
                if (isAirRotate && canStartAirRotate) {
                    float stepAngle = 90.0f * Time.deltaTime * 2;
                    if ((remainAngle - stepAngle) >= 0) {
                        bossRotate.transform.Rotate(Vector3.right, stepAngle, Space.World);
                        remainAngle -= stepAngle;
                    }

                    else {
                        bossRotate.transform.Rotate(Vector3.right, remainAngle, Space.World);
                        remainAngle = 90.0f;
                        isAirRotate = false;
                        canStartAirRotate = false;
                    }
                }
            }
            
            //시간경과 및 회전 종료 시 레이저 모션 종료
            if (currentTimeSum >= laserMotionTime && !isAirRotate) {
                currentTimeSum = 0.0f;
                laserTime = 0.8f;
                laserFireEnd = false;
                isLaserFire = false;
                moveCount = 4f;
            }
            
        }
    }

    int idleCount = 1;
    private void OnCollisionEnter(Collision col) {
        if(col.gameObject.tag == "FriendlyArrow" && idleCount == 1) {
            idleCount--;
            isBossAttacked = true;
        }
    }

    //Test용 함수
    void GetInput() {
        if (!isBossMoving) {
            // -1 ~ 1 사이 값 입력받기
            float inputX = Input.GetAxis("Horizontal");
            float inputZ = Input.GetAxis("Vertical");

            if (Mathf.Abs(inputX) > Mathf.Abs(inputZ)) {
                if (inputX > 0)
                    SetDest("right");
                else
                    SetDest("left");
            }
            else if (Mathf.Abs(inputX) < Mathf.Abs(inputZ)) {
                if (inputZ > 0)
                    SetDest("up");
                else
                    SetDest("down");
            }
            else if (Input.GetMouseButtonDown(0)) {
                FireLaser();
            }
            else
                return;
        }
    }

}
