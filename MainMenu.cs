using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject credits;
    public GameObject buttons;
    public GameObject ghost;
    public GameObject lootBox;

    Animator openLootBox = null;
    public void ToCredits()
    {
        buttons.SetActive(false);
        ghost.SetActive(false);
        credits.SetActive(true);
    }

    public void BackToMainMenu()
    {
        buttons.SetActive(true);
        ghost.SetActive(true);
        credits.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        Config.GetStats();
        Events.PlayerInitialized.AddListener(PlayerInitialized);
#if UNITY_EDITOR
        lootBox.SetActive(Yandex.auth_dumm == false && ScoreManager.Instance.AuthBonusAvailable());
#else
#if PLATFORM_WEBGL
        lootBox.SetActive(Yandex.IsPlayerAuthorized() == false && ScoreManager.Instance.AuthBonusAvailable());
#endif
#endif

    }


    public void OpenLootBox(Animator lootBox)
    {
        bool auth = false;
#if UNITY_EDITOR
        auth = Yandex.auth_dumm;
#else
#if PLATFORM_WEBGL
        auth = Yandex.IsPlayerAuthorized();
#endif
#endif
        if (auth == false)
        {
#if UNITY_EDITOR
            Yandex.auth_dumm = true;
#else
#if PLATFORM_WEBGL
            Yandex.AuthorizePlayer();
            openLootBox = lootBox;
#endif
#endif
        }

    }

    public void ButtonPlayerReset()
    {
        Config.Instance.Reset();
    }
    private void PlayerInitialized()
    {
        if (openLootBox != null && Yandex.IsPlayerAuthorized())
        {
            if (!ScoreManager.Instance.AuthBonusAvailable())
            {
                lootBox.SetActive(false);
            }
            else
            {
                openLootBox.Play("Loot Box Open");
                openLootBox = null;
                ScoreManager.Instance.AddCoinScore(1);
                ScoreManager.Instance.SetLastAuthBonus();
            }
        }
    }
}
