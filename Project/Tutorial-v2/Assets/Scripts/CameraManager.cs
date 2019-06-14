using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    public Transform cEstablishing;
    public Transform cPlayer;
    public Transform cZoom;

    //public CinemachineVirtualCamera current;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        Timer t = new Timer(1, CamFollowPlayer);
        GameManager.instance.AddTimer(t, gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CamEstablishingShot()
    {
        cEstablishing.gameObject.SetActive(true);
        cPlayer.gameObject.SetActive(false);
        cZoom.gameObject.SetActive(false);

        //current = cEstablishing.GetComponent<CinemachineVirtualCamera>();
    }

    public void CamFollowPlayer()
    {
        cEstablishing.gameObject.SetActive(false);
        cPlayer.gameObject.SetActive(true);
        cZoom.gameObject.SetActive(false);

        //current = cPlayer.GetComponent<CinemachineVirtualCamera>();
        SetFollowTarget(FindObjectOfType<PlayerController>().transform);
    }

    public void CamZoom()
    {
        cEstablishing.gameObject.SetActive(false);
        cPlayer.gameObject.SetActive(false);
        cZoom.gameObject.SetActive(true);

        //current = cZoom.GetComponent<CinemachineVirtualCamera>();
    }

    public void SetFollowTarget(Transform target)
    {
        //current.Follow = target;
    }

    public void ReturnToNormal()
    {
        CamFollowPlayer();
        //current.Follow = FindObjectOfType<PlayerController>().transform;
    }

    public void SetDutch()
    {
        float angle = Random.Range(20, 25);
        
        int mult = 0;
        while (mult == 0)
            mult = Random.Range(-1, 2);

        Debug.Log(mult * angle + "");
        //current.m_Lens.Dutch = angle * mult ;
    }
}
