using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Unity.VisualScripting;
using System.Data.Common;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Runtime.CompilerServices;

public class PlayerControl : MonoBehaviour
{
    public float RotateSpeed;
    public float Acclerate;
    public float MaxSpeed;
    public float StartPullForce;
    public float curPullForce;
    public float ForceIncreasement;
    public float CloseRangePullForce;
    public Vector2 targetFacing = Vector2.zero;
    public bool CanControl = true;

    public float MaxEnergy;
    public float EnergyRecoverSpeed;

    public float PullCoolDown;
    
    public bool UsingNewPullMech=false;
    public float MaxCharingTime = 3f;

    public bool CanMove=true;
    public bool CanPull=true;



    Light2D selfLight;
    Light2D haloLight;
    Rigidbody2D rig;
    bool isMoving=false;
    Player player;
    Moon moon;
    float curEnergy;
    float coolDownTimer;
    float curPullChargeCount;
    bool isPulling;
    ParticleSystem pullEffect;
    float pullIndex=0f;
    public AudioSource starTwinkle;
    public AudioSource pullNoise;

    // Start is called before the first frame update
    void Start()
    {
        player=ReInput.players.GetPlayer(0);
        rig=GetComponent<Rigidbody2D>();
        moon = GameControl.Game.moon;

        selfLight=transform.Find("Self Light").GetComponentInChildren<Light2D>();
        haloLight = transform.Find("Halo").GetComponent<Light2D>();
        curEnergy = MaxEnergy;
        isPulling = false;
        pullEffect=transform.Find("PullEffect").GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        if (isMoving)
        {
            rig.drag = 0;
            float targetAngle = Vector2.SignedAngle(Vector2.right, targetFacing);
            Vector3 v = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(v.z, targetAngle, RotateSpeed * Time.deltaTime));
            rig.velocity = Vector2.MoveTowards(rig.velocity, targetFacing * MaxSpeed, curEnergy / MaxEnergy * Acclerate * Time.deltaTime);
            if (!starTwinkle.isPlaying)
            { //starTwinkle.volume = 0.1f;
              starTwinkle.Play();
            }
                
        }
        else//Stopping
        {
            rig.drag = 2.5f*Acclerate / (rig.velocity.magnitude + 1f);
            //rig.velocity = Vector2.MoveTowards(rig.velocity,Vector2.zero,Acclerate*Time.deltaTime/Mathf.Log(1+ rig.velocity.magnitude));
            if (GetComponent<Rigidbody2D>().velocity.magnitude < 1)
            {
                starTwinkle.Stop();
            }
        }
        
        if(CanPull&&player.GetButton("Pull")&&curEnergy>0&&(coolDownTimer<=0f||isPulling))
        {
            isPulling = true;
            if (UsingNewPullMech)
            {

                curPullChargeCount += Time.deltaTime;
                selfLight.intensity += 3*Time.deltaTime;
                selfLight.pointLightOuterRadius += 2 * Time.deltaTime;
                //Randomer rnd = new Randomer();
                //transform.position += new Vector2();
                if (curPullChargeCount>=MaxCharingTime*curEnergy/MaxEnergy)
                {
                    Debug.Log("Overcharged!");
                    curEnergy = 0;
                    curPullChargeCount = 0;
                }

            }
            else
            {
                //Consume Energy
                curEnergy -= Time.deltaTime;
                if (curEnergy < 0)
                    curEnergy = 0;
                if (!pullNoise.isPlaying)
                {
                    pullNoise.volume = 0.16f;
                    pullNoise.Play();
                }
                curPullForce += (curEnergy / MaxEnergy) * ForceIncreasement * Time.deltaTime;

                //Calculate Pull Force
                Vector2 force = transform.position - moon.transform.position;
                force = force.normalized;
                float forceIndex = (curEnergy / MaxEnergy * 3 + 1) * curPullForce * rig.mass * moon.GetComponent<Rigidbody2D>().mass 
                    /( 2 * Mathf.Log(Vector2.Distance(transform.position, moon.transform.position)));
                pullIndex = forceIndex;
                if (Vector2.Distance(moon.transform.position, transform.position) <= 10)//Prevent throw the moon too far
                {
                    forceIndex = Vector2.Distance(moon.transform.position, transform.position) * CloseRangePullForce;
                }
                //Debug.Log($"Cur Pull force: {forceIndex.ToString("0.00")},  ");
                moon.OnBeingPull(force * forceIndex * Time.deltaTime);
            }
            
            
        }
        else
        {

            if (UsingNewPullMech)
            {
                if (curPullChargeCount > 0)
                {
                    //Calculate Pull Force
                    Vector2 force = transform.position - moon.transform.position;
                    force = force.normalized;
                    float forceIndex = curPullChargeCount*400000f+200000f;



                    if (Vector2.Distance(moon.transform.position, transform.position) <= 1)//Prevent throw the moon too far
                    {
                        forceIndex = Vector2.Distance(moon.transform.position, transform.position) * CloseRangePullForce;
                    }

                    Debug.Log($"Released at {forceIndex}!");
                    moon.OnBeingPull(force * forceIndex * Time.deltaTime);

                    curEnergy -= curPullChargeCount * MaxEnergy / MaxCharingTime;
                    curPullChargeCount = 0;
                }
            }

            //Restore Energy 
            if(curEnergy<MaxEnergy)
                curEnergy +=EnergyRecoverSpeed*Time.deltaTime;

            if (isPulling)
            {
                isPulling = false;
                coolDownTimer = PullCoolDown;
                //pullNoise.Stop();
            }
            else
            {
                pullNoise.volume=Mathf.MoveTowards(pullNoise.volume,0,Time.deltaTime);
                if (pullNoise.volume < 0.05f)
                {
                    pullNoise.Stop();
                }
            }
            if (coolDownTimer > 0) coolDownTimer -= Time.deltaTime;
            moon.isPulling = false;
            curPullForce = StartPullForce;
        }

        //Set Light intense
        if (!UsingNewPullMech)
        {
            selfLight.intensity = 3*curEnergy / MaxEnergy;
            selfLight.pointLightOuterRadius = 8f * curEnergy / MaxEnergy;

            if (coolDownTimer <= 0)
            {
                haloLight.transform.localScale = new Vector3(1, 1, 1) * 2 * curEnergy / MaxEnergy;
                haloLight.intensity = 5 * curEnergy / MaxEnergy;
            }
            else
            {
                haloLight.transform.localScale = new Vector3(1, 1, 1) * 2 * curEnergy / MaxEnergy;
                haloLight.intensity = 5 * curEnergy / MaxEnergy +3* Mathf.Sin((PullCoolDown - coolDownTimer)*Mathf.PI*4/PullCoolDown) ; 
            }
        }
        else
        {
            if (curPullChargeCount <= 0.05f)
            {
                selfLight.intensity = curEnergy / MaxEnergy;
                selfLight.pointLightOuterRadius = 8f * curEnergy / MaxEnergy;               
                
            }

        }
        //coneLight.intensity = curEnergy / MaxEnergy;

        RefreshPullEffect();

    }
    void GetInput()
    {

        if (!CanMove||Mathf.Abs(player.GetAxis("MoveX")) < 0.05f && Mathf.Abs(player.GetAxis("MoveY")) < 0.05f)
        {
            isMoving = false;
            return;
        }
        isMoving=true;
        targetFacing.x = player.GetAxis("MoveX");
        targetFacing.y = player.GetAxis("MoveY");
        targetFacing= targetFacing.normalized;

    }

    void OnControllerConnected(ControllerStatusChangedEventArgs args)
    {
        Debug.Log($"Connected Device is {args.controllerType}");
        if (args.controllerType != ControllerType.Joystick)
        {
            //Debug.Log($"Connected Device is {args.controllerType}");
            return;
        }
        
    }

    void RefreshPullEffect()
    {
        float angle = Vector2.SignedAngle(Vector2.right, GameControl.Game.moon.transform.position-transform.position);
        pullEffect.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        if (isPulling)
        {
            var rateOverTime = pullEffect.emission;
            rateOverTime.rateOverTime= pullIndex/500f;
            var lifeTime = pullEffect.main;
            lifeTime.startLifetime=Vector2.Distance(transform.position,moon.transform.position)*0.3f/lifeTime.startSpeed.constant;
            if(!pullEffect.isPlaying)
                pullEffect.Play();
        }
        else
        {
            if (pullEffect.isPlaying)
                pullEffect.Stop();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "DustCloud")
        {
            MaxSpeed /= 2;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "DustCloud")
        {
            MaxSpeed *= 2;
        }
    }

}


//TODO: Player slow down enterinig the cloud
