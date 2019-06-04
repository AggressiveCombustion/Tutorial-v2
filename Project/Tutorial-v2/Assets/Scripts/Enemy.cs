using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PhysicsObject
{
    public GameObject sight;

    public int facing = 1;

    float deathFallTime = 0.75f;
    bool deathFall = false;
    float fallTime = 0;
    EnemyState startingState;
    Vector3 startingPoint;
    bool startingStationary;
    int startingDir;
    public bool startingPatrol;
    public int startingPatrolIndex;
    private Vector2 startingSize;
    private Vector2 startingBoxOffset;
    bool stunned = false;
    bool dead = false;

    public bool stationary = true;

    Vector2 hitDirection = Vector2.zero;
    Vector2 move = Vector2.zero;

    public float maxSpeed = 4;

    public Animator anim;
    public GameObject blood;
    public GameObject soundBlast;

    public bool patrol = false;
    public Transform[] patrolNodes;
    public int patrolIndex = 0;
    public float nodeDistance = 0.3f;
    public float nodeWaitTime = 0;
    public float elapsedNodeTime = 0;

    public bool suspicious = false;
    public Vector2 suspiciousPoint;

    public bool pursue = false;
    public float attackDistance = 1.0f;

    public bool returnToStart = false;

    public bool ranged = false;

    public GameObject question;
    public GameObject exclamation;
    public GameObject bloodParticle;
    public LineRenderer gunLine;

    public bool spawned = false;
    

    public bool target = false;

    public enum EnemyState
    {
        normal,
        track,
        shoot,
        melee,
        fall,
        knockback,
        dead
    }

    public EnemyState state = EnemyState.normal;


    // Start is called before the first frame update
    void Start()
    {
        
        SaveOriginalValues();
    }

    void SaveOriginalValues()
    {
        startingState = state;
        startingPoint = transform.position;
        startingStationary = stationary;
        startingDir = facing;
        startingPatrol = patrol;
        startingSize = GetComponent<BoxCollider2D>().size;
        startingPatrolIndex = patrolIndex;
        startingBoxOffset = GetComponent<BoxCollider2D>().offset;
    }

    public void Restart()
    {
        if(spawned)
        {
            Destroy(gameObject);
        }

        fallTime = 0;
        deathFall = false;
        state = EnemyState.normal;
        transform.position = startingPoint;
        stationary = startingStationary;
        facing = startingDir;
        
        suspicious = false;
        suspiciousPoint = Vector2.zero;
        pursue = false;

        GetComponent<BoxCollider2D>().size = startingSize;
        GetComponent<BoxCollider2D>().offset = startingBoxOffset;

        dead = false;

        anim.SetBool("running", false);
        anim.SetBool("damaged", false);
        anim.SetBool("fall", false);
        anim.SetBool("hang", false);
        anim.SetTrigger("restart");

        hitDirection = Vector2.zero;
        patrol = startingPatrol;
        patrolIndex = startingPatrolIndex;
    }

    // Update is called once per frame
    protected override void ComputeVelocity()
    {
        switch(state)
        {
            case EnemyState.normal:
                UpdateNormal();
                break;
            case EnemyState.track:
                break;
            case EnemyState.knockback:
                UpdateKnockback();
                break;
            case EnemyState.melee:
                break;
            case EnemyState.fall:
                break;
            case EnemyState.shoot:
                break;
            case EnemyState.dead:
                GetComponent<BoxCollider2D>().size = new Vector2(GetComponent<BoxCollider2D>().size.x,
                                                         0.2f);
                GetComponent<BoxCollider2D>().offset = new Vector2(0, -0.2f);
                break;
        }

        targetVelocity = move * maxSpeed;


        anim.SetBool("grounded", grounded);
        
    }

    new void Update()
    {
        if (move.x != 0)
        {
            facing = (int)Mathf.Sign(move.x);
        }
        
        anim.GetComponent<SpriteRenderer>().flipX = (facing == 1? false : true);
        anim.SetBool("running", move.x != 0 && grounded);
        base.Update();
    }

    void DieByFall()
    {
        if (dead)
            return;

        
        dead = true;

        GameManager.instance.FreezeFrame();
        Instantiate(bloodParticle, transform.position, Quaternion.Euler(0, 0, 0));

        // create blood splatter
        Instantiate(blood, transform.position + new Vector3(0, -0.75f, -0.1f), Quaternion.Euler(Random.Range(0, 360),
                                                                Random.Range(0, 360),
                                                                Random.Range(0, 360)));
        Instantiate(soundBlast, transform.position, Quaternion.Euler(0, 0, 0));
        // play death sound
        // shake screen
        state = EnemyState.dead;

        // alert others
        foreach (Enemy e in FindObjectsOfType<Enemy>())
        {
            if(e != this)
            {
                if(e.transform.position.y < transform.position.y + 2 &&
                    e.transform.position.y > transform.position.y - 2)
                {
                    if(Vector2.Distance(e.transform.position, transform.position) < 10)
                    {
                        e.suspicious = true;
                        e.suspiciousPoint = transform.position;

                        Instantiate(question, e.transform.position + new Vector3(0, 1.5f), Quaternion.Euler(0, 0, 0));
                    }
                    
                }
            }
        }
    }

    public void Die()
    {
        
        if (dead)
            return;
        
        move.x = 0;
        anim.SetTrigger("hit");
        dead = true;
        state = EnemyState.dead;
        Instantiate(blood, transform.position + new Vector3(0, -0.75f, -0.1f), Quaternion.Euler(Random.Range(0, 360),
                                                                Random.Range(0, 360),
                                                                Random.Range(0, 360)));
    }

    public void GetHit(Vector2 dir, float strength)
    {
        move = Vector2.zero;
        anim.SetBool("damaged", true);
        state = EnemyState.knockback;
        hitDirection = dir * strength;
        Timer t = new Timer(0.35f, ReturnToNormal);
        GameManager.instance.AddTimer(t, gameObject);
    }

    void UpdateNormal()
    {
        move = Vector2.zero;

        if (patrol)
        {
            Transform target = patrolNodes[patrolIndex];
            if (target.position.x < transform.position.x)
            {
                move.x = -maxSpeed;
            }
            else
            {
                move.x = maxSpeed;
            }

            if (Vector2.Distance(transform.position, target.position) < nodeDistance)
            {
                move = Vector2.zero;

                elapsedNodeTime += Time.deltaTime * GameManager.instance.speed;
                if (elapsedNodeTime > nodeWaitTime)
                {
                    elapsedNodeTime = 0;
                    patrolIndex += 1;

                    if (patrolIndex > patrolNodes.Length - 1)
                    {
                        patrolIndex = 0;
                    }
                }


            }

        }

        else if (suspicious) // something happened so go investigate
        {
            if(FindObjectOfType<PlayerController>().state == PlayerController.PlayerState.death)
            {
                suspiciousPoint = Vector2.zero;
                suspicious = false;
            }
            if (suspiciousPoint.x < transform.position.x)
            {
                move.x = -maxSpeed;
            }
            else
            {
                move.x = maxSpeed;
            }

            RaycastHit2D wallHit = Physics2D.Raycast(transform.position + new Vector3(1.0f * facing, 0),
                                             (Vector2.right * facing),
                                             1f);

            if(wallHit)
            {
                if(wallHit.transform.tag == "Ground")
                {
                    //suspiciousPoint = wallHit.point;
                    elapsedNodeTime += Time.deltaTime * GameManager.instance.speed;

                    if (elapsedNodeTime >= 3.0f)
                    {
                        // not suspicious anymore
                        suspicious = false;
                        suspiciousPoint = Vector2.zero;
                        elapsedNodeTime = 0;
                        returnToStart = true;
                    }
                }
            }

            if (Vector2.Distance(transform.position, suspiciousPoint) < nodeDistance)
            {
                move.x = 0;
                anim.SetBool("running", false);
                elapsedNodeTime += Time.deltaTime * GameManager.instance.speed;

                if (elapsedNodeTime >= 3.0f)
                {
                    // not suspicious anymore
                    suspicious = false;
                    suspiciousPoint = Vector2.zero;
                    elapsedNodeTime = 0;
                    returnToStart = true;
                }
            }
        }

        else if (pursue)
        {
            suspicious = false;
            suspiciousPoint = Vector2.zero;
            Transform target = FindObjectOfType<PlayerController>().transform;

            if (target.position.x < transform.position.x)
            {
                move.x = -maxSpeed * 1.5f;
            }
            else
            {
                move.x = maxSpeed * 1.5f;
            }

            if (Vector2.Distance(transform.position, target.position) < attackDistance)
            {
                move = Vector2.zero;

                if (ranged)
                {
                    state = EnemyState.shoot;
                    move.x = 0;
                    velocity.x = 0;
                    anim.SetTrigger("shoot");
                    Timer tBullet = new Timer(0.5f, ShootGun);
                    Timer tLine = new Timer(0.7f, DisableGunLine);
                    Timer tNormal = new Timer(1.25f, ReturnToNormal);

                    Debug.Log(tBullet.id);
                    Debug.Log(tNormal.id);

                    GameManager.instance.AddTimer(tBullet, gameObject);
                    GameManager.instance.AddTimer(tLine, gameObject);
                    GameManager.instance.AddTimer(tNormal, gameObject);
                }

                else
                {
                    anim.SetTrigger("sword");
                    state = EnemyState.melee;
                    Timer t = new Timer(1.5f, ReturnToNormal);
                    GameManager.instance.AddTimer(t, gameObject);
                }

                
                
            }
        }
        
        else if(returnToStart)
        {
            anim.SetBool("running", true);
            Vector2 target = startingPoint;

            if (target.x < transform.position.x)
            {
                move.x = -maxSpeed * 1.5f;
            }
            else
            {
                move.x = maxSpeed * 1.5f;
            }

            if (Vector2.Distance(transform.position, startingPoint) < nodeDistance)
            {
                move.x = 0;
                returnToStart = false;
                anim.SetBool("running", false);
            }
                
        }

        

        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = new Vector2(0, -0.2f + (i * 0.1f));

            RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(1.0f * facing, offset.y),
                                                 Vector2.right * facing,
                                                 30f);
            Vector3 endLine = transform.position + new Vector3(1.0f * facing, offset.y) + (Vector3.right * facing) * 30;
            if (hit && !pursue)
            {
                endLine = hit.point;
                //Debug.Log("Hit " + hit.transform.name);
                if (hit.transform.tag == "Player" && hit.transform.GetComponent<PlayerController>().state != PlayerController.PlayerState.death)
                {
                    suspicious = false;
                    suspiciousPoint = Vector2.zero;
                    pursue = true;
                    patrol = false;
                    Instantiate(exclamation, transform.position + new Vector3(0, 1.5f), Quaternion.Euler(0, 0, 0));

                }
            }

            Debug.DrawLine(transform.position + new Vector3(1.0f * facing, offset.y),
                           endLine);
        }
        

        // falling to death
        if (!grounded)
        {
            if(hitDirection != Vector2.zero)
            {
                patrol = false;
            }

            fallTime += Time.deltaTime * GameManager.instance.speed;
            if (fallTime > deathFallTime)
            {
                fallTime = 0;

                /*int r = Random.Range(0, 4);
                SfxManager.instance.PlaySFX(SfxManager.instance.screams[r], true);*/
                Debug.Log("AAAAA");
                deathFall = true;
                anim.SetBool("fall", true);
                
                
                
            }
        }
        else
        {
            fallTime = 0;
            if (deathFall && !dead)
            {
                DieByFall();
                anim.SetTrigger("death");
            }
        }
    }

    void UpdateKnockback()
    {
        patrol = false;
        pursue = false;
        move.x = hitDirection.x;
    }

    void UpdateFall()
    {

    }

    public void ChangeState(EnemyState newState)
    {
        state = newState;
    }

    public void SwordHit()
    {
        RaycastHit2D swordHit = Physics2D.Raycast(transform.position + new Vector3(0.5f * facing, 0), Vector2.right * facing, 1);
        if (swordHit)
        {
            //Debug.Log("Sword Hit <" + swordHit.transform.name + ">");
            if(swordHit.transform.tag == "Player" &&
                swordHit.transform.GetComponent<PlayerController>().state != PlayerController.PlayerState.death &&
                !swordHit.transform.GetComponent<PlayerController>().invulnerable)
            {
                Instantiate(bloodParticle, swordHit.point, Quaternion.Euler(0, 0, 0));
                GameManager.instance.FreezeFrame();
                swordHit.transform.GetComponent<PlayerController>().Die();
            }
        }
    }

    void ShootGun()
    {
        //instantiate effect
        Instantiate(soundBlast, transform.position + new Vector3(0.5f * facing, 0), Quaternion.Euler(0, 0, 0));
        // alert others
        foreach (Enemy e in FindObjectsOfType<Enemy>())
        {
            if (e != this)
            {
                if (e.transform.position.y < transform.position.y + 2 &&
                    e.transform.position.y > transform.position.y - 2)
                {
                    if (Vector2.Distance(e.transform.position, transform.position) < 20)
                    {
                        e.suspicious = true;
                        e.suspiciousPoint = transform.position;
                    }
                }
            }
        }
        // do raycast
        gunLine.SetPosition(0, transform.position + new Vector3(0.5f * facing, 0));
        gunLine.SetPosition(1, transform.position + new Vector3(100f * facing, 0));
        gunLine.enabled = true;
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;
        RaycastHit2D gunHit = Physics2D.Raycast(transform.position + new Vector3(0.5f * facing, 0), Vector2.right * facing, 100f, layerMask);
        if (gunHit)
        {
            gunLine.SetPosition(1, gunHit.point);
            Debug.Log("Shot something");
            Debug.Log(gunHit.transform.name);
            if (gunHit.transform.tag == "Player" &&
                gunHit.transform.GetComponent<PlayerController>().state != PlayerController.PlayerState.death &&
                !gunHit.transform.GetComponent<PlayerController>().invulnerable)
            {
                GameManager.instance.FreezeFrame();
                gunHit.transform.GetComponent<PlayerController>().Die();
                Instantiate(bloodParticle, gunHit.transform.position, Quaternion.Euler(0, 0, 0));
            }
        }
    }

    void DisableGunLine()
    {
        gunLine.enabled = false;
    }

    public void ReturnToNormal()
    {
        if (dead)
            return;
        ChangeState(EnemyState.normal);
        //returnToStart = true;
        pursue = false;
        anim.SetBool("damaged", false);

    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.transform.tag == "Explosion")
        {
            if (state != EnemyState.dead)
            {
                Instantiate(bloodParticle, transform.position, Quaternion.Euler(0, 0, 0));
                //GameManager.instance.FreezeFrame();
                Die();
            }

        }
    }
}
