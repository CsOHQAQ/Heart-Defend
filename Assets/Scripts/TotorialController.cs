using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rewired;
public class TotorialController : MonoBehaviour
{
    public TotorialStep curStep=TotorialStep.empty;


    TextBubble textBub;
    float timer = 0;


    private void Start()
    {
        textBub=GameObject.Find("TextBubble").GetComponent<TextBubble>();
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
                GameControl.Game.cam.
                if (timer >= StayTime)
                {
                    SwitchStep(TotorialStep.MoonMoveToCloud);
                }
            }


        }
        else if (curStep==TotorialStep.MoonMoveToCloud)
        {
            if (GameControl.Game.moon.isStucked)
            {
                SwitchStep(TotorialStep.PullTheMoon);
            }

        }
        else if (curStep==TotorialStep.PullTheMoon)
        {

        }
        else if (curStep == TotorialStep.CollectStar)
        {

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
        SceneManager.LoadScene(1);
    }


    public enum TotorialStep
    {
        empty,
        MoveToMoon,
        MoonMoveToCloud,
        PullTheMoon,
        CollectStar,
    }
}
