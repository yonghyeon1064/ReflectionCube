using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class delayTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("print1");
        StartCoroutine(delay());
        Debug.Log("print2");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator delay() {
        Debug.Log("print in 1");
        yield return new WaitForSeconds(2f);
        Debug.Log("print in 2");
    }
}
