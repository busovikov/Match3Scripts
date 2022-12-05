﻿using System;
using System.Collections;
using UnityEngine;


public class Advert : Singletone<Advert>
{

    private DateTime lastAd;
    private bool tryToRate = false;
    private bool lastWin = false;
    private int nextLevel = 0;

    private bool isActive = false;

    public Advert()
    {
        Events.LevelComplete.AddListener(OnLevelEnd);
        lastAd = DateTime.Now;
        CheckForReview();
    }

    public void CheckForReview()
    {
#if PLATFORM_WEBGL && !UNITY_EDITOR
        Yandex.IsPlayerAbleReview();
#endif
    }

    void OnAdvertComplete()
    {
        isActive = false;
    }

    void OnCanReview(int val)
    {
        tryToRate = val != 0;
    }

    void OnLevelEnd(bool win, int level)
    {
        lastWin = win;
        nextLevel = level;
    }

    public IEnumerator WaitForAdvertDone()
    {
        var until = new WaitUntil(() => isActive == false);
        yield return until;
    }
    public void ShowAd()
    {
#if PLATFORM_WEBGL && !UNITY_EDITOR
        if (tryToRate && lastWin && nextLevel > 2 && Yandex.IsPlayerAuthorized())
        {
            tryToRate = false;
            Yandex.RateGame();
            isActive = true;
        }
        else if (nextLevel > 3 && DateTime.Now - lastAd > TimeSpan.FromMinutes(2))
        {
            Yandex.ShowFullScreenAdv();
            lastAd = DateTime.Now;
            isActive = true;
        }
        
#endif
    }
}
