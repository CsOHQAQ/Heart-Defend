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
    public bool needFollowing=true;

    Camera cam;
    PlayerControl player;
    SpriteRenderer bgSprite;
    float borderX=float.MaxValue, borderY=float.MaxValue;

    void Start()
    {
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
        //Camera Following
        transform.position = Vector3.Lerp(transform.position,player.transform.position+new Vector3(0,0,-10),MoveSpeed*Time.deltaTime);

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -borderX / 2 + cam.orthographicSize * cam.pixelWidth / cam.pixelHeight, borderX / 2 - cam.orthographicSize * cam.pixelWidth / cam.pixelHeight),
            Mathf.Clamp(transform.position.y, -borderY / 2 + cam.orthographicSize, borderY / 2 - cam.orthographicSize ),-10);


        //CameraZoom
        if (Vector2.Distance(GameControl.Game.player.transform.position, GameControl.Game.moon.transform.position) < ZoomRange)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize,OriginalSize+ZoomSize,ZoomSpeed*Time.deltaTime);
        }
        else
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, OriginalSize , ZoomSpeed * Time.deltaTime);
        }
    }
}
