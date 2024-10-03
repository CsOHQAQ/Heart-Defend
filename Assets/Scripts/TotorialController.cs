using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rewired;
using UnityEngine.UI;
using System;
public class TotorialController : MonoBehaviour
{
    public TotorialStep curStep=TotorialStep.empty;

    Image TitleUI,blackOut;
    
    TextBubble textBub;
    float timer = 0;


    private void Start()
    {
        textBub=GameObject.Find("TextBubble").GetComponent<TextBubble>();
        blackOut= GameObject.Find("BlackOut").GetComponent<Image>();
        blackOut.color = new Color(0, 0, 0, 0);
        TitleUI = GameObject.Find("TitleUI").GetComponent<Image>();  
        SwitchStep(TotorialStep.MoveToMoon);
    }
    private void Update()
    {
        if (curStep == TotorialStep.MoveToMoon)
        {
            float StayRange = 15f;
            float StayTime = 3f;

            if (Vector2.Distance(GameControl.Game.player.transform.position,GameControl.Game.moon.transform.position)<StayRange)
            {
                timer += Time.deltaTime;
                TitleUI.color = new Color(1, 1, 1, Mathf.MoveTowards(TitleUI.color.a, 0, Time.deltaTime / 3f));
                if (timer >= StayTime)
                {
                    SwitchStep(TotorialStep.MoonMoveToCloud);
                }
            }
            else
            {
                TitleUI.color = new Color(1, 1, 1, Mathf.MoveTowards(TitleUI.color.a, 1, Time.deltaTime / 3f));
            }


        }
        else if (curStep==TotorialStep.MoonMoveToCloud)
        {
            if (GameControl.Game.moon.isStucked)
            {
                GameControl.Game.moon.SetFullMoonIndex(0.3f);
                SwitchStep(TotorialStep.PullTheMoon);
            }

        }
        else if (curStep==TotorialStep.PullTheMoon)
        {
            if (!GameControl.Game.moon.isStucked)
            {
                SwitchStep(TotorialStep.CollectStar);
            }
        }
        else if (curStep == TotorialStep.CollectStar)
        {
            bool flag = true;
            foreach (var star in GameControl.Game.StarList)
            {
                if (!star.isLit)
                {
                    flag = false;
                }
            }
            if (flag)
            {
                SwitchStep(TotorialStep.Final);                
            }
        }
        else if (curStep == TotorialStep.Final)
        {
            blackOut.color = new Color(0,0,0,Mathf.MoveTowards(blackOut.color.a,1,Time.deltaTime));
            if (blackOut.color.a >= 0.98f)
            {
                FinishTotorial();
            }
        }

    }

    void SwitchStep(TotorialStep next) 
    {
        curStep = next;
        if (next == TotorialStep.MoveToMoon)
        {
            GameControl.Game.player.CanMove = true;
            GameControl.Game.player.CanPull= false;
            GameControl.Game.cam.isFollowing = false;

            StartCoroutine(ShowText("press WASD to move", 5, 3));
        }

        else if (next == TotorialStep.MoonMoveToCloud)
        {
            Destroy(TitleUI.gameObject);
            GameControl.Game.cam.isFollowing= true;
            GameControl.Game.cam.followGO = GameControl.Game.moon.gameObject;
            GameControl.Game.player.CanPull= false;
            GameControl.Game.player.CanMove= false;
            GameControl.Game.moon.CanMove = true;
            GameControl.Game.moon.SetWanderTarget();
            GameControl.Game.moon.SetNextWanderPosition();
        }

        else if (next == TotorialStep.PullTheMoon)
        {
            GameControl.Game.cam.isFollowing = true;
            GameControl.Game.cam.followGO = GameControl.Game.player.gameObject;
            GameControl.Game.player.CanPull = true;
            GameControl.Game.player.CanMove = true;
            GameControl.Game.moon.CanMove = true;

        }

        else if (next == TotorialStep.CollectStar)
        {    
            
        }

    }
    IEnumerator ShowText(string text,float showTime, float waitSecond)
    {
        yield return new WaitForSeconds(waitSecond);
        textBub.AddText(text,showTime);
    }
    void FinishTotorial()
    {
        GameControl.Game.Reset();
        SceneManager.LoadScene(1);
    }


    public enum TotorialStep
    {
        empty,
        MoveToMoon,
        MoonMoveToCloud,
        PullTheMoon,
        CollectStar,
        Final
    }
}
