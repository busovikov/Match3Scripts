using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndLevel : MonoBehaviour
{

    public GameObject scoreObject;
    private ScoreManager score;

    public GameObject boostersObject;
    private Boosters boosters;
    private Text[] boosterCount;
    private Text[] boosterPrice;

    private Text winLabel;
    private Text nextBtnLabel;

    private Text scoreLabel;
    private Text bestScoreLabel;
    private Text totalScoreLabel;
    
    private Text boostersLabel;

    
    
    private void Awake()
    {
        score = scoreObject.GetComponent<ScoreManager>();
        boosters = boostersObject.GetComponent<Boosters>();

        boosterCount = new Text[(int)Boosters.BoosterType.Count];
        boosterPrice = new Text[(int)Boosters.BoosterType.Count];

        Transform popupBoosters = transform.Find("BG/Boosters").transform;
        int size = Mathf.Min(popupBoosters.childCount, (int)Boosters.BoosterType.Count);
        for (int i = 0; i < size; i++)
        { 
            boosterCount[i] = popupBoosters.GetChild(i).Find("Count").GetComponent<Text>();
            boosterPrice[i] = popupBoosters.GetChild(i).Find("Price").GetComponent<Text>();
        }

        boosters.FillAmount(boosterCount);
        boosters.FillPrice(boosterPrice);

        winLabel = transform.Find("BG/Win").GetComponent<Text>();
        nextBtnLabel = transform.Find("BG/Buttons/Repeat/Text").GetComponent<Text>();

        scoreLabel = transform.Find("BG/Score/Score").GetComponent<Text>();
        bestScoreLabel = transform.Find("BG/Score/Best Score").GetComponent<Text>();
        totalScoreLabel = transform.Find("BG/Score/Total Score").GetComponent<Text>();
    }

    private void OnEnable()
    {
        scoreLabel.GetComponent<ScoreUI>().Set(score.current);
        bestScoreLabel.GetComponent<ScoreUI>().Set(score.GetBest());
        totalScoreLabel.GetComponent<ScoreUI>().Set(score.total);
    }

    public void Enable(bool win)
    {
        gameObject.SetActive(true);
        if (win)
        {
            winLabel.text = Language.current._popup_win_label;
            nextBtnLabel.text = Language.current._menu_play_next;
        }
        else
        {
            winLabel.text = Language.current._popup_lose_label;
            nextBtnLabel.text = Language.current._menu_play_repeate;
        }
        GetComponent<Animator>().SetTrigger("LevelEnd");
    }

    private bool DealOn(Boosters.BoosterType type)
    {
        int price = boosters.Price(type);
        if (score.total >= price)
        {
            boosterCount[boosters.Index(type)].text = boosters.AddBooster(type).ToString();
            score.SubTotalScore(price);
            totalScoreLabel.GetComponent<ScoreUI>().Set(score.total);
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
