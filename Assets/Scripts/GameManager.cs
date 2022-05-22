using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //참조변수
    GameObject player;
    Player playerScript;
    GameObject boss;
    Boss bossScript;
    public new GameObject camera;
    CameraWork cameraWork;
    public GameObject gameOverText;
    public GameObject gameClearText;
    public GameObject restartButton;
    public GameObject replayButton;
    public GameObject quitButton;

    //변수
    public bool gameActive;
    int layerMask;

    //sound
    AudioSource soundPlayer;
    public AudioClip arrowBackSound;

    // Start is called before the first frame update
    void Awake()
    {
        soundPlayer = GetComponent<AudioSource>();
        soundPlayer.loop = false;

        player = GameObject.Find("Player");
        playerScript = player.GetComponent<Player>();
        boss = GameObject.Find("Boss");
        bossScript = boss.GetComponent<Boss>();
        cameraWork = camera.GetComponent<CameraWork>();

        layerMask = 1 << LayerMask.NameToLayer("Floor");
        
        gameOverText.SetActive(false);
        gameClearText.SetActive(false);
        restartButton.SetActive(false);
        replayButton.SetActive(false);
        quitButton.SetActive(false);
        
        gameActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        CheckEsc();
        if (gameActive)
            ManageFire();
    }

    public void Clear() {
        Debug.Log("Clear");
        cameraWork.SetSound("fight", false);
        bossScript.BossDied();
        gameClearText.SetActive(true);
        replayButton.SetActive(true);
    }

    public void GameOver() {
        //player 위치 고정
        player.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        playerScript.SetIsTrigger();

        //소리
        playerScript.SetSound("walk", false);
        cameraWork.SetSound("fight", false);
        cameraWork.SetSound("gameOver", true);

        //UI 처리 및 게임 정지
        gameOverText.SetActive(true);
        restartButton.SetActive(true);
        gameActive = false;
    }

    void ManageFire() {
        if (Input.GetMouseButton(1)) {
            playerScript.SetAimDirection(GetDestOfArrow());
        }

        //ready
        if (Input.GetMouseButtonDown(1)) {
            playerScript.ChangeReadyState("Down");
        }
        else if (Input.GetMouseButtonUp(1)) {
            playerScript.ChangeReadyState("Up");
        }

        //fire
        if(playerScript.ReturnReadyState() && Input.GetMouseButtonDown(0)) {
            playerScript.FireArrow();
        }
    }

    //player에서 mouse 위치 까지의 vector3 반환
    Vector3 GetDestOfArrow() {
        // screen 상의 좌표를 3d space 공간의 카메라에서 발사되는 광선으로 바꿔준다
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitResult;
        if (Physics.Raycast(ray, out hitResult, 100.0f, layerMask)) {
            Vector3 mouseDir = new Vector3(hitResult.point.x, player.transform.position.y, hitResult.point.z) - player.transform.position;
            return mouseDir;
        }
        else return new Vector3(0, 0, 0);
    }

    public void ArrowBackSound() {
        soundPlayer.clip = arrowBackSound;
        soundPlayer.Play();
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    bool isThereQuitButton = false;
    void CheckEsc() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (!isThereQuitButton) {
                isThereQuitButton = true;
                quitButton.SetActive(true);
            }
            else {
                isThereQuitButton = false;
                quitButton.SetActive(false);
            }   
        }
    }

    public void QuitGame() {
        Application.Quit();
    }
}
