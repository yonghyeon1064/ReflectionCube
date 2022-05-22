using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Player : MonoBehaviour
{
    //전역 참조변수
    public GameObject gameM;
    GameManager gameManager;
    GameObject boss;
    Boss bossScript;
    Rigidbody playerRig;
    public GameObject arrow;
    public GameObject indicator;
    public GameObject body;
    public GameObject face;
    GameObject currentArrow;
    GameObject currentIndicator;
    Vector3 indicatorDirection;
    Vector3 aimDirection;
    System.Diagnostics.Stopwatch watch;
    LineRenderer aimLine;

    //전역 변수
    public float moveSpeed;
    public float walkSpeed = 8f;
    public float chargingSpeed = 5f;
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
        gameManager = gameM.GetComponent<GameManager>();

        boss = GameObject.Find("Boss");
        bossScript = boss.GetComponent<Boss>();

        playerRig = GetComponent<Rigidbody>();
        watch = new System.Diagnostics.Stopwatch();

        moveSpeed = walkSpeed;

        //Line renderer
        aimLine = GetComponent<LineRenderer>();
        aimLine.startColor = Color.yellow;
        aimLine.endColor = Color.yellow;
        aimLine.startWidth = 0.2f;
        aimLine.endWidth = 0.2f;
        aimLine.positionCount = 0;

        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameActive) {
            PlayerMove();
            ManageChargingArrow();
            ManageIndicator();
            DrawAimLine();
        }
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
    public void ChangeReadyState(string upDown) {
        if (!isReadyToFire && upDown == "Down") {
            if (isFired)
                return;

            isReadyToFire = true;
            moveSpeed = chargingSpeed;
            watch.Start();
            currentArrow = Instantiate(arrow, transform.position + aimDirection.normalized + new Vector3(0, 0.5f, 0), Quaternion.identity); //생성할 instance, 생성 위치, 생성 각도
        }
        else if (isReadyToFire && upDown == "Up" && !isFired) {
            isReadyToFire = false;
            moveSpeed = walkSpeed;
            watch.Stop();
            watch.Reset();
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

        //chargeRate 계산
        float chargeRate;
        if (chargeTime >= 2f)
            chargeRate = 1f;
        else
            chargeRate = (chargeTime) * 0.5f;
        //UnityEngine.Debug.Log(chargeRate);
        //UnityEngine.Debug.Log(normalArrowForce + bonusArrowForce * chargeRate);

        //발사
        currentArrow.GetComponent<SphereCollider>().isTrigger = false;
        currentArrow.GetComponent<Projectile>().ShootArrow(aimDirection, normalArrowForce + bonusArrowForce * chargeRate);
        
        isReadyToFire = false;
        isFired = true;
        moveSpeed = walkSpeed;

        watch.Reset();

        //indicator 표시
        currentIndicator = Instantiate(indicator, transform.position + aimDirection.normalized + new Vector3(0, 0.5f, 0), transform.rotation); //생성할 instance, 생성 위치, 생성 각도

        //조준선 지우기
        if (aimLine.positionCount == 2) {
            aimLine.SetPosition(0, Vector3.zero);
            aimLine.SetPosition(1, Vector3.zero);
            aimLine.positionCount = 0;
        }
    }

    //Game Manager가 호출
    public void SetAimDirection(Vector3 aim) {
        aimDirection = aim;
    }

    //Game Manager가 호출
    public bool ReturnReadyState() {
        return isReadyToFire;
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

    private void OnTriggerEnter(Collider col) {
        if (gameManager.gameActive) {
            if (col.gameObject.tag == "Laser" || (col.gameObject.tag == "Boss" && bossScript.curState == Boss.CurrentState.attack)) {
                gameManager.GameOver();
            }
        }
    }

    private void OnCollisionEnter(Collision col) {
        if (gameManager.gameActive) {
            if(col.gameObject.tag == "FriendlyArrow") {
                GetArrowBack();
            }
        }
    }

    //Arrow 회수 함수
    public void GetArrowBack() {
        Destroy(currentArrow);
        Destroy(currentIndicator);
        isFired = false;
    }

    //colliider들의 isTrigger true로 바꾸는 함수 (GameOver가 호출)
    public void SetIsTrigger() {
        body.GetComponent<Collider>().isTrigger = true;
        face.GetComponent<Collider>().isTrigger = true;
    }

    
}
