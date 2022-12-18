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

    private GameObject authButton;
    private Button authBtn;

    private bool auth = false;

    private Text boostersLabel;

    EndLevel()
    {
        Events.LevelComplete.AddListener(Enable);
        Events.PlayerStatsUpdated.AddListener(UpdateUi);
        Events.PlayerInitialized.AddListener(PlayerInitialized);
        Events.PlayerAuthorized.AddListener(PlayerAuthorized);
    }

    public void ActivateBuyCoinsPopup()
    {
        ScoreManager.Instance.OnBuyCoinsPopupActivated();

    }

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();

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

        authButton = transform.Find("Coins/AuthButton").gameObject;
        authBtn = authButton.transform.GetChild(0).GetComponent<Button>();

#if PLATFORM_WEBGL && !UNITY_EDITOR
        auth = !Yandex.GetNoAuth();
#endif
    }

    private void OnEnable()
    {
        UpdateUi();
    }

    public void UpdateUi()
    {
        if (gameObject.activeSelf)
        {
            Debug.Log("UpdateUi()");
            scoreLabel.Set(ScoreManager.Instance.current);
            bestScoreLabel.Set(ScoreManager.Instance.best);
            totalScoreLabel.Set(ScoreManager.Instance.total);
            coinScoreLabel.Set(ScoreManager.Instance.coin);
            background.SetActive(true);

            authButton.SetActive(auth == false);
        }
    }

    public void Auth()
    {
        authBtn.interactable = false;
#if UNITY_EDITOR
        PlayerAuthorized(true);
#else
#if PLATFORM_WEBGL
        Yandex.AuthorizePlayer();
#endif
#endif
    }

    private void PlayerInitialized(bool _auth)
    {
        Debug.Log("EndLevel event PlayerInitialized " + auth);
        auth = _auth;
    }

    private void PlayerAuthorized(bool _auth)
    {
        Debug.Log("EndLevel event PlayerAuthorized");
        auth = _auth;
        authBtn.interactable = !_auth;
        UpdateUi();
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
        if (ScoreManager.Instance.coin >= price)
        {
            boosterCount[boosters.Index(type)].text = boosters.AddBooster(type).ToString();
            ScoreManager.Instance.SubCoinScore(price);
            coinScoreLabel.Set(ScoreManager.Instance.coin);
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
