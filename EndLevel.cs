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

    private Text winLabel;
    private Text nextBtnLabel;

    private ScoreUI scoreLabel;
    private ScoreUI bestScoreLabel;
    private ScoreUI totalScoreLabel;

    private Text boostersLabel;

    EndLevel()
    {
        Events.LevelComplete.AddListener(Enable);
        Events.PlayerStatsUpdated.AddListener(UpdateUi);
    }

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();

        winLabel = transform.Find("BG/Win").GetComponent<Text>();
        nextBtnLabel = transform.Find("Buttons/Repeat/Text").GetComponent<Text>();

        scoreLabel = transform.Find("BG/Score/Score").GetComponent<ScoreUI>();
        bestScoreLabel = transform.Find("BG/Score/Best Score").GetComponent<ScoreUI>();
        totalScoreLabel = transform.Find("BG/Score/Total Score").GetComponent<ScoreUI>();
    }

    private void Start()
    {

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
            background.SetActive(true);
        }
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

    
    
}
