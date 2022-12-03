using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Goals : MonoBehaviour
{
    static readonly string goalTimeString = "Goal.Time";
    static readonly string goalMovesString = "Goal.Moves";

    public Sprite[] images;
    public Text label;

    [HideInInspector]
    public int type;
    private Animator animator;
    private LevelLoader.GameMode gameMode;
    private int goal = 9;
    private int next = 0;

    public bool reached = false;

    private void Awake()
    {
        type = UnityEngine.Random.Range(0, images.Length);
        GetComponent<Image>().sprite = images[type];
        animator = GetComponent<Animator>();
    }

    public int GetGoalForGameMode(LevelLoader.GameMode gm)
    {
        int moves = 0;
        gameMode = gm;
        if (gm == LevelLoader.GameMode.Time)
        {
            Config.Instance.LoadGoalTime(out goal);
            moves = (goal / 3) * 5;
        }
        else
        {
            Config.Instance.LoadGoalMoves(out goal);
            moves = (goal / 3) * 2;
        }
        next = goal + 3;
        label.text = goal.ToString();
        return moves;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        animator.SetTrigger("Add");
        collision.gameObject.SetActive(false);
        goal--;
        if (goal > 0)
        {
            label.text = goal.ToString();
        }
        else if (!reached)
        {
            label.text = 0.ToString();
            if (gameMode == LevelLoader.GameMode.Time)
            {
                Config.SaveGoalTime(next);
            }
            else
            {
                Config.SaveGoalMoves(next);
            }
            reached = true;
        }
    }
}
