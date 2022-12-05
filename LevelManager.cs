using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private static readonly string TriggerName = "Bonus";
    private static readonly string BonusMovesString = "More moves  +";
    private static readonly string BonusTimeString = "More time  +";

    private Text bonusVal;
    private Text bonusHeader;
    private Animator bonusAnimator;
    public ScoreUI stringValue;
    public ScoreUI stringLevel;
    
    private float accumulator = 0;

    [HideInInspector]
    public int moves = 0;
    [HideInInspector]
    public int level
    {
        get 
        { 
            return LevelLoader.mode == LevelLoader.GameMode.Moves ? Config.GetStats().levelMoves : Config.GetStats().levelTime; 
        }
        set 
        {
            if (LevelLoader.mode == LevelLoader.GameMode.Moves)
            {
                Config.SaveLevelMoves(value);
#if PLATFORM_WEBGL && !UNITY_EDITOR
        Yandex.SetBestMovesToLeaderBoard(value);
#endif
            }
            else
            {
                Config.SaveLevelTime(value);
#if PLATFORM_WEBGL && !UNITY_EDITOR
        Yandex.SetBestTimeToLeaderBoard(value);
#endif
            }
        }
    }
    public bool running = false;

    public Text label;
    public GameObject bonus;

    // Start is called before the first frame update
    void Awake()
    {
        bonusHeader = bonus.transform.Find("Header").GetComponent<Text>();
        bonusVal = bonus.transform.Find("Val").GetComponent<Text>();
        bonusAnimator = bonus.GetComponent<Animator>();
    }

    private void Start()
    {
        stringLevel.Set(level);
    }

    public void NextLevel()
    {
        level++;
        stringLevel.Set(level);
        stringValue.Set(0);
    }

     public bool Check()
    {
        if (running && moves > 0)
        {
            accumulator += Time.deltaTime;
            if (accumulator >= 1)
            {
                accumulator -= 1;
                SubMoves(1);
            }
        }
        return moves > 0;
    }

    public void AddMoves(int val)
    {
        moves += val;
        stringValue.Set(moves);
        bonusVal.text = val.ToString();
        bonusAnimator.SetTrigger(TriggerName);
    }

    public void SubMoves(int val)
    {
        moves -= val;
        stringValue.Set(moves);
        if (running && moves <= 0)
        {
            running = false;
        }
    }

    public void StartAsMoves(int val)
    {
        SetValue(val);
        label.text = Language.current._hud_play_moves;
        bonusHeader.text = BonusMovesString;
    }

    private void SetValue(int val)
    {
        moves = val;
        stringValue.Set(moves);
    }

    public void StartAsSeconds(int seconds)
    {
        SetValue(seconds);
        label.text = Language.current._hud_play_time;
        bonusHeader.text = BonusTimeString;
        running = true;
    }

    
}
