using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public enum GameMode
    {
        Time,
        Moves
    }
    
    public static float trnsactionTime = 1f;
    public static int levelMoves = 0;
    public static int levelTime = 0;
    public static LevelLoader Instance;
    
    private Animator animator;


    public static GameMode mode = GameMode.Time;
    
    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            animator = GetComponent<Animator>();
        }
        else if (Instance != this)
        {
            // TODO: make endgame manager
            Instance.animator.Rebind();
            Instance.animator.ResetTrigger("Out");
            Destroy(gameObject);
        }
    }

    public static void Exit()
    {
        Application.Quit();
    }

    public static void StartGameWithMoves()
    {
        Instance.StopAllCoroutines();
        mode = GameMode.Moves;
        Instance.StartCoroutine(Instance.MakeTransactionToQuickGame());
    }

    public static void StartGameWithTime()
    {
        Instance.StopAllCoroutines();
        mode = GameMode.Time;
        Instance.StartCoroutine(Instance.MakeTransactionToQuickGame());
    }

    public static void RepeatLevel()
    {
        Instance.StopAllCoroutines();
        Instance.StartCoroutine(Instance.MakeTransactionToQuickGame());
    }

    public static void GoToMainMenu()
    {
        Instance.StopAllCoroutines();
        Instance.StartCoroutine(Instance.MakeTransactionToMenu());
    }

    IEnumerator MakeTransactionToQuickGame()
    {
        Debug.Log("MakeTransactionToQuickGame");
        Advert.Instance.ShowAd();
        Debug.Log("Advert.Instance.ShowAd();");
        yield return Advert.Instance.WaitForAdvertDone();
        Debug.Log("Advert.Instance.WaitForAdvertDone()");

        animator.SetTrigger("Out");

        yield return new WaitForSeconds(trnsactionTime);

        SceneManager.LoadScene("Quick Game");
    }

    IEnumerator MakeTransactionToMenu()
    {
        animator.SetTrigger("Out");

        yield return new WaitForSeconds(trnsactionTime);

        SceneManager.LoadScene("Main Menu");
    }

    // Update is called once per frame
    void Update()
    {
        // Make sure user is on Android platform
        if (Application.platform == RuntimePlatform.Android)
        {
            // Check if Back was pressed this frame
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (SceneManager.GetActiveScene().buildIndex == 0)
                {
                    Exit();
                }
                else 
                {
                    GoToMainMenu();
                }
            }
        }
    }
}
