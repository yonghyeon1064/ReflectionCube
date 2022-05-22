using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWork : MonoBehaviour
{
    //참조변수
    GameObject player;
    //변수
    public float yRevision = 15f;
    public float zRevision = -8f;
    Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
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
    public float shakeAmount = 5f;
    public void ShakeCameraForTime(float time) {
        shakeTime = time;
    }
}
