using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rewired;
public class TotorialController : MonoBehaviour
{
    public TotorialStep curStep;

    private void Start()
    {
        curStep = TotorialStep.MoveToMoon;
    }
    private void Update()
    {
        if (curStep == TotorialStep.MoveToMoon)
        {

        }
        else if (curStep==TotorialStep.MoonMoveToCloud)
        {

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
        if (next == TotorialStep.MoveToMoon)
        {

        }

        else if (next == TotorialStep.MoonMoveToCloud)
        {

        }

        else if (next == TotorialStep.PullTheMoon)
        {

        }

        else if (next == TotorialStep.CollectStar)
        {

        }

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
