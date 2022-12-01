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
    
    public GameObject endLevelPopup;

    public GameMode mode = GameMode.Time;
    public float trnsactionTime = 1f;
    public int levelMoves = 0;
    public int levelTime = 0;
    public static LevelLoader Instance;
    
    private Animator animator;
    private SoundManager soundManager;

    private DateTime lastAd;
    private static bool tryToRate = true;
    private static bool lastWin = false;
    private static int nextLevel = 0;
    void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            animator = GetComponent<Animator>();
            lastAd = DateTime.Now;
        }
        else if (Instance != this)
        {
            // TODO: make endgame manager
            Instance.endLevelPopup = endLevelPopup;
            Instance.soundManager = soundManager;
            Instance.animator.Rebind();
            Instance.animator.ResetTrigger("Out");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    public static void Exit()
    {
        Application.Quit();
    }

    public static void StartGameWithMoves()
    {
        Instance.mode = GameMode.Moves;
        Instance.StartCoroutine(Instance.MakeTransactionToQuickGame());
    }

    public static void StartGameWithTime()
    {
        Instance.mode = GameMode.Time;
        Instance.StartCoroutine(Instance.MakeTransactionToQuickGame());
    }

    public static void RepeatLevel()
    {
        Instance.StartCoroutine(Instance.MakeTransactionToQuickGame());
    }

    public static void GoToMainMenu()
    {
        Instance.StartCoroutine(Instance.MakeTransactionToMenu());
    }

    public static void EndLevel(bool win, int next_level)
    {
        Instance.endLevelPopup.GetComponent<EndLevel>().Enable(win);
        Instance.soundManager.PlayPopupSound();
        lastWin = win;
        nextLevel = next_level;
    }

    IEnumerator MakeTransactionToQuickGame()
    {
#if PLATFORM_WEBGL
        if (tryToRate && lastWin && nextLevel > 2)
        {
            tryToRate = false;
            Yandex.RateGame();
        }
        else if ( !tryToRate && nextLevel > 3 && System.DateTime.Now - lastAd > TimeSpan.FromMinutes(3))
        {
            lastAd = System.DateTime.Now;
            Yandex.ShowFullScreenAdv();
        }
#endif

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
