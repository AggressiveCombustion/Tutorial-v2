using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public Transform parent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoSwordAttack()
    {
        if(parent.GetComponent<Enemy>())
        {
            parent.GetComponent<Enemy>().SwordHit();
        }

        else if (parent.GetComponent<PlayerController>())
        {
            parent.GetComponent<PlayerController>().SwordHit();
        }
    }
}
