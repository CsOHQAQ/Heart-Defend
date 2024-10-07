using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using UnityEditor.Rendering;

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

    public float GlobalSaturate;
    
    SpriteRenderer background;
    Light2D globalLight;

    public float FullMoonAnimationTime;
    float bgFullMoonIndex;
    float lFullMoonIndex;
	Volume volume;
    GameObject testGO;
    ColorAdjustments colorAdjust;
    public bool isFullMoon;
	public AudioSource baseMusicLoop;
    public AudioSource winMusicLoop;    public float SaturateTime;
    bool isSaturating=false;
    bool isMoonMovingCenter = false;
    Image endUI;


    private void Awake()
    {
        _gameControl = this;
        _gameControl.Init();
        baseMusicLoop.Play();
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
        volume = GameObject.Find("Global Volume").GetComponent<Volume>();
        volume.profile.TryGet<ColorAdjustments>(out colorAdjust);
        colorAdjust.saturation.value = GlobalSaturate - moon.FullMoonIndex * GlobalSaturate;
        if (isTotorial)
            Totorial = GameObject.Find("TotorialController").GetComponent<TotorialController>();

        endUI = GameObject.Find("TheEndUI").GetComponent<Image>();
        endUI.color = new Color(1, 1, 1, 0);

        isFullMoon = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isFullMoon) 
        {     
            if (isSaturating)
            {
                globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, 1, lFullMoonIndex * Time.deltaTime);
                background.color = new Color(1, 1, 1, Mathf.MoveTowards(background.color.a, 1, bgFullMoonIndex * Time.deltaTime));
                moon.FullMoonMaxSaturation = Mathf.MoveTowards(moon.FullMoonMaxSaturation, 1, Time.deltaTime / SaturateTime);
                colorAdjust.saturation.value = Mathf.MoveTowards(colorAdjust.saturation.value, moon.FullMoonMaxSaturation * moon.FullMoonIndex * 100 - 100, Time.deltaTime);
            }
            else
            {
                endUI.color = new Color(1, 1, 1, Mathf.MoveTowards(endUI.color.a, 1, Time.deltaTime / 3f));
            }

            if (isMoonMovingCenter)
            {
                moon.transform.position = Vector2.MoveTowards(moon.transform.position, moon.FullMoonPos, Time.deltaTime / (FullMoonAnimationTime - SaturateTime));
            }
        }
        else
        {
            //Set Global Saturation
            colorAdjust.saturation.value = Mathf.MoveTowards(colorAdjust.saturation.value,  GlobalSaturate-moon.FullMoonIndex * GlobalSaturate, Time.deltaTime);
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

    /*
    public void FullMoon()
    {
        isFullMoon = true;
        player.CanMove = false;
        player.CanPull= false;

        StartCoroutine(Wait(SaturateTime, cam.FullMoon));
        
        bgFullMoonIndex = (1f - background.color.a) / FullMoonAnimationTime;
        lFullMoonIndex = (1f - globalLight.intensity) / FullMoonAnimationTime;        
        // moon.transform.GetComponent<AudioSource>().Stop();
        //moon.transform.Find("moonReprise").GetComponent<AudioSource>().Play();
        baseMusicLoop.Stop();
        winMusicLoop.Play();
    }
    */
    public void Reset()
    {
        _gameControl=null;
    }
    public IEnumerator FullMoon()
    {
        isFullMoon=true;
        player.CanMove = false;
        player.CanPull= false;
        isSaturating = true;
        moon.CanMove = false;
        isMoonMovingCenter = false;
        cam.FullMoonMove();
        bgFullMoonIndex = (1f - background.color.a) / SaturateTime;
        lFullMoonIndex = (1f - globalLight.intensity) / SaturateTime;
        yield return new WaitForSeconds(SaturateTime);
        isMoonMovingCenter = true;
        yield return new WaitForSeconds(FullMoonAnimationTime-SaturateTime);
        player.CanMove = true;
        cam.FullMoonZoomOut();
        yield return new WaitForSeconds(2);
        isSaturating = false;

    }

}
