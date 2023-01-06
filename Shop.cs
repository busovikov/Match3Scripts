using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public Shop()
    {
        Events.PlayerStatsUpdated.AddListener(UpdateUi);
        Events.PlayerInitialized.AddListener(PlayerInitialized);
        Events.PlayerAuthorized.AddListener(PlayerAuthorized);
    }

    public GameObject boostersObject;
    private Boosters boosters;
    private Text[] boosterCount;
    private Text[] boosterPrice;

    private ScoreUI coinScoreLabel;
    private GameObject authButton;
    private Button authBtn;

    private bool auth = false;

    private void Awake()
    {
        // soundManager = FindObjectOfType<SoundManager>();

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

        coinScoreLabel = transform.Find("Coins/Label/CoinLabel").GetComponent<ScoreUI>();

        authButton = transform.Find("AuthButton").gameObject;
        authBtn = authButton.transform.GetChild(0).GetComponent<Button>();
    }

    void Start()
    {
#if PLATFORM_WEBGL && !UNITY_EDITOR
        auth = !Yandex.GetNoAuth();
#endif
    }

    public void ActivateBuyCoinsPopup()
    {
        ScoreManager.Instance.OnBuyCoinsPopupActivated();

    }
    public void UpdateUi()
    {
        if (gameObject.activeSelf)
        {
            coinScoreLabel.Set(ScoreManager.Instance.coin);
            authButton.SetActive(auth == false);
        }
        boosters.FillAmount(boosterCount);
        boosters.FillPrice(boosterPrice);
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
        auth = _auth;
    }

    private void PlayerAuthorized(bool _auth)
    {
        auth = _auth;
        authBtn.interactable = !_auth;
        UpdateUi();
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
