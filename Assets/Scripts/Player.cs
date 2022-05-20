using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Player : MonoBehaviour
{
    //전역 참조변수
    Rigidbody playerRig;
    public GameObject arrow;
    public GameObject indicator;
    GameObject currentArrow;
    GameObject currentIndicator;
    Vector3 indicatorDirection;
    Vector3 aimDirection;
    System.Diagnostics.Stopwatch watch;
    LineRenderer aimLine;

    //전역 변수
    public float moveSpeed = 5f;
    float inputX = 0;
    float inputZ = 0;
    bool isReadyToFire = false;
    float chargeTime = 0;
    public float normalArrowForce = 200f;
    public float bonusArrowForce = 800f;
    public float fullChargeTime = 2f;
    bool isFired = false;
    public LayerMask layerMask;

    // Start is called before the first frame update
    void Start()
    {
        playerRig = GetComponent<Rigidbody>();
        watch = new System.Diagnostics.Stopwatch();

        //Line renderer
        aimLine = GetComponent<LineRenderer>();
        aimLine.startColor = new Color(1, 0, 0, 0.5f);
        aimLine.endColor = new Color(1, 0, 0, 0.5f);
        aimLine.startWidth = 0.2f;
        aimLine.endWidth = 0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMove();
        ManageChargingArrow();
        ManageIndicator();
        DrawAimLine();
    }

    void PlayerMove() {
        // -1 ~ 1 사이 값 입력받기
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        //플레이어 이동
        Vector3 velocity = new Vector3(inputX, 0, inputZ);
        if(velocity != new Vector3(0, 0, 0)) {
            //이동
            playerRig.velocity = velocity.normalized * moveSpeed * Mathf.Max(Mathf.Pow(inputX, 2), Mathf.Pow(inputZ, 2));

            //시점전환 (조준 안하는 동안)
            if(velocity.magnitude > 0.8 && !isReadyToFire)
                transform.rotation = Quaternion.LookRotation(velocity);
        }
        
        //시점전환 (조준 하는 동안)
        if(isReadyToFire)
            transform.rotation = Quaternion.LookRotation(aimDirection);
    }

    //화살 발사 준비 (Game Manager가 호출)
    public void ChangeReadyState() {
        if (!isReadyToFire) {
            if (isFired)
                return;

            isReadyToFire = true;
            watch.Start();
            currentArrow = Instantiate(arrow, transform.position + aimDirection.normalized + new Vector3(0, 0.5f, 0), Quaternion.identity); //생성할 instance, 생성 위치, 생성 각도
        }
        else {
            isReadyToFire = false;
            watch.Stop();
            watch.Reset();
            if(!isFired)
                Destroy(currentArrow);

            //조준선 지우기
            if (aimLine.positionCount == 2) {
                aimLine.SetPosition(0, Vector3.zero);
                aimLine.SetPosition(1, Vector3.zero);
                aimLine.positionCount = 0;
            }
        }            
    }

    //화살 발사 (Game Manager가 호출)
    public void FireArrow() {
        watch.Stop();
        chargeTime = watch.ElapsedMilliseconds / 1000f;
        //UnityEngine.Debug.Log(chargeTime + " s");

        float chargeRate;
        if (chargeTime >= 2f)
            chargeRate = 1f;
        else
            chargeRate = (chargeTime) * 0.5f;

        currentArrow.GetComponent<SphereCollider>().isTrigger = false;
        //UnityEngine.Debug.Log(chargeRate);
        //UnityEngine.Debug.Log(normalArrowForce + bonusArrowForce * chargeRate);
        currentArrow.GetComponent<Projectile>().ShootArrow(aimDirection, normalArrowForce + bonusArrowForce * chargeRate);
        isFired = true;

        watch.Reset();
        watch.Start();

        //indicator 표시
        currentIndicator = Instantiate(indicator, transform.position + aimDirection.normalized + new Vector3(0, 0.5f, 0), transform.rotation); //생성할 instance, 생성 위치, 생성 각도

        //조준선 지우기
        if (aimLine.positionCount == 2) {
            aimLine.SetPosition(0, Vector3.zero);
            aimLine.SetPosition(1, Vector3.zero);
            aimLine.positionCount = 0;
        }
    }

    //차징 중인 arrow 위치 관리
    void ManageChargingArrow() {
        if (isReadyToFire && !isFired)
            currentArrow.transform.position = transform.position + aimDirection.normalized + new Vector3(0, 0.5f, 0);
    }

    // 생긴 Indicator 위치 관리
    void ManageIndicator() {
        if (isFired) {
            indicatorDirection = currentArrow.transform.position - transform.position;
            currentIndicator.transform.position = transform.position + indicatorDirection.normalized + new Vector3(0, 0.5f, 0);
            currentIndicator.transform.rotation = Quaternion.LookRotation(indicatorDirection);
        }
    }

    // 조준, 발사안함 상태일때 조준선 그리기
    void DrawAimLine() {
        if (isReadyToFire && !isFired) {
            if(aimLine.positionCount == 2) {
                aimLine.SetPosition(0, Vector3.zero);
                aimLine.SetPosition(1, Vector3.zero);
                aimLine.positionCount = 0;
            }
            RaycastHit hitResult;
            if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), transform.forward, out hitResult, 100f, layerMask)) {
                aimLine.positionCount = 2;
                aimLine.SetPosition(0, currentArrow.transform.position);
                aimLine.SetPosition(1, hitResult.point);
            }
        }        
    }

    private void OnCollisionEnter(Collision col) {
        GetArrowBack(col);
    }

    //Arrow 회수 함수
    void GetArrowBack(Collision col) {
        if (col.gameObject.tag == "FriendlyArrow") {
            Destroy(currentArrow);
            Destroy(currentIndicator);
            isFired = false;
        }
    }

    public void SetAimDirection(Vector3 aim) {
        aimDirection = aim;
    }

    public bool ReturnReadyState() {
        return isReadyToFire;
    }
}
