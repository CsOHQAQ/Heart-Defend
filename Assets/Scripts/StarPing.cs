using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarPing : MonoBehaviour
{
    public float MaxShownDistance;
    public float FullShownDistance;
    public float radius;
    [HideInInspector]
    public Image img;
    
    Moon moon;
    Star star;
    float nextShownIndex;


    public void Init(Moon iMoon,Star iStar)
    {
        img=GetComponent<Image>();
        moon=iMoon;
        star=iStar;
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 direction=star.transform.position-moon.transform.position;
        float angle = Vector2.SignedAngle(Vector2.right,direction);
        transform.position = Camera.main.WorldToScreenPoint((Vector2)moon.transform.position+direction.normalized*radius);
        transform.rotation = Quaternion.Euler(0, 0, angle);
        nextShownIndex = Mathf.Clamp01((Vector2.Distance(moon.transform.position,star.transform.position)-MaxShownDistance)/FullShownDistance);
        if (Vector2.Distance(GameControl.Game.player.transform.position,moon.transform.position)>GameControl.Game.cam.ZoomRange||star.isLit)
        {
            nextShownIndex = 0;
        }
        img.color = new Color(1,1,1,Mathf.MoveTowards(img.color.a,nextShownIndex,Time.deltaTime));

    }


}
