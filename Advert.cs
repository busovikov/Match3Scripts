using System;
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
    }

    private void Start()
    {
#if PLATFORM_WEBGL && !UNITY_EDITOR
        Yandex.CanReview();
#endif
    }

    void OnAdvertComplete()
    {
        Debug.Log("OnAdvertComplete()");
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
        if (isActive)
            return;
        TimeSpan t = DateTime.Now - lastAd;

        if (tryToRate && lastWin && nextLevel > 2)
        {
            tryToRate = false;
            isActive = true;
#if PLATFORM_WEBGL && !UNITY_EDITOR
            Yandex.RateGame();
#endif
        }
        else if (nextLevel > 3 && DateTime.Now - lastAd > TimeSpan.FromMinutes(2))
        {
            isActive = true;
            lastAd = DateTime.Now;
#if PLATFORM_WEBGL && !UNITY_EDITOR
            Yandex.ShowFullScreenAdv();
#endif
        }
        
    }
}
