using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : MonoBehaviour
{
    public bool isPulling = false;
    public Rigidbody2D rig;
    public float Friction;
    public float OverSpeedLimit;

    Vector3 lastPos;
    private void Start()
    {
        isPulling = false;
        rig = GetComponent<Rigidbody2D>();
        lastPos=this.transform.position;
    }
    private void Update()
    {
        if (!isPulling)
        {
            rig.velocity = Vector2.MoveTowards(GetComponent<Rigidbody2D>().velocity,Vector2.zero,Time.deltaTime/2);
        }
        if (rig.velocity.magnitude >= 30f)
        {
            rig.velocity = rig.velocity.normalized * 30f;
        }

        if (Vector3.Distance(lastPos, transform.position) > OverSpeedLimit)
        {
            transform.position = lastPos+(transform.position-lastPos).normalized*OverSpeedLimit;
        }
        lastPos = transform.position;
    }

}
