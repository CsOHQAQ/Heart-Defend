using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    PlayerControl player;
    public float MoveSpeed;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerControl>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position,player.transform.position+new Vector3(0,0,-10),MoveSpeed*Time.deltaTime);
    }
}
