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
    public float FullMoonIndex = 0.1f;
    public float FullMoonMaskPosition=3.8f;
    public float FullMoonLightRadius = 25f;
    public bool isStucked;

    GameObject maskGO;
    Vector3 lastPos;
    Light2D moonLight;
    SpriteRenderer moonSprite;
    SpriteRenderer moonNoColorSprite;
    float stuckFriction;

    private void Start()
    {
        isPulling = false;
        rig = GetComponent<Rigidbody2D>();
        moonLight = GetComponent<Light2D>();
        maskGO = transform.Find("Mask").gameObject;
        lastPos=this.transform.position;

        moonSprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        moonNoColorSprite = transform.Find("Sprite NoColor").GetComponent<SpriteRenderer>();

    }
    private void Update()
    {
        //DEBUG ONLY
        RefreshFullMoonIndex(FullMoonIndex);

        if (!isPulling)
        {
            Debug.Log($"Moon Stopping, cur speed {rig.velocity.magnitude}");
            rig.velocity = Vector2.MoveTowards(rig.velocity,Vector2.zero,Friction* Time.deltaTime);
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
    }


    public void RefreshFullMoonIndex(float index)
    {
        index = Mathf.Clamp(index, 0,1);

        FullMoonIndex = index;
        //Set Mask Position 
        maskGO.transform.localPosition = new Vector2(FullMoonMaskPosition*FullMoonIndex,0); 
        //Set Sprite 
        moonSprite.color = new Color(moonSprite.color.r, moonSprite.color.g, moonSprite.color.b,FullMoonIndex);
        moonNoColorSprite.color = new Color(moonNoColorSprite.color.r, moonNoColorSprite.color.g, moonNoColorSprite.color.b, 1-FullMoonIndex);
        //Set Light
        moonLight.intensity = FullMoonIndex*5+0.5f;
        moonLight.pointLightOuterRadius = (FullMoonLightRadius-6)*FullMoonIndex+6;  //[6, FollMoonLightRadius]
        moonLight.falloffIntensity = (1 - FullMoonIndex) * 0.3f + 0.2f; //[0.2,0.5]

    }

    public void OnBeingPull(Vector2 pullForce)
    {

    }
}
