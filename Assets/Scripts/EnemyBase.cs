using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public float maxHealth=5f;
    public float curHealth;
    public float moveSpeed;
    public LineRenderer path;
    public int curPoint;

    public void Init(LineRenderer l)
    {
        path = l;
        curHealth = maxHealth;
        curPoint= 0;
        transform.position = path.GetPosition(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (curPoint == path.positionCount - 1)//Arrive at the end
        {
            OnDestory();
            return;
        }

        if (Vector2.Distance(transform.position, path.GetPosition(curPoint + 1)) < 0.05f)
        {
            transform.position = path.GetPosition(curPoint + 1);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position,path.GetPosition(curPoint+1),moveSpeed);

    }


    void OnDestory()
    {
        Debug.Log($"{transform.name} is destoryed");
        Destroy(gameObject);
    }
}
