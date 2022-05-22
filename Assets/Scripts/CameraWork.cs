using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWork : MonoBehaviour
{
    //참조변수
    GameObject player;
    public GameObject gameM;
    GameManager gameManager;

    //변수
    public float yRevision = 15f;
    public float zRevision = -8f;
    Vector3 initialPosition;

    //sound
    AudioSource soundPlayer;
    public AudioClip idleBgm;
    public AudioClip fightBgm;
    public AudioClip gameOverSound;
    public AudioClip gameClearSound;
    /* info of fightBgm
     Epic Cinematic Trailer | ELITE by Alex-Productions | https://www.youtube.com/channel/UCx0_M61F81Nfb-BRXE-SeVA
    Music promoted by https://www.chosic.com/free-music/all/
    Creative Commons CC BY 3.0
    https://creativecommons.org/licenses/by/3.0/
 
     */

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        gameManager = gameM.GetComponent<GameManager>();

        soundPlayer = GetComponent<AudioSource>();
        soundPlayer.loop = true;

        SetSound("idle", true);
    }

    // Update is called once per frame
    void Update()
    {
        initialPosition = new Vector3(player.transform.position.x, player.transform.position.y + yRevision, player.transform.position.z + zRevision);
        if(shakeTime > 0) {
            transform.position = Random.insideUnitSphere * shakeAmount + initialPosition;
            shakeTime -= Time.deltaTime;
        }
        else {
            shakeTime = 0f;
            transform.position = initialPosition;
        }
    }

    //카메라 흔들기
    float shakeTime = 0f;
    public float shakeAmount = 0.2f;
    public void ShakeCameraForTime(float time) {
        shakeTime = time;
    }

    //Bgm 관리
    public void SetSound(string name, bool onOff) {
        if (gameManager.gameActive) {
            if (name == "idle") {
                if (onOff) {
                    soundPlayer.loop = true;
                    soundPlayer.clip = idleBgm;
                    soundPlayer.Play();
                }
                else {
                    soundPlayer.Stop();
                }
            }
            else if (name == "fight") {
                if (onOff) {
                    soundPlayer.loop = true;
                    soundPlayer.clip = fightBgm;
                    soundPlayer.Play();
                }
                else {
                    soundPlayer.Stop();
                }
            }
            else if (name == "gameOver") {
                if (onOff) {
                    soundPlayer.loop = false;
                    soundPlayer.clip = gameOverSound;
                    soundPlayer.Play();
                }
                else {
                    soundPlayer.Stop();
                }
            }
            else if (name == "clear") {
                if (onOff) {
                    soundPlayer.loop = false;
                    soundPlayer.clip = gameClearSound;
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
}
