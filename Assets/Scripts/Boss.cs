using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    //전역 참조변수
    Rigidbody bossRig;
    //전역변수
    bool isBossMoving = false;

    // Start is called before the first frame update
    void Start()
    {
        bossRig = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        BossMove();
    }

    void BossMove() {
        // -1 ~ 1 사이 값 입력받기
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        Vector3 direction;
        
        if (Mathf.Abs(inputX) > Mathf.Abs(inputZ))
            direction = new Vector3(1 * Mathf.Sign(inputX), 0, 0);
        else if (Mathf.Abs(inputX) < Mathf.Abs(inputZ))
            direction = new Vector3(0, 0, 1 * Mathf.Sign(inputZ));
        else
            return;

        if (!isBossMoving) {
            //이동
            Rotate(direction);
        }
    }

    void Rotate(Vector3 direction) {
        //isBossMoving = true;
        Debug.Log("Lotate: " + direction);
    }
}
