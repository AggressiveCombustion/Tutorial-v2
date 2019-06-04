using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolNode : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
