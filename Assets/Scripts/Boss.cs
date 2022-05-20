using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    //전역 참조변수
    Rigidbody bossRig;
    Vector3 destPosition;
    Vector3 destAngleAxis;
    GameObject bossAni;
    GameObject bossRotate;
    Animator anim;
    //전역변수
    bool isBossMoving = false;
    bool isRotateDone = true;
    float remainAngle = 90f;

    // Start is called before the first frame update
    void Start()
    {
        bossRig = GetComponent<Rigidbody>();
        Debug.Log(Vector3.up);
        Debug.Log(transform.TransformDirection(Vector3.up));
        bossAni = transform.GetChild(0).gameObject;
        bossRotate = bossAni.transform.GetChild(0).gameObject;
        Debug.Log(bossRotate.name);
        anim = bossAni.GetComponent<Animator>();
        anim.SetBool("isFin", true);
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        BossMove();
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
            else
                return;

            Debug.Log(destPosition);
            Debug.Log(destAngleAxis);
            Debug.Log(transform.forward);
        }
    }

    //Boss가 움직일 방향 지정
    void SetDest(string dir) {
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
        isBossMoving = true;
        isRotateDone = false;
        remainAngle = 90.0f;        
    }

    //SetDest가 실행되면 발동, Boss를 움직임
    void BossMove() {
        if (isBossMoving) {
            
            if(transform.position == destPosition && isRotateDone) {
                
                isBossMoving = false;
                return;
            }
            
            //위치 (1초에 4 이동)
            if(transform.position != destPosition)
                transform.position = Vector3.MoveTowards(transform.position, destPosition, 4 * Time.deltaTime);

            //각도 (1초에 90도 회전)
            if (!isRotateDone) {
                float stepAngle = 90.0f * Time.deltaTime;
                if ((remainAngle - stepAngle) >= 0) {
                    bossRotate.transform.Rotate(destAngleAxis, stepAngle, Space.World);
                    remainAngle -= stepAngle;
                }
                    
                else {
                    bossRotate.transform.Rotate(destAngleAxis, remainAngle, Space.World);
                    remainAngle = 0;
                    isRotateDone = true;
                }
            }
        }
    }
}
