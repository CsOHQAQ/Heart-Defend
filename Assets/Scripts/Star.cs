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

    List<GameObject> inTriggerList;

    bool liting;

    public void Init(AudioClip clip)
    {
        isLit = false;
        liting = false;
        inTriggerList = new List<GameObject>();

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
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Mathf.MoveTowards(GetComponent<SpriteRenderer>().color.a, 1, Time.deltaTime / 0.25f));
            light2D.pointLightOuterRadius = Mathf.Lerp(light2D.pointLightOuterRadius, LitRadius, LitSpeed * Time.deltaTime);
            light2D.falloffIntensity = Mathf.Lerp(light2D.falloffIntensity, 0.5f, LitSpeed * Time.deltaTime);
        }


        bool isMoonIn = false, isMaskIn = false;
        foreach (GameObject obj in inTriggerList)
        {
            if (obj.tag == "Moon")
                isMoonIn = true;
            if (obj.tag == "MoonMask")
                isMaskIn = true;
        }
    }

    /*
    private void OnTriggerStay2D(Collider2D collision)
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!inTriggerList.Contains(collision.gameObject))
            inTriggerList.Add(collision.gameObject);
        if (collision.tag == "Moon")
        {
            bool flag = false;
            foreach (var point in GetComponent<PolygonCollider2D>().points)
            {
                if (Vector2.Distance(point,FindAnyObjectByType))
                {

                }
            }
            
        }


    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        inTriggerList.Remove(collision.gameObject);

    }
    */
}


//TODO:Star Lit Hint!