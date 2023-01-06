using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Goals : MonoBehaviour
{
    public Sprite[] images;
    public Text label;

    [HideInInspector]
    public int type;
    private Animator animator;

    private int IncCoef { get { return LevelLoader.mode == LevelLoader.GameMode.Moves ? 2 : 10; } }
    private int goal
    {
        get { return LevelLoader.mode == LevelLoader.GameMode.Moves ? Config.GetStats().goalsMoves : Config.GetStats().goalsTime; }
        set 
        {
            if (LevelLoader.mode == LevelLoader.GameMode.Time)
            {
                Config.SaveGoalTime(value);
            }
            else
            {
                Config.SaveGoalMoves(value);
            }
        }
    }
    private int current = 0;

    public bool reached = false;

    private void Awake()
    {
        type = UnityEngine.Random.Range(0, images.Length);
        GetComponent<Image>().sprite = images[type];
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        UpdateUI();
    }
    public void UpdateUI()
    {
        current = goal;
        label.text = current.ToString();
    }

    public int GetMovesForGameMode()
    {
        return goal / 3 * IncCoef;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        animator.SetTrigger("Add");
        collision.gameObject.SetActive(false);
        current--;
        if (current > 0)
        {
            label.text = current.ToString();
        }
        else if (!reached)
        {
            label.text = 0.ToString();
            goal += 3;
            reached = true;
        }
    }

    internal Sprite GetSprite()
    {
        return GetComponent<Image>().sprite;
    }

    internal int GetAmount()
    {
        return goal;
    }
}
