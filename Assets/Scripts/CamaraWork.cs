using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraWork : MonoBehaviour
{
    //참조변수
    GameObject player;
    //변수
    public float yRevision = 15f;
    public float zRevision = -8f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + yRevision, player.transform.position.z + zRevision);
    }
}
