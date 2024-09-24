using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Star : MonoBehaviour
{
    public bool isLit=false;
    public float LitSpeed;
    public float LitRadius;

    
    AudioSource sound;    
    Light2D light2D;

    bool liting;

    public void Init(AudioClip clip)
    {
        isLit = false;
        liting = false;
        
        sound = GetComponentInChildren<AudioSource>();
        light2D = GetComponentInChildren<Light2D>();
        sound.clip = clip;
        sound.Play();
    }

    public void Lit()
    {
        isLit=true;
        liting = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (liting) 
        {
            light2D.pointLightOuterRadius = Mathf.Lerp(light2D.pointLightOuterRadius, LitRadius, LitSpeed * Time.deltaTime);
            light2D.falloffIntensity = Mathf.Lerp(light2D.falloffIntensity, 0.5f, LitSpeed * Time.deltaTime);
        }

    }
}
