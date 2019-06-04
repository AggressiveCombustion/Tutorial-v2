using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodParticle : MonoBehaviour
{
    public GameObject blood; 
    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        //Debug.Log("AAAA");
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
        
        int i = 0;

        while (i < numCollisionEvents)
        {
            Instantiate(blood, collisionEvents[i].intersection + new Vector3(0, 0, -0.1f), Quaternion.Euler(Random.Range(0, 360),
                                                                Random.Range(0, 360),
                                                                Random.Range(0, 360)));
            i++;
        }
    }
}
