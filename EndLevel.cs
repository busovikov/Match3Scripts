using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndLevel : MonoBehaviour
{
    public GameObject background;
    public GameObject scull;
    public GameObject stars;
    public GameObject scoreObject;
    private ScoreManager score;
    private SoundManager soundManager;


    public GameObject boostersObject;
    private Boosters boosters;
    private Text[] boosterCount;
    private Text[] boosterPrice;

    private Text winLabel;
    private Text nextBtnLabel;

    private ScoreUI scoreLabel;
    private ScoreUI bestScoreLabel;
    private ScoreUI totalScoreLabel;
    private ScoreUI coinScoreLabel;

    private Text boostersLabel;

    EndLevel()
    {
        Events.LevelComplete.AddListener(Enable);
    }

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();

        score = scoreObject.GetComponent<ScoreManager>();
        boosters = boostersObject.GetComponent<Boosters>();

        boosterCount = new Text[(int)Boosters.BoosterType.Count];
        boosterPrice = new Text[(int)Boosters.BoosterType.Count];

        Transform popupBoosters = transform.Find("Boosters").transform;
        int size = Mathf.Min(popupBoosters.childCount, (int)Boosters.BoosterType.Count);
        for (int i = 0; i < size; i++)
        { 
            boosterCount[i] = popupBoosters.GetChild(i).Find("Count").GetComponent<Text>();
            boosterPrice[i] = popupBoosters.GetChild(i).Find("Price").GetComponent<Text>();
        }

        boosters.FillAmount(boosterCount);
        boosters.FillPrice(boosterPrice);

        winLabel = transform.Find("BG/Win").GetComponent<Text>();
        nextBtnLabel = transform.Find("Buttons/Repeat/Text").GetComponent<Text>();

        scoreLabel = transform.Find("BG/Score/Score").GetComponent<ScoreUI>();
        bestScoreLabel = transform.Find("BG/Score/Best Score").GetComponent<ScoreUI>();
        totalScoreLabel = transform.Find("BG/Score/Total Score").GetComponent<ScoreUI>();
        coinScoreLabel = transform.Find("Coins/Label/CoinLabel").GetComponent<ScoreUI>();
    }

    private void OnEnable()
    {
        scoreLabel.Set(score.current);
        bestScoreLabel.Set(score.GetBest());
        totalScoreLabel.Set(score.total);
        coinScoreLabel.Set(score.coin);
        background.SetActive(true);
    }

    private void OnDisable()
    {
        background.SetActive(false);
    }

    public void Enable(bool win, int nextLevel)
    {
        gameObject.SetActive(true);
        
        soundManager.PlayPopupSound();

        if (win)
        {
            scull.SetActive(false);
            stars.SetActive(true);

            winLabel.text = Language.current._popup_win_label;
            nextBtnLabel.text = Language.current._menu_play_next;
        }
        else
        {
            scull.SetActive(true);
            stars.SetActive(false);

            winLabel.text = Language.current._popup_lose_label;
            nextBtnLabel.text = Language.current._menu_play_repeate;
        }
        GetComponent<Animator>().SetTrigger("LevelEnd");
    }

    private bool DealOn(Boosters.BoosterType type)
    {
        int price = boosters.Price(type);
        if (score.coin >= price)
        {
            boosterCount[boosters.Index(type)].text = boosters.AddBooster(type).ToString();
            score.SubCoinScore(price);
            coinScoreLabel.Set(score.coin);
            return true;
        }
        return false;
    }
    public void BuyBooster(string type)
    {
        bool deal = false;
        if (type == "shuffle")
        {
            deal = DealOn(Boosters.BoosterType.Mix);
        }
        else if (type == "erase")
        {
            deal = DealOn(Boosters.BoosterType.Erase);
        }
        else if (type == "add")
        {
            deal = DealOn(Boosters.BoosterType.Add);
        }

        // todo shop
    }
    
}
