using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject credits;
    public GameObject buttons;
    public GameObject ghost;
    public GameObject lootBox;
    public ScoreManager scoreManager;
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
#if UNITY_EDITOR
        lootBox.SetActive(Yandex.auth_dumm == false && scoreManager.AuthBonusAvailable());
#else
#if PLATFORM_WEBGL
        lootBox.SetActive(Yandex.IsPlayerAuthorized() == false && scoreManager.AuthBonusAvailable());
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
#endif
#endif
        }
        else
        {
            lootBox.Play("Loot Box Open");
            scoreManager.AddCoinScore(1);
            scoreManager.SetLastAuthBonus();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
