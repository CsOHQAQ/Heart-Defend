using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Timers;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Moon : MonoBehaviour
{
    public bool isPulling = false;
    [HideInInspector]
    public Rigidbody2D rig;
    //public float Friction;
    public float OverSpeedLimit;
    public float ShakeStrength;
    [Header("Wander Settings")]
    public float WanderRadius;
    public float WanderStepLength;
    public float WanderSpeed;


    [Header("Full Moon Settings")]
    public float InitFullMoonIndex = 0.3f;
    public float FullMoonIndex = 0.1f;
    public float FullMoonMaskPosition=3.8f;
    public float FullMoonLightRadius = 25f;    
    public Vector2 FullMoonPos;

    public bool CanMove=true;
    public bool isStucked = false;

    GameObject maskGO;
    Vector3 lastPos;
    Light2D moonLight;
    SpriteRenderer moonSprite;
    SpriteRenderer moonNoColorSprite;
    float stuckFriction;
    float stuckPullingCount;
    float nextMoonIndex;
    float posFMIndex;
    float wanderTimer;
    float changeTargetTimer;
    GameObject wanderTarget;
    Vector2 wanderPos;
    Vector2 beforeShakePos;
    float stuckMoveInTimer;
    Vector2 stuckMoveInPos;

    public AudioSource stuckSound;
    public AudioSource starPickup;
    public AudioSource unstuckNoise;


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
        wanderTimer= 0;

        if (CanMove)
        {
            SetWanderTarget();
            SetNextWanderPosition();
        }

    }
    private void Update()
    {
        if (CanMove)
        {
            if (!isPulling)
            {
                //rig.velocity = Vector2.MoveTowards(rig.velocity,Vector2.zero,Friction* Time.deltaTime);
                wanderTimer += Time.deltaTime;
            }
            else
            {
                wanderTimer = 0;
            }

            //Random change target every 3s
            changeTargetTimer += Time.deltaTime;
            if (changeTargetTimer > 3f && FullMoonIndex <= 0.95f)
            {
                changeTargetTimer = 0;
                SetWanderTarget();
                SetNextWanderPosition();
            }

            if (stuckMoveInTimer > 0&&isStucked)
            {
                stuckMoveInTimer -= Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, stuckMoveInPos, 3 * Time.deltaTime);
            }


            //Check wander

            Vector2 speed = (wanderPos - (Vector2)transform.position).normalized * WanderSpeed;
            rig.velocity = Vector2.MoveTowards(rig.velocity, speed, Time.deltaTime / 0.5f);
            Debug.DrawLine(transform.position, wanderPos, Color.red);
            if (Vector2.Distance(transform.position, wanderPos) < 1f)
            {
                Debug.Log("Moon have wander to the target");
                if (Vector2.Distance(transform.position, wanderTarget.transform.position) < 5f)
                    SetWanderTarget();
                SetNextWanderPosition();
            }

            if (wanderTimer > 0.5f)
            {
                /*
                Vector2 speed = (wanderPos - (Vector2)transform.position).normalized * WanderSpeed;
                rig.velocity = Vector2.MoveTowards(rig.velocity, speed, Time.deltaTime / 0.5f);
                Debug.DrawLine(transform.position,wanderPos,Color.red);
                if (Vector2.Distance(transform.position,wanderPos)<1f)
                {
                    Debug.Log("Moon have wander to the target");
                    if (Vector2.Distance(transform.position, wanderTarget.transform.position) < 5f)
                        SetWanderTarget();
                    SetNextWanderPosition();
                }
                */
            }

            //Check if stucked
            if (stuckFriction > 0)
            {
                //rig.velocity = Vector2.MoveTowards(rig.velocity, Vector2.zero, stuckFriction/20 * Time.deltaTime);// subdivide 50 to adjust the index
            }
            if (rig.velocity.magnitude > 60f)
            {
                rig.velocity = rig.velocity.normalized * 60f;
            }

            FullMoonIndex = Mathf.MoveTowards(FullMoonIndex, nextMoonIndex, Time.deltaTime / 2f);

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
        else
        {
            //Check if stucked
            if (stuckFriction > 0)
            {
                //rig.velocity = Vector2.MoveTowards(rig.velocity, Vector2.zero, stuckFriction/20 * Time.deltaTime);// subdivide 50 to adjust the index
            }
            if (rig.velocity.magnitude > 60f)
            {
                rig.velocity = rig.velocity.normalized * 60f;
            }

            FullMoonIndex = Mathf.MoveTowards(FullMoonIndex, nextMoonIndex, Time.deltaTime / 2f);

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

        UpdatePolygonColliderChange();

    }

    public void SetStuckForce(float f)
    {
        stuckFriction = f;
        if (f > 0)
        {
            rig.drag = f;
            beforeShakePos = transform.position;
        }
        else
        {
            rig.drag = 0.75f;
        }
    }

    public void GetStar()
    {
        if (!GameControl.Game.isTotorial)
        {
            nextMoonIndex += (1f - InitFullMoonIndex) / GameControl.Game.StarNum;
            if (nextMoonIndex > 0.95f)
                nextMoonIndex = 1f;
        }        
    }

    /// <summary>
    /// Triggered before force being added
    /// </summary>
    /// <param name="pullForce">This is force per FRAME!!!</param>
    public void OnBeingPull(Vector2 pullForce)
    {       
        isPulling = true;
        if (stuckFriction > 0)
        {
            //New friction thought
            if (stuckPullingCount > 2f)
            {
                rig.drag  = 0f;
                transform.rotation=Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                Randomer rnd = new Randomer();

                float shakeAngle = (Mathf.Sin(stuckPullingCount * Mathf.PI*3)-0.5f) * ShakeStrength;
                transform.rotation = Quaternion.Euler(0, 0, shakeAngle);
                stuckPullingCount += Time.deltaTime;
            }
            rig.AddForce(pullForce);

            //Original friction
            /*
            //If pullforce big enough or pulling too long
            if (pullForce.magnitude > stuckFriction * Time.deltaTime || (stuckPullingCount > 2f&&pullForce.magnitude>100f))
            {
                stuckPullingCount = 0;

                //rig.drag = 0;
                rig.AddForce(pullForce);
            }
            else
            {
                Randomer rnd = new Randomer();
                float angle = rnd.nextFloat();
                if (rnd.nextFloat() > 0.5f)
                    return;
                Vector2 shakePos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * pullForce.magnitude / (stuckFriction * Time.deltaTime) * (ShakeStrength + 0.2f * stuckPullingCount);
                transform.position = (Vector2)transform.position + shakePos;
                stuckPullingCount += Time.deltaTime;

            }
            */
        }
        else
        {
            rig.AddForce(pullForce);
        }       
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Collision {collision.name}");


        if (collision.tag=="DustCloud")
        {
            stuckPullingCount = 0;
            SetStuckForce(collision.GetComponent<DustCloud>().StaticFriction);
			stuckSound.Play();
			stuckMoveInTimer = 2f;
            stuckMoveInPos = collision.transform.position;
            isStucked = true;
        }

        if (collision.tag == "Star")
        {
            Star star = collision.GetComponent<Star>();
            /*
            bool flag = false; 
            if (Vector2.Distance(star.transform.position, transform.position) < GetComponent<CircleCollider2D>().radius && Vector2.Distance(star.transform.position, transform.Find("Mask").transform.position) > GetComponent<CircleCollider2D>().radius)
            {
                flag = true;
            }
            */
            if (!star.isLit)
            {
                star.Lit();
                GetStar();   
                starPickup.Play();
            }
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"Collision leave {collision.name}");
        if (collision.tag == "DustCloud")
        {
            unstuckNoise.Play();
            SetStuckForce(0);
            isStucked = false;
            rig.AddForce(rig.velocity.normalized * 4000f);
        }
    }

    private void UpdatePolygonColliderChange()
    {
        PolygonCollider2D poly=GetComponent<PolygonCollider2D>();

        Vector2[] nPath =new Vector2[poly.points.Length];
        poly.points.CopyTo(nPath,0);        

        nPath[0].x = Mathf.Abs(poly.points[4].x) * (FullMoonIndex * 2 - 1);
        nPath[1].x = Mathf.Abs(poly.points[3].x)*(FullMoonIndex*2-1);
        nPath[7].x = Mathf.Abs(poly.points[5].x) * (FullMoonIndex * 2 - 1);

        poly.SetPath(0, nPath);
    }

    public void SetWanderTarget()
    {
        if (FullMoonIndex > 0.95f)
            return;
        Randomer rnd = new Randomer();
        float choice = rnd.nextFloat();
        if (choice<FullMoonIndex)//Choose Star
        {
            Star nStar = null;
            float dis = 99999999f;
            foreach(Star star in GameControl.Game.StarList)
            {
                if (Vector2.Distance(star.transform.position, transform.position) < dis && !star.isLit)
                {
                    if (Vector2.Distance(star.transform.position, wanderPos) < 5f)
                        continue;
                    nStar= star;
                    dis=Vector2.Distance(star.transform.position, transform.position);
                }
            }
            if (nStar == null)
            {
                Debug.Log("No star finded as next target!");
            }
            wanderTarget = nStar.gameObject;
        }

        else//Choose cloud
        {
            DustCloud nCloud = null;
            float dis = 99999999f;
            foreach (DustCloud cloud in GameControl.Game.DustCloudList)
            {
                if (Vector2.Distance(cloud.transform.position, transform.position) < dis)
                {
                    if (Vector2.Distance(cloud.transform.position, wanderPos) < 5f)
                        continue;
                    nCloud = cloud;
                    dis = Vector2.Distance(cloud.transform.position, transform.position);
                }
            }
            if (nCloud == null)
            {
                Debug.Log("No star finded as next target!");
            }
            wanderTarget = nCloud.gameObject;
        }
    }

    public void SetNextWanderPosition()
    {        
        Vector2 center = Vector2.MoveTowards(transform.position,wanderTarget.transform.position,WanderStepLength);
        Randomer rnd= new Randomer();
        float angle=rnd.nextFloat()*2*Mathf.PI,r=rnd.nextFloat()*WanderRadius;
        wanderPos=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r+center;
    }

    public void SetFullMoonIndex(float index) 
    {
        nextMoonIndex = index;
    }

}

//TODO: Moon shake
