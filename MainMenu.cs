using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject credits;
    public GameObject buttons;
    public GameObject ghost;
    public GameObject lootBox;

    Button lootBoxButton;

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

    private void Awake()
    {
        Events.PlayerInitialized.AddListener(PlayerInitialized);
        Events.PlayerAuthorized.AddListener(PlayerAuthorized);
    }
    void Start()
    {
        Config.GetStats();
    }


    public void OpenLootBox(Animator lootBox)
    {
        if (openLootBox == null)
        {
            openLootBox = lootBox;
#if UNITY_EDITOR
            PlayerAuthorized(true);
#else
#if PLATFORM_WEBGL
            Yandex.AuthorizePlayer();
#endif
#endif
        }

    }

    public void OpenLootBox(Button button)
    {
        lootBoxButton = button;
        lootBoxButton.interactable = false;
    }

    public void ButtonPlayerReset()
    {
        Config.Instance.Reset();
    }
    private void PlayerInitialized(bool auth)
    {
        lootBox.SetActive(auth == false && ScoreManager.Instance.AuthBonusAvailable());
    }
    private void PlayerAuthorized(bool auth)
    {
        Debug.Log("MainMenu event PlayerAuthorized");
        lootBoxButton.interactable = !auth;
        if (openLootBox != null && auth)
        {
            if (!ScoreManager.Instance.AuthBonusAvailable())
            {
                lootBox.SetActive(false);
            }
            else
            {
                openLootBox.Play("Loot Box Open");
                lootBoxButton.gameObject.SetActive(false);
                ScoreManager.Instance.AddCoinScore(1);
                ScoreManager.Instance.SetLastAuthBonus();
            }
        }
        openLootBox = null;
    }
}
