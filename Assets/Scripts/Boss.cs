using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    //전역 참조변수
    public new GameObject camera;
    CameraWork cameraWork;
    public GameObject gameM;
    GameManager gameManager;
    public GameObject nucleus;
    public GameObject subNucl;
    public GameObject[] bossBody = new GameObject[8];
    public GameObject[] bossWall = new GameObject[10];
    public GameObject[] bossSubWall = new GameObject[4];
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

    //sound
    AudioSource soundPlayer;
    public AudioClip impactSound;
    public AudioClip laserSound;
    public AudioClip wakeUpSound;
    public AudioClip deadSound;

    //color
    Renderer[] nucleusColor = new Renderer[2];
    Renderer[] bodyColor = new Renderer[8];
    Renderer[] wallColor = new Renderer[9];
    Renderer[] subWallColor = new Renderer[4];

    Color[] colorForNucl = new Color[3];
    Color[] colorForBody = new Color[3];
    Color[] colorForWall = new Color[3];
    Color[] colorForSubWall = new Color[3];


    // Start is called before the first frame update
    void Start()
    {
        gameManager = gameM.GetComponent<GameManager>();
        cameraWork = camera.GetComponent<CameraWork>();
        player = GameObject.Find("Player");
        

        soundPlayer = GetComponent<AudioSource>();
        soundPlayer.loop = false;

        bossAni = transform.GetChild(0).gameObject;
        bossRotate = bossAni.transform.GetChild(0).gameObject;
        bossRotate.GetComponent<Collider>().enabled = false;
        anim = bossAni.GetComponent<Animator>();
        anim.SetBool("isFin", true);

        coroutine = RepeatChasing();
        StartCoroutine(FindPlayer());

        //색깔
        SetColorTemplate();
        SetRenderer();
        ColorChange("Blue");

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

                //Boss 일어나는 모션 및 사운드
                cameraWork.SetSound("idle", false);
                SetSound("wakeUp", true);
                ColorChange("Red");

                yield return new WaitForSeconds(2.5f);

                isBossAttacked = false;
                StartCoroutine(coroutine);
            }
        }
    }

    bool playBgm = true;
    public float waitTime = 0.4f;
    IEnumerator RepeatChasing() {
        if (playBgm) {
            cameraWork.SetSound("fight", true);
            playBgm = false;
        }
        while (gameManager.gameActive) {
            yield return new WaitForSeconds(waitTime);
            ChasingPlayer();
        }
    }

    public void BossDied() {
        StopCoroutine(coroutine);
        curState = CurrentState.dead;

        //Boss 죽는 모션 및 사운드
        StartCoroutine(BossDeadMotion());
    }

    IEnumerator BossDeadMotion() {
        SetSound("die", true);
        ColorChange("Blue");
        yield return new WaitForSeconds(1f);
        cameraWork.SetSound("idle", true);
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
            ColorChange("Red");
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
                ColorChange("Yellow");
                SetSound("impact", true);
                cameraWork.ShakeCameraForTime(shakeTime);
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
                    laserMotionTime = 1.3f;
                    laserScale.transform.localScale = new Vector3(1, 10, 1); //아래방향 발사
                    anim.SetTrigger("laserDown");
                    isAirRotate = true;
                }
                    
                else
                    laserScale.transform.localScale = new Vector3(1, hitResult.distance/2, 1);
            }
            else
                laserScale.transform.localScale = new Vector3(1, 10, 1); //위 방향 발사
            SetSound("laser", true);
            isLaserFire = true;
            cameraWork.ShakeCameraForTime(shakeTime);
        }
    }

    //레이저 발사 관리
    void calculateLaserTime() {
        if (isLaserFire) {
            //레이저 발사 경과시간 계산
            currentTimeSum += Time.deltaTime;


            //시간경과 시 레이저 발사 종료
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
                        SetSound("impact", true);
                        cameraWork.ShakeCameraForTime(shakeTime);
                        isAirRotate = false;
                        canStartAirRotate = false;
                    }
                }
            }
            
            //시간경과 및 회전 종료 시 레이저 모션 종료
            if (currentTimeSum >= laserMotionTime && !isAirRotate) {
                currentTimeSum = 0.0f;
                laserTime = 0.8f;
                laserMotionTime = 1f;
                laserFireEnd = false;
                isLaserFire = false;
                moveCount = 4f;
            }
            
        }
    }

    //sound 관리
    void SetSound(string name, bool onOff) {
        if (gameManager.gameActive) {
            if (name == "impact") {
                if (onOff) {
                    soundPlayer.clip = impactSound;
                    soundPlayer.Play();
                }
                else {
                    soundPlayer.Stop();
                }
            }
            else if (name == "laser") {
                if (onOff) {
                    soundPlayer.clip = laserSound;
                    soundPlayer.Play();
                }
                else {
                    soundPlayer.Stop();
                }
            }
            else if (name == "wakeUp") {
                if (onOff) {
                    soundPlayer.clip = wakeUpSound;
                    soundPlayer.Play();
                }
                else {
                    soundPlayer.Stop();
                }
            }
            else if (name == "die") {
                if (onOff) {
                    soundPlayer.clip = deadSound;
                    soundPlayer.Play();
                }
                else {
                    soundPlayer.Stop();
                }
            }
            else
                UnityEngine.Debug.Log("Wrong Input in SetSound");
        }
    }

    void SetColorTemplate() {
        //Nucl
        ColorUtility.TryParseHtmlString("#007FFF", out colorForNucl[0]);
        ColorUtility.TryParseHtmlString("#FF3135", out colorForNucl[1]);
        ColorUtility.TryParseHtmlString("#B3B31C", out colorForNucl[2]);

        //Body
        ColorUtility.TryParseHtmlString("#1D309C", out colorForBody[0]);
        ColorUtility.TryParseHtmlString("#9C1D22", out colorForBody[1]);
        ColorUtility.TryParseHtmlString("#B46807", out colorForBody[2]);

        //Wall
        ColorUtility.TryParseHtmlString("#06236A", out colorForWall[0]);
        ColorUtility.TryParseHtmlString("#6A061C", out colorForWall[1]);
        ColorUtility.TryParseHtmlString("#9C5C1D", out colorForWall[2]);

        //SubWall
        ColorUtility.TryParseHtmlString("#101143", out colorForSubWall[0]);
        ColorUtility.TryParseHtmlString("#431110", out colorForSubWall[1]);
        ColorUtility.TryParseHtmlString("#432C10", out colorForSubWall[2]);
    }

    void SetRenderer() {
        nucleusColor[0] = nucleus.GetComponent<Renderer>();
        nucleusColor[1] = subNucl.GetComponent<Renderer>();
        for (int i=0; i<9; i++) {
            wallColor[i] = bossWall[i].GetComponent<Renderer>();
            if(i < 8)
                bodyColor[i] = bossBody[i].GetComponent<Renderer>();
            if (i < 4)
                subWallColor[i] = bossSubWall[i].GetComponent<Renderer>();
        }
    }

    void ColorChange(string color) {
        if (color == "Blue") {
            nucleusColor[0].material.color = colorForNucl[0];
            nucleusColor[1].material.color = colorForNucl[0];
            for (int i = 0; i < 9; i++) {
                wallColor[i].material.color = colorForWall[0];
                if (i < 8)
                    bodyColor[i].material.color = colorForBody[0];
                if (i < 4)
                    subWallColor[i].material.color = colorForSubWall[0];
            }
        }
        else if (color == "Red") {
            nucleusColor[0].material.color = colorForNucl[1];
            nucleusColor[1].material.color = colorForNucl[1];
            for (int i=0; i<9; i++) {
                wallColor[i].material.color = colorForWall[1];
                if (i < 8)
                    bodyColor[i].material.color = colorForBody[1];
                if (i < 4)
                    subWallColor[i].material.color = colorForSubWall[1];
            }
        }
        else if(color == "Yellow") {
            nucleusColor[0].material.color = colorForNucl[2];
            nucleusColor[1].material.color = colorForNucl[2];
            for (int i = 0; i < 9; i++) {
                wallColor[i].material.color = colorForWall[2];
                if (i < 8)
                    bodyColor[i].material.color = colorForBody[2];
                if (i < 4)
                    subWallColor[i].material.color = colorForSubWall[2];
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
