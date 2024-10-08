using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float MoveSpeed;
    public float ShakeStrength;
    public float ZoomSize;
    public float ZoomSpeed;
    public float OriginalSize;
    public float ZoomRange;
    public bool isFollowing = true;
    public GameObject followGO;
    


    public Camera cam;
    PlayerControl player;
    SpriteRenderer bgSprite;
    float borderX=float.MaxValue, borderY=float.MaxValue;
    bool isFullMoon;
    float posFMIndex;
    float sizeFMIndex;

    void Start()
    {
        if (!GameControl.Game.isTotorial)
        {
            isFullMoon = false;
            cam = GetComponent<Camera>();
            player = GameObject.FindWithTag("Player").GetComponent<PlayerControl>();
            followGO=player.gameObject;
            OriginalSize = cam.orthographicSize;
            bgSprite = GameObject.Find("Background").GetComponent<SpriteRenderer>();
            if (bgSprite != null)
            {
                borderX = bgSprite.bounds.size.x;
                borderY = bgSprite.bounds.size.y;
            }
        }
        else
        {
            isFullMoon = false;
            isFollowing = false;
            cam = GetComponent<Camera>();
            player = GameObject.FindWithTag("Player").GetComponent<PlayerControl>();
            followGO = null;
            OriginalSize = cam.orthographicSize;
            bgSprite = GameObject.Find("Background").GetComponent<SpriteRenderer>();
            if (bgSprite != null)
            {
                borderX = bgSprite.bounds.size.x;
                borderY = bgSprite.bounds.size.y;
            }
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
                transform.position = Vector3.MoveTowards(transform.position, followGO.transform.position + new Vector3(0, 0, -10), MoveSpeed * Time.deltaTime);

                transform.position = new Vector3(Mathf.Clamp(transform.position.x, -borderX / 2 + cam.orthographicSize * cam.pixelWidth / cam.pixelHeight, borderX / 2 - cam.orthographicSize * cam.pixelWidth / cam.pixelHeight),
                    Mathf.Clamp(transform.position.y, -borderY / 2 + cam.orthographicSize, borderY / 2 - cam.orthographicSize), -10);
            }
            else
            {

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
            if(!isFollowing)
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(0, 0, -10), posFMIndex * Time.deltaTime);
                cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, 90f, sizeFMIndex * Time.deltaTime);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, followGO.transform.position + new Vector3(0, 0, -10), MoveSpeed * Time.deltaTime);

                cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, OriginalSize+ZoomSize, sizeFMIndex * Time.deltaTime);
                transform.position = new Vector3(Mathf.Clamp(transform.position.x, -borderX / 2 + cam.orthographicSize * cam.pixelWidth / cam.pixelHeight, borderX / 2 - cam.orthographicSize * cam.pixelWidth / cam.pixelHeight),
                    Mathf.Clamp(transform.position.y, -borderY / 2 + cam.orthographicSize, borderY / 2 - cam.orthographicSize), -10);

            }
        }
    }

    public void FullMoonMove()
    {
        followGO = GameControl.Game.moon.gameObject;
        isFullMoon = true;
    }

    public void FullMoonZoomOut()
    {
        isFollowing = false;
        posFMIndex = Vector3.Distance(transform.position, new Vector3(0, 0, -10)) / 2f;
        sizeFMIndex = (90f - cam.orthographicSize) / (2f);
    }

     void Shake(float AdditionalShakeStrength=0f)
    {
        Randomer rnd=new Randomer();
        transform.position += new Vector3(rnd.nextFloat(), rnd.nextFloat()) * (ShakeStrength+AdditionalShakeStrength);
    }
}
