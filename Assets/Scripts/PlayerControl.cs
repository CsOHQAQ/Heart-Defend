using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Unity.VisualScripting;
using System.Data.Common;
using UnityEngine.Rendering;

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

    Rigidbody2D rig;
    bool isMoving=false;
    Player player;
    Moon moon;

    // Start is called before the first frame update
    void Start()
    {
        player=ReInput.players.GetPlayer(0);
        rig=GetComponent<Rigidbody2D>();
        moon=GameObject.FindGameObjectWithTag("Moon").GetComponent<Moon>();

    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        if (isMoving)
        {
            float targetAngle = Vector2.SignedAngle(Vector2.right, targetFacing);
            Vector3 v = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(v.z, targetAngle, RotateSpeed * Time.deltaTime));
            rig.velocity = Vector2.MoveTowards(rig.velocity, targetFacing*MaxSpeed,Acclerate*Time.deltaTime);
        }
        else//Stopping
        {
            rig.velocity = Vector2.MoveTowards(rig.velocity,Vector2.zero,Acclerate*Time.deltaTime/4);
        }

        if(player.GetButton("Pull"))
        {
            curPullForce += ForceIncreasement * Time.deltaTime;

            Vector2 force = transform.position-moon.transform.position;
            force=force.normalized;
            float forceIndex =curPullForce*rig.mass*moon.GetComponent<Rigidbody2D>().mass /2* Mathf.Log(Vector2.Distance(transform.position, moon.transform.position));
            
            if (Vector2.Distance(moon.transform.position, transform.position) <= 1)//Prevent throw the moon too far
            {
                forceIndex = Vector2.Distance(moon.transform.position, transform.position) * CloseRangePullForce;
            }

            moon.GetComponent<Rigidbody2D>().AddForce(force  * forceIndex * Time.deltaTime);
            moon.isPulling= true;
        }
        else
        {
            moon.isPulling = false;
            curPullForce = StartPullForce;
        }
    }
    void GetInput()
    {
        if (Mathf.Abs(player.GetAxis("MoveX")) < 0.05f && Mathf.Abs(player.GetAxis("MoveY")) < 0.05f)
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
}
