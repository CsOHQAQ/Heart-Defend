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
    float timer = 0,timer2=0f;
    bool flag;


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
                if (timer >= StayTime&&flag)
                {
                    flag = false;
                    textBub.AddText("The sky looks great tonight. Woah! It looks even better from your perspective.",6f);
                    Action act = ()=> {SwitchStep(TotorialStep.MoonMoveToCloud);};
                    textBub.AddText("Wait, where are you going?",6f,act);
                    
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
                textBub.AddText("My friend has been engulfed by those dark clouds, I need to help the moon out of them.", 6f);
                textBub.AddText("Maybe I can pull them out with my light.",6f);
                GameControl.Game.moon.SetFullMoonIndex(0.3f);
                SwitchStep(TotorialStep.PullTheMoon);
            }

        }
        else if (curStep==TotorialStep.PullTheMoon)
        {
            if (!GameControl.Game.moon.isStucked)
            {
                textBub.AddText("That took a lot of effort, I must take care of myself to not burn out.", 6f);
                textBub.AddText("Those clouds affected them really badly. They look like they have just shut down.", 6f);
                textBub.AddText("I could help them see the stars again, to brighten their mood.", 6);
                SwitchStep(TotorialStep.CollectStar);
            }
        }
        else if (curStep == TotorialStep.CollectStar)
        {
            bool flag2 = true;
            foreach (var star in GameControl.Game.StarList)
            {
                if (!star.isLit)
                {
                    flag2 = false;
                }
            }

            if (flag&&flag2)
            {
                flag= false;
                textBub.AddText("Does that help? Maybe we can seek more stars.",6f);
                textBub.AddText("I know the best thing I can do is support the moon, no matter what.", 6f, () => { SwitchStep(TotorialStep.Final); });
                          
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
            flag = true;
            StartCoroutine(ShowText("Meet the moon to start", 6, 2));
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
            GameControl.Game.moon.DirectSetTargetToWanderPosition();
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
            flag = true;
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
