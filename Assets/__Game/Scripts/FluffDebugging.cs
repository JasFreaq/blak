using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluffDebugging : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            Time.timeScale = 0.2f;
    }
}
