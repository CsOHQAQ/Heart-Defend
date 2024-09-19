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


    GameObject maskGO;
    Vector3 lastPos;
    Light2D moonLight;

    private void Start()
    {
        isPulling = false;
        rig = GetComponent<Rigidbody2D>();
        moonLight = GetComponent<Light2D>();
        maskGO = transform.Find("Mask").gameObject;
        lastPos=this.transform.position;
    }
    private void Update()
    {
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


    public void SetFullMoonIndex(float index)
    {
        if(index<0) index = 0; 
        if(index>1) index = 1;  



    }
}
