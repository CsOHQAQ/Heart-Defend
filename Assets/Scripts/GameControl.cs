using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameControl:MonoBehaviour
{
    private static GameControl _gameControl=null;
    public static GameControl Game
    {
        get
        {
            if( _gameControl  is null)
            {
                //_gameControl = new GameControl();
                GameObject.Find("GameControl");//Really Ugly 
                _gameControl.Init();                
            }
            return _gameControl;
        }
    }

    [HideInInspector]
    public Moon moon=null;
    [HideInInspector]
    public PlayerControl player;
    [HideInInspector]
    public CameraControl cam;
    [HideInInspector]
    public float MapHeight;
    [HideInInspector]
    public float MapWidth;
    public bool isTotorial;
    public TotorialController Totorial;

    public GameObject StarPrefab;
    public int StarNum;
    public float CloestObjDistance;
    [HideInInspector]
    public List<Star> StarList;
    [HideInInspector]
    public List<DustCloud> DustCloudList;

    public int CloudNum;
    public GameObject DustCloudPrefab;
    public List<GameObject> DustCloudPrefabs;
    public List<AudioClip> AudioClipList;

    public GameObject StarPingPrefab;
    public List<StarPing> StarPingList;
    
    SpriteRenderer background;
    Light2D globalLight;

    public float FullMoonAnimationTime;
    float bgFullMoonIndex;
    float lFullMoonIndex;
    public bool isFullMoon;

    private void Awake()
    {
        _gameControl = this;
        _gameControl.Init();
    }
    private void Init()
    {
        if (GameObject.Find("Moon") is not null)
            moon = GameObject.Find("Moon").GetComponent<Moon>();
        else
            Debug.LogError("Moon Not Found!");

        if (GameObject.Find("Player") is not null)
            player= GameObject.Find("Player").GetComponent<PlayerControl>();
        else
            Debug.LogError("Player Not Found!");

        cam = GameObject.Find("Main Camera").GetComponent<CameraControl>();

        background = GameObject.Find("Background").GetComponent<SpriteRenderer>();
        globalLight = GameObject.Find("Global Light 2D").GetComponent<Light2D>();

        if (background is not null)
        {
            MapHeight =background.bounds.size.y;
            MapWidth = background.bounds.size.x;
        }
        DustCloudList = new List<DustCloud>();
        GenerateCloud(CloudNum);
        StarList = new List<Star>();
        GenerateStars(StarNum);
        StarPingList = new List<StarPing>();
        Transform canvas = GameObject.Find("Canvas").transform;
        foreach (var star in StarList)
        {
            StarPing ping = Instantiate(StarPingPrefab,canvas).GetComponent<StarPing>();
            ping.Init(moon,star);
            StarPingList.Add(ping);
        }


        if (isTotorial)
            Totorial = GameObject.Find("TotorialController").GetComponent<TotorialController>();

        isFullMoon = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isFullMoon) 
        {
            globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, 1,lFullMoonIndex* Time.deltaTime);
            background.color = new Color(1,1,1,Mathf.MoveTowards(background.color.a,1,bgFullMoonIndex*Time.deltaTime));
        }
    }

    void GenerateCloud(int generateNum)
    {
        if (isTotorial)
        {
            foreach (var item in GameObject.FindGameObjectsWithTag("DustCloud"))
            {
                DustCloudList.Clear();
                item.GetComponent<DustCloud>().Init();
                DustCloudList.Add(item.GetComponent<DustCloud>());
            }
            return;
        }

        Randomer rnd = new Randomer();
        DustCloudList.Clear();

        int loopCount = 0;

        while (loopCount <= 10000 && DustCloudList.Count < generateNum)
        {
            Vector2 nextPos = new Vector2((rnd.nextFloat() - 0.5f) * MapWidth * 0.8f, (rnd.nextFloat() - 0.5f) * MapHeight * 0.8f);
            bool flag = true;
            foreach (DustCloud cloud in DustCloudList)
            {
                if (Vector2.Distance(cloud.transform.position, nextPos) < CloestObjDistance)
                {
                    flag = false; break;
                }
            }

            if (Vector2.Distance(moon.transform.position, nextPos) < CloestObjDistance)
            {
                flag = false;
            }
            if (!flag)
                continue;

            DustCloud nextCloud = null;

            if (DustCloudPrefabs.Count > 0)
            {
                int cId = rnd.nextInt(DustCloudPrefabs.Count);
                nextCloud = Instantiate(DustCloudPrefabs[cId]).GetComponent<DustCloud>();
            }
            else
            {
                nextCloud = Instantiate(DustCloudPrefab).GetComponent<DustCloud>();
            }            
            nextCloud.transform.position = nextPos;
            nextCloud.Init();
            DustCloudList.Add(nextCloud);
        }
        if (DustCloudList.Count < generateNum)
            Debug.LogError("Not enough Cloud Generated!");
    }

    void GenerateStars(int generateNum)
    {
        Randomer rnd = new Randomer();

        if (isTotorial)
        {
            foreach (var item in GameObject.FindGameObjectsWithTag("Star"))
            {
                StarList.Clear();
                item.GetComponent<Star>().Init(AudioClipList[rnd.nextInt(AudioClipList.Count)]);
                StarList.Add(item.GetComponent<Star>());
            }
            return;
        }

        StarList.Clear();

        int loopCount = 0;

        while (loopCount<=10000&&StarList.Count<generateNum)
        {
            Vector2 nextPos=new Vector2((rnd.nextFloat()-0.5f)*MapWidth*0.8f, (rnd.nextFloat() - 0.5f) * MapHeight * 0.8f);
            bool flag = true;

            foreach (DustCloud cloud in DustCloudList)
            {
                if (Vector2.Distance(cloud.transform.position, nextPos) < CloestObjDistance)
                {
                    flag = false; break;
                }
            }
            foreach (Star star in StarList) 
            {
                if (Vector2.Distance(star.transform.position,nextPos)<CloestObjDistance)
                {
                    flag= false; break;
                }
            }
            if (Vector2.Distance(moon.transform.position, nextPos) < CloestObjDistance)
            {
                flag = false;
            }

            if (!flag)
                continue;

            Star nextStar=Instantiate(StarPrefab).GetComponent<Star>();             
            nextStar.transform.position = nextPos;
            nextStar.Init(AudioClipList[rnd.nextInt(AudioClipList.Count)]);
            StarList.Add(nextStar);
        }
        if (StarList.Count < generateNum)
            Debug.LogError("Not enough Star Generated!");
    }

    public void FullMoon()
    {
        cam.FullMoon();
        bgFullMoonIndex = (1f - background.color.a) / FullMoonAnimationTime;
        lFullMoonIndex = (1f - globalLight.intensity) / FullMoonAnimationTime;
        isFullMoon = true;
        moon.transform.GetComponent<AudioSource>().Stop();
        moon.transform.Find("moonReprise").GetComponent<AudioSource>().Play();
    }

    public void Reset()
    {
        _gameControl=null;
    }

}
