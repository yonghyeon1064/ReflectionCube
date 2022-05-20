using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraWork : MonoBehaviour
{
    //참조변수
    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 19, player.transform.position.z - 11);
    }
}
