using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DustCloud : MonoBehaviour
{
    public float StaticFriction;
    float ThunderSpeed=1f;
    float ThunderInterval;
    float curCount;
    Light2D thunder;
    List<GameObject> inTriggerList;
    bool isStuckingMoon = false;
    

    public void Init()
    {
        thunder = GetComponentInChildren<Light2D>();
        PlayThunder();
        inTriggerList = new List<GameObject>();
    }

    private void Update()
    {
        thunder.intensity = Mathf.MoveTowards(thunder.intensity,0,Time.deltaTime*ThunderSpeed);
        curCount += Time.deltaTime;
        if(curCount > ThunderInterval)
        {
            PlayThunder();
        }

        bool InMoon=false, InMask=false;
        foreach (GameObject obj in inTriggerList) 
        {
            if(obj.tag=="Moon")
                InMoon = true;
            if(obj.tag=="MoonMask")
                InMask = true;
        }
        if (!(InMask&&InMoon)&&InMoon)
        {
            Debug.Log("Collide!");
            if (!isStuckingMoon)
            {
                GameControl.Game.moon.SetStuckForce(StaticFriction);
                isStuckingMoon = true;
            }
        }
        else
        {
            if (isStuckingMoon)
            {
                GameControl.Game.moon.SetStuckForce(0);
                isStuckingMoon = false;
            }

        }
    }

    public void PlayThunder()
    {
        Randomer rnd = new Randomer();
        curCount = 0;
        ThunderInterval = (rnd.nextFloat() - 0.5f) + 2f;
        thunder.intensity = 0.8f;
        thunder.transform.localPosition = new Vector2((rnd.nextFloat()-0.5f)*1.6f, (rnd.nextFloat() - 0.5f) * 1.6f);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!inTriggerList.Contains(collision.gameObject))
            inTriggerList.Add(collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        inTriggerList.Remove(collision.gameObject);
        if (collision.tag == "Moon")
        {
        }
    }
}
