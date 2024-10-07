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
                textBub.Tmp.color = new Color(1, 1, 1, Mathf.MoveTowards(textBub.Tmp.color.a, 0, Time.deltaTime / 3f));
                TitleUI.color = new Color(1, 1, 1, Mathf.MoveTowards(TitleUI.color.a, 0, Time.deltaTime / 3f));
                if (timer >= StayTime&&flag)
                {
                    flag = false;
                    textBub.texts.RemoveAt(0);
                    textBub.Tmp.text = "";
                    GameControl.Game.player.CanMove = false;
                    textBub.AddText("I always forget how much of the sky I get to see when I'm with you",6f);
                    Action act = ()=> {SwitchStep(TotorialStep.MoonMoveToCloud);};
                    textBub.AddText("What's wrong? You aren't saying anything.", 3f);
                    textBub.AddText("Wait, where are you going?",3f,act);
                    
                }
            }
            else
            {
                textBub.Tmp.color = new Color(1, 1, 1, Mathf.MoveTowards(TitleUI.color.a, 1, Time.deltaTime / 3f));
                TitleUI.color = new Color(1, 1, 1, Mathf.MoveTowards(TitleUI.color.a, 1, Time.deltaTime / 3f));
            }


        }
        else if (curStep==TotorialStep.MoonMoveToCloud)
        {
            if (GameControl.Game.moon.isStucked)
            {
                textBub.AddText("My friend has been engulfed by those dark clouds. I need to help the moon.", 6f);
                textBub.AddText("I wonder if I can help pull them out with my light.",6f);
                GameControl.Game.moon.SetFullMoonIndex(0.3f);
                SwitchStep(TotorialStep.PullTheMoon);
            }

        }
        else if (curStep==TotorialStep.PullTheMoon)
        {
            if (!GameControl.Game.moon.isStucked)
            {
                textBub.AddText("I'm glad that works, although it took a lot of effort. I should really take care of myself to not burn out.", 6f);
                textBub.AddText("Those clouds have really affected the moon badly. They seem a lot more down.", 6f);
                textBub.AddText("But the stars shine so bright! Maybe if I show them those stars again, It could brighten their mood.", 6);
                SwitchStep(TotorialStep.CollectStar);
                textBub.AddText("It's nice to stay close to the moon. Sometimes it helps to just have company. It also helps me see more of the sky. The stars to collect, and the clouds to avoid", 7f);
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
                textBub.AddText("I hope I'm helping. A few more stars ought to do the trick.",6f);
                textBub.AddText("The best thing I can do is support the moon, no matter what.", 6f, () => { SwitchStep(TotorialStep.Final); });
                          
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
            StartCoroutine(ShowText("Meet the moon to start", 99999999, 2));
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
