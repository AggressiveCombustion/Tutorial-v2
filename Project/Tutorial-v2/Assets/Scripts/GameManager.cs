using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public float speed;
    float savedSpeed;

    public List<Timer> timers;
    List<Timer> timersToAdd;

    // ties timer ID to specific GameObject
    public Dictionary<string, GameObject> registry;

    

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        if (instance != this)
            Destroy(this);

        timers = new List<Timer>();
        timersToAdd = new List<Timer>();
        registry = new Dictionary<string, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.R))
        {
            DoRestart();
        }
        foreach(Timer t in timers)
        {
            if(!t.complete)
                t.Update();
        }

        // add in any new timers only after the existing one's are done updating
        foreach(Timer t in timersToAdd)
        {
            timers.Add(t);
        }

        timersToAdd.Clear();
    }

    public void AddTimer(Timer t, GameObject g)
    {
        timersToAdd.Add(t);
        // register timer to gameObject
        if(!registry.ContainsKey(t.id))
            registry.Add(t.id, g);
    }

    public void CameraShake()
    {

    }

    public void FreezeFrame()
    {
        savedSpeed = instance.speed;
        instance.speed = 0;
        Timer t = new Timer(0.2f, ReturnToSavedSpeed, true);
        instance.AddTimer(t, instance.gameObject);
    }

    public void ReturnToSavedSpeed()
    {
        instance.speed = savedSpeed;
    }

    public void ReturnToNormalSpeed()
    {
        instance.speed = 1;
    }

    public void RestartLevel()
    {
        // do restart vfx

        // delay restart
        DoRestart();
    }

    void DoRestart()
    {
        
    }

    public void ReturnToNormalTime()
    {
        speed = 1;
    }

}

public delegate void TimerFunction();

public class Timer
{
    public float duration;
    public float elapsed = 0;
    public TimerFunction action;
    public bool complete = false;
    bool ignoreSpeed = false;
    public string id = "";

    public Timer(float duration, TimerFunction action)
    {
        this.duration = duration;
        this.action = action;

        MakeRandomID();
    }

    public Timer (float duration, TimerFunction action, bool ignoreSpeed)
    {
        this.duration = duration;
        this.action = action;
        this.ignoreSpeed = ignoreSpeed;

        MakeRandomID();
    }

    public Timer(float duration, TimerFunction action, bool ignoreSpeed, string id)
    {
        this.duration = duration;
        this.action = action;
        this.ignoreSpeed = ignoreSpeed;
        this.id = id;
    }

    public void Update()
    {
        elapsed += Time.deltaTime * (ignoreSpeed ? 1 : GameManager.instance.speed);

        if (!GameManager.instance.registry.ContainsKey(id))
            return;

        // kill timer if the gameobject that created it no longer exists
        if (GameManager.instance.registry[id] == null)
            complete = true;

        if (elapsed >= duration && !complete)
        {
            action();
            complete = true;
        }

        if(complete && GameManager.instance.registry[id] != null)
        {
            GameManager.instance.registry.Remove(id);
        }

        
    }

    void MakeRandomID()
    {
        string text = Time.realtimeSinceStartup.ToString() +
                      Random.Range(0, 1000.0f) +
                      GameManager.instance.registry.Count;

        id = text;
    }
}
