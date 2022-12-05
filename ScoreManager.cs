using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : Singletone<ScoreManager>
{
    
    private static readonly string TriggerName = "Combo";

    private GameObject comboBonus;
    private Text comboBonusVal;
    private Animator comboBonusAnimator;
    private ScoreUI scoreUI;

    private GameObject buyCoinsPopup;

    [HideInInspector]
    public int current = 0;
    [HideInInspector]
    public int best
    {
        get { return LevelLoader.mode == LevelLoader.GameMode.Moves ? Config.GetStats().scoreBestMoves : Config.GetStats().scoreBestTime; }
        set 
        {
            if (LevelLoader.mode == LevelLoader.GameMode.Moves)
            {
                Config.SaveScoreBestMoves(value);
            }
            else
            {
                Config.SaveScoreBestTime(value);
            }
        }
    }
    [HideInInspector]
    public int total
    {
        get { return Config.GetStats().scoreTotal; }
        set { Config.SaveScoreTotal(value); }
    }
    [HideInInspector]
    public int coin
    {
        get { return Config.GetStats().coins; }
        set { Config.SaveScoreCoin(value); }
    }
    [HideInInspector]
    public DateTime lastAuthBonus
    {
        get { return Config.LoadLastAuthBonusDate(); }
        set { Config.SaveLastAuthBonusDate(value); }
    }

    public ScoreManager()
    {
        
    }

    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Init();
    }

    public void OnBuyCoinsPopupActivated()
    {
        buyCoinsPopup = GameObject.FindWithTag("BuyCoinsPopup");

        var CoinBuyButtonObj = GameObject.FindWithTag("BuyCoins5");
        if (CoinBuyButtonObj)
        {
            CoinBuyButtonObj.GetComponent<Button>().onClick.RemoveAllListeners();
            CoinBuyButtonObj.GetComponent<Button>().onClick.AddListener(BuyCoins5);
        }
        CoinBuyButtonObj = GameObject.FindWithTag("BuyCoins20");
        if (CoinBuyButtonObj)
        {
            CoinBuyButtonObj.GetComponent<Button>().onClick.RemoveAllListeners();
            CoinBuyButtonObj.GetComponent<Button>().onClick.AddListener(BuyCoins20);
        }
        CoinBuyButtonObj = GameObject.FindWithTag("BuyCoins100");
        if (CoinBuyButtonObj)
        {
            CoinBuyButtonObj.GetComponent<Button>().onClick.RemoveAllListeners();
            CoinBuyButtonObj.GetComponent<Button>().onClick.AddListener(BuyCoins100);
        }
    }

    public override void Init() 
    {
        comboBonus = GameObject.Find("Counts/Combo");
        if (comboBonus)
        {
            comboBonusVal = comboBonus.transform.Find("Val").GetComponent<Text>();
            comboBonusAnimator = comboBonus.GetComponent<Animator>();
        }
        var scoreObj = GameObject.Find("Counts/Score");
        if (scoreObj)
        {
            scoreUI = scoreObj.GetComponent<ScoreUI>();
        }
    }

    public void SetLastAuthBonus()
    {
        lastAuthBonus = DateTime.Now;
    }

    public bool AuthBonusAvailable()
    {
        return (DateTime.Now - lastAuthBonus).TotalDays > 1;
    }
    public void SubCoinScore(int val)
    {
        if (val > 0 && coin >= val)
        {
            coin -= val;
        }
    }

    [Serializable]
    class CoinObj
    {
        public int number;
        public string token;
    };

    public void AddCoinScoreJson(string json)
    {
        var val = JsonUtility.FromJson<CoinObj>(json);
        Debug.Log("Received Coin obj: " + json);

        if (val != null)
        {
            AddCoinScore(val.number);
#if PLATFORM_WEBGL && !UNITY_EDITOR
            if (val.token != "")
            {
                Yandex.ConsumePurchase(val.token);
            }
#endif
        }
    }
    public void AddCoinScore(int val)
    {
        Debug.Log("Got coins: " + val);
        coin += val;

        Events.PlayerStatsUpdated.Invoke();
        if (buyCoinsPopup) buyCoinsPopup.SetActive(false);
    }

    public void AddScore(int val)
    {
        if (val > 0 )
        {
            current += val;
            scoreUI.Set(current);
        }
    }

    public void SetTotalScore()
    {
        if (best < current)
        {
            best = current;
        }
        total += current;
        
#if PLATFORM_WEBGL && !UNITY_EDITOR
        Yandex.SetScoreToLeaderBoard(total);
#endif
    }

#if PLATFORM_WEBGL
    private void BuyCoins5()
    {
#if UNITY_EDITOR
        Debug.Log("Yandex.BuyCoins((int)Yandex.CoinsOptions.coins5);");
#else
        Yandex.BuyCoins((int)Yandex.CoinsOptions.coins5);
#endif
    }
    private void BuyCoins20()
    {
#if UNITY_EDITOR
        Debug.Log("Yandex.BuyCoins((int)Yandex.CoinsOptions.coins5);");
#else
        Yandex.BuyCoins((int)Yandex.CoinsOptions.coins20);
#endif
    }
    private void BuyCoins100()
    {
#if UNITY_EDITOR
        Debug.Log("Yandex.BuyCoins((int)Yandex.CoinsOptions.coins5);");
#else
        Yandex.BuyCoins((int)Yandex.CoinsOptions.coins100);
#endif
    }
#endif
    internal void AddCombo(int count)
    {
        if (count > 0)
        {
            comboBonusVal.text = (++count * 10).ToString();
            comboBonusAnimator.SetTrigger(TriggerName);
        }
    }
}
