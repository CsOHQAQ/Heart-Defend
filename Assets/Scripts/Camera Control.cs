using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float MoveSpeed;
    public float ZoomSize;
    public float ZoomSpeed;
    public float OriginalSize;
    public float ZoomRange;
    public bool isFollowing = true;

    Camera cam;
    PlayerControl player;
    SpriteRenderer bgSprite;
    float borderX=float.MaxValue, borderY=float.MaxValue;
    bool isFullMoon;
    float posFMIndex;
    float sizeFMIndex;

    void Start()
    {
        isFullMoon = false;
        cam= GetComponent<Camera>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerControl>();
        OriginalSize = cam.orthographicSize;
        bgSprite=GameObject.Find("Background").GetComponent<SpriteRenderer>();
        if(bgSprite != null)
        {
            borderX = bgSprite.bounds.size.x;
            borderY=bgSprite.bounds.size.y;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!isFullMoon) 
        {
            //Camera Following
            if (isFollowing)
            {
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position + new Vector3(0, 0, -10), MoveSpeed * Time.deltaTime);

                transform.position = new Vector3(Mathf.Clamp(transform.position.x, -borderX / 2 + cam.orthographicSize * cam.pixelWidth / cam.pixelHeight, borderX / 2 - cam.orthographicSize * cam.pixelWidth / cam.pixelHeight),
                    Mathf.Clamp(transform.position.y, -borderY / 2 + cam.orthographicSize, borderY / 2 - cam.orthographicSize), -10);
            } 

            //CameraZoom
            if (Vector2.Distance(GameControl.Game.player.transform.position, GameControl.Game.moon.transform.position) < ZoomRange)
            {
                cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, OriginalSize + ZoomSize, ZoomSpeed * Time.deltaTime);
            }
            else
            {
                cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, OriginalSize, ZoomSpeed * Time.deltaTime);
            }
        }
        else
        {
            transform.position=Vector3.MoveTowards(transform.position,new Vector3(0,0,-10), posFMIndex * Time.deltaTime);
            cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize,90f,sizeFMIndex*Time.deltaTime);
        }
    }

    public void FullMoon()
    {
        posFMIndex = Vector3.Distance(transform.position, new Vector3(0, 0, -10)) / (GameControl.Game.FullMoonAnimationTime);
        sizeFMIndex=(90f-cam.orthographicSize)/GameControl.Game.FullMoonAnimationTime;
        isFullMoon = true;
    }
}
