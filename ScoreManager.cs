using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private static readonly string ScoreTotalString = "Score.Total";
    private static readonly string ScoreBestTimeString = "Score.Best.Time";
    private static readonly string ScoreBestMovesString = "Score.Best.Moves";
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

    void Awake()
    {
        comboBonusVal = comboBonus.transform.Find("Val").GetComponent<Text>();
        comboBonusAnimator = comboBonus.GetComponent<Animator>();

        Config.LoadInt(ScoreBestTimeString, out bestTime, bestTime);
        Config.LoadInt(ScoreBestMovesString, out bestMoves, bestMoves);
        Config.LoadInt(ScoreTotalString, out total, total);
    }

    public void SubTotalScore(int val)
    {
        if (val > 0 && total >= val)
        {
            total -= val;
            Config.SaveInt(ScoreTotalString, total);
        }
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
        return LevelLoader.Instance.mode == LevelLoader.GameMode.Moves ? bestMoves : bestTime;
    }

    void SetBest(int val)
    {
        if (LevelLoader.Instance.mode == LevelLoader.GameMode.Moves)
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
            Config.SaveInt(LevelLoader.Instance.mode == LevelLoader.GameMode.Moves ? ScoreBestMovesString : ScoreBestTimeString, current);
        }
        total += current;
        Config.SaveInt(ScoreTotalString, total);
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
