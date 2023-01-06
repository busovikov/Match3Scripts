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

    public GameObject startLevelPopup;
    public GameObject startBackground;

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
    private bool started = false;
    private Goals goals;

    public LevelManager()
    {
        Events.PlayerStatsUpdated.AddListener(UpdateUi);
    }
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
        if (!started)
            return true;

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

    internal void Start(Goals _goals)
    {
        goals = _goals;
        UpdateUi();
        startLevelPopup.SetActive(true);
        startBackground.SetActive(true);
    }

    public void OnStartConfirm()
    {
        startLevelPopup.SetActive(false);
        startBackground.SetActive(false);
        running = LevelLoader.mode != LevelLoader.GameMode.Moves;
        started = true;
    }

    public void UpdateUi()
    {
        if (goals == null)
            return;

        goals.UpdateUI();

        SetValue(goals.GetMovesForGameMode());
        Image img = startLevelPopup.transform.Find("Goal").GetComponent<Image>();
        Text txt = startLevelPopup.transform.Find("Amount").GetComponent<Text>();
        img.sprite = goals.GetSprite();
        txt.text = goals.GetAmount().ToString();
        stringLevel.Set(level);
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

    private void SetValue(int val)
    {
        if (LevelLoader.mode == LevelLoader.GameMode.Moves)
        {
            moves = val;
            label.text = Language.current._hud_play_moves;
            bonusHeader.text = BonusMovesString;
        }
        else
        {
            moves = val;
            label.text = Language.current._hud_play_time;
            bonusHeader.text = BonusTimeString;
        }
        stringValue.Set(moves);
    }


    
}
