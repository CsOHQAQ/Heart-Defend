using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Moon : MonoBehaviour
{
    public bool isPulling = false;
    public Rigidbody2D rig;
    public float Friction;
    public float OverSpeedLimit;
    public float InitFullMoonIndex = 0.3f;
    public float FullMoonIndex = 0.1f;
    public float FullMoonMaskPosition=3.8f;
    public float FullMoonLightRadius = 25f;
    public Vector2 FullMoonPos;

    GameObject maskGO;
    Vector3 lastPos;
    Light2D moonLight;
    SpriteRenderer moonSprite;
    SpriteRenderer moonNoColorSprite;
    float stuckFriction;
    float stuckPullingCount;
    float nextMoonIndex;
    float posFMIndex;

    private void Start()
    {
        isPulling = false;
        rig = GetComponent<Rigidbody2D>();
        moonLight = GetComponentInChildren<Light2D>();
        maskGO = transform.Find("Mask").gameObject;
        lastPos=transform.position;
        nextMoonIndex = FullMoonIndex;
        moonSprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        moonNoColorSprite = transform.Find("Sprite NoColor").GetComponent<SpriteRenderer>();
        nextMoonIndex = InitFullMoonIndex;
    }
    private void Update()
    {
        if (!isPulling)
        {
            rig.velocity = Vector2.MoveTowards(rig.velocity,Vector2.zero,Friction* Time.deltaTime);
        }

        if(stuckFriction > 0)
        {
            rig.velocity = Vector2.MoveTowards(rig.velocity, Vector2.zero, stuckFriction/100 * Time.deltaTime);// subdivide 50 to adjust the index
        }

        if (rig.velocity.magnitude > 30f)
        {
            rig.velocity = rig.velocity.normalized * 30f;
        }

        if (Vector3.Distance(lastPos, transform.position) > OverSpeedLimit)
        {
            transform.position = lastPos+(transform.position-lastPos).normalized*OverSpeedLimit;
        }
        lastPos = transform.position;

        FullMoonIndex = Mathf.MoveTowards(FullMoonIndex, nextMoonIndex, Time.deltaTime / 1f);
        Debug.Log($"Next Moon Index{nextMoonIndex}");
        RefreshFullMoonIndex();

        if (FullMoonIndex >= 1f)
        {
            if (!GameControl.Game.isFullMoon)
            {
                GameControl.Game.FullMoon();
                posFMIndex = Vector2.Distance(transform.position, FullMoonPos) / GameControl.Game.FullMoonAnimationTime;
            }
            transform.position = Vector2.MoveTowards(transform.position, FullMoonPos, posFMIndex * Time.deltaTime);

        }
    }

    public void RefreshFullMoonIndex()
    {
        FullMoonIndex = Mathf.Clamp(FullMoonIndex, 0,1);

        //Set Mask Position 
        maskGO.transform.localPosition = new Vector2(FullMoonMaskPosition*FullMoonIndex,0); 
        //Set Sprite 
        moonSprite.color = new Color(moonSprite.color.r, moonSprite.color.g, moonSprite.color.b,FullMoonIndex);
        moonNoColorSprite.color = new Color(moonNoColorSprite.color.r, moonNoColorSprite.color.g, moonNoColorSprite.color.b, 1-FullMoonIndex);
        //Set Light
        
        moonLight.transform.localPosition = transform.Find("Mask").localPosition/2-new Vector3(transform.Find("Sprite").GetComponent<CircleCollider2D>().radius,0);
        
        moonLight.intensity = FullMoonIndex*5+0.5f;
        moonLight.pointLightOuterRadius = (FullMoonLightRadius-6)*FullMoonIndex+6;  //[6, FollMoonLightRadius]
        moonLight.falloffIntensity = (1 - FullMoonIndex) * 0.3f + 0.2f; //[0.2,0.5]

    }

    public void SetStuckForce(float f)
    {
        stuckFriction = f;
    }

    public void GetStar()
    {
        nextMoonIndex += (1f - InitFullMoonIndex) / GameControl.Game.StarNum;
    }

    /// <summary>
    /// Triggered before force being added
    /// </summary>
    /// <param name="pullForce">This is force per FRAME!!!</param>
    public void OnBeingPull(Vector2 pullForce)
    {       
        isPulling = true;

        if(pullForce.magnitude>stuckFriction*Time.deltaTime)
        {
            stuckPullingCount = 0;
            //rig.AddForce(pullForce.normalized*(pullForce.magnitude- stuckFriction * Time.deltaTime));
            rig.AddForce(pullForce.normalized*(pullForce.magnitude));
        }
        else
        {
            stuckPullingCount+=Time.deltaTime;

        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Collision {collision.name}");


        if (collision.tag=="DustCloud")
        {
            /*
            bool flag = false;
            foreach (var point in collision.GetComponent<PolygonCollider2D>().points) 
            {
                if(Vector2.Distance(point,transform.position)<GetComponent<CircleCollider2D>().radius&&Vector2.Distance(point, transform.Find("Mask").transform.position) > GetComponent<CircleCollider2D>().radius)
                {
                    Debug.Log("In!");
                    flag = true;
                    break;
                }
            }

            if(flag)

            */
                SetStuckForce(collision.GetComponent<DustCloud>().StaticFriction);
        }

        if (collision.tag == "Star")
        {
            Star star = collision.GetComponent<Star>();
            bool flag = false; 
            if (Vector2.Distance(star.transform.position, transform.position) < GetComponent<CircleCollider2D>().radius && Vector2.Distance(star.transform.position, transform.Find("Mask").transform.position) > GetComponent<CircleCollider2D>().radius)
            {
                flag = true;
            }

            if (!star.isLit)
            {
                star.Lit();
                nextMoonIndex += (1f-InitFullMoonIndex) / GameControl.Game.StarNum;
            }
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"Collision leave {collision.name}");
        if (collision.tag == "DustCloud")
        {
            SetStuckForce(0);
        }
    }

}
