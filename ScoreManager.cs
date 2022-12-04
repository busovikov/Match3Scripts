using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    
    private static readonly string TriggerName = "Combo";

    public GameObject comboBonus;
    private Text comboBonusVal;
    private Animator comboBonusAnimator;
    public ScoreUI scoreUI;

    [HideInInspector]
    public int current = 0;
    [HideInInspector]
    public int bestTime = 0;
    [HideInInspector]
    public int bestMoves = 0;
    [HideInInspector]
    public int total = 0;
    [HideInInspector]
    public int coin = 0;
    [HideInInspector]
    public DateTime lastAuthBonus;

    void Start()
    {
        if (comboBonus)
        {
            comboBonusVal = comboBonus.transform.Find("Val").GetComponent<Text>();
            comboBonusAnimator = comboBonus.GetComponent<Animator>();
        }

        Config.Instance.LoadScoreBestTime(out bestTime);
        Config.Instance.LoadScoreBestMoves(out bestMoves);
        Config.Instance.LoadScoreTotal(out total);
        Config.Instance.LoadScoreCoin(out coin);
        Config.LoadLastAuthBonusDate(out lastAuthBonus);
    }

    public void SetLastAuthBonus()
    {
        lastAuthBonus = DateTime.Now;
        Config.SaveLastAuthBonusDate(lastAuthBonus);
    }

    public bool AuthBonusAvailable()
    {
        Config.LoadLastAuthBonusDate(out lastAuthBonus);
        return (DateTime.Now - lastAuthBonus).TotalDays > 1;
    }
    public void SubCoinScore(int val)
    {
        if (val > 0 && coin >= val)
        {
            coin -= val;
            Config.SaveScoreCoin(coin);
        }
    }

    public void AddCoinScore(int val)
    {
        coin += val;
        Config.SaveScoreCoin(coin);
    }

    public void AddScore(int val)
    {
        if (val > 0 )
        {
            current += val;
            scoreUI.Set(current);
        }
    }

    public int GetBest()
    { 
        return LevelLoader.mode == LevelLoader.GameMode.Moves ? bestMoves : bestTime;
    }

    void SetBest(int val)
    {
        if (LevelLoader.mode == LevelLoader.GameMode.Moves)
        {
            bestMoves = val;
        }
        else
        {
            bestTime = val;
        }
    }
    public void SetTotalScore()
    {
        if (GetBest() < current)
        {
            SetBest(current);
            if (LevelLoader.mode == LevelLoader.GameMode.Moves)
            {
                Config.SaveScoreBestMoves(current);
            }
            else
            {
                Config.SaveScoreBestTime(current);
            }
        }
        total += current;
        Config.SaveScoreTotal(total);
#if PLATFORM_WEBGL && !UNITY_EDITOR
        Yandex.SetScoreToLeaderBoard(total);
#endif
    }

    internal void AddCombo(int count)
    {
        if (count > 0)
        {
            comboBonusVal.text = (++count * 10).ToString();
            comboBonusAnimator.SetTrigger(TriggerName);
        }
    }
}
