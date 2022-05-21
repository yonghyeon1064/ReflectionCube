using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    //전역 참조변수
    Vector3 destPosition;
    Vector3 destAngleAxis;
    GameObject bossAni;
    GameObject bossRotate;
    Animator anim;
    public GameObject laserScale;
    public LayerMask layerMask;

    //전역변수
    bool isBossMoving = false;
    bool isRotate = false;
    float remainAngle = 90f;
    float speed = 2.0f; //speed == 1 일때 1초동안 한번의 이동이 일어남

    bool isLaserFire = false;
    float laserTime = 0.8f;
    float laserTopTime = 0.5f;
    float laserMotionTime = 1.0f;
    float currentLaserTimeSum = 0.0f;
    bool isAirRotate = false;
    bool canStartAirRotate = false;
    bool laserFireEnd = false;

    // Start is called before the first frame update
    void Start()
    {
        bossAni = transform.GetChild(0).gameObject;
        bossRotate = bossAni.transform.GetChild(0).gameObject;
        anim = bossAni.GetComponent<Animator>();
        anim.SetBool("isFin", true);

    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        BossMove();
        calculateLaserTime();
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
            isRotate = true;
            isBossMoving = true;
        }        
    }

    //SetDest가 실행되면 발동, Boss를 움직임 (이동 관리)
    void BossMove() {
        if (isBossMoving) {
            
            if(transform.position == destPosition && !isRotate) {
                isBossMoving = false;
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
        }
    }

    //레이저 발사 관리
    void calculateLaserTime() {
        if (isLaserFire) {
            //레이저 발사 경과시간 계산
            currentLaserTimeSum += Time.deltaTime;


            //시간경과 시 레이저 종료
            if (currentLaserTimeSum >= laserTime && !laserFireEnd) {
                laserScale.transform.localScale = new Vector3(1, 0, 1);
                laserFireEnd = true;
            }

            // 공중에 뜬 경우
            if (isAirRotate) {
                //Boss가 최고점에 올라감을 체크
                if ((currentLaserTimeSum >= laserTopTime - 0.005f) && !canStartAirRotate) { //오차 대비 0.05
                    canStartAirRotate = true;
                    Debug.Log(bossRotate.transform.rotation);
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
            if (currentLaserTimeSum >= laserMotionTime && !isAirRotate) {
                currentLaserTimeSum = 0.0f;
                laserTime = 0.8f;
                laserFireEnd = false;
                isLaserFire = false;
            }
            
        }
    }
}
