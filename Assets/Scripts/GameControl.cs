using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    List<EnemyBase> enemyList;
    List<LineRenderer> lanes;
    private GameControl _gameControl=null;
    public static GameControl Game
    {
        get
        {
            if( Game._gameControl  is null)
            {
                Game._gameControl= new GameControl();
                Game._gameControl.Init();
            }
            return Game._gameControl;
        }
    }

    private void Init()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        enemyList= new List<EnemyBase>();
        lanes= new List<LineRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
    }
}
