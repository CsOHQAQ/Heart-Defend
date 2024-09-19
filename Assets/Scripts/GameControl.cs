using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl
{
    List<EnemyBase> enemyList;
    List<LineRenderer> lanes;
    private static GameControl _gameControl=null;
    public static GameControl Game
    {
        get
        {
            if( _gameControl  is null)
            {
                _gameControl = new GameControl();
                _gameControl.Init();
            }
            return _gameControl;
        }
    }

    public Moon moon=null;
    public PlayerControl player=null;


    private void Init()
    {
        if (GameObject.Find("Moon") is not null)
            moon = GameObject.Find("Moon").GetComponent<Moon>();
        else
            Debug.LogError("Moon Not Found!");

        if (GameObject.Find("Player") is not null)
            player= GameObject.Find("Player").GetComponent<PlayerControl>();
        else
            Debug.LogError("Player Not Found!");

        enemyList = new List<EnemyBase>();
        lanes = new List<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
