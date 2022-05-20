using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //참조변수
    GameObject player;
    Player playerScript;
    //변수
    int layerMask;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.Find("Player");
        playerScript = player.GetComponent<Player>();
        layerMask = 1 << LayerMask.NameToLayer("Floor");
    }

    // Update is called once per frame
    void Update()
    {
        ManageFire();
    }

    void ManageFire() {
        if (Input.GetMouseButton(1)) {
            playerScript.SetAimDirection(GetDestOfArrow());
        }

        //ready
        if (Input.GetMouseButtonDown(1)) {
            playerScript.ChangeReadyState();
        }
        else if (Input.GetMouseButtonUp(1)) {
            playerScript.ChangeReadyState();
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
}
