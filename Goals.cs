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
    public static TileMap.BasicTileType type;
    [HideInInspector]
    public static Goals goals;
    private Animator animator;
    private LevelLoader.GameMode gameMode;
    private int goal = 9;
    private int next = 0;

    public bool reached = false;

    private void Awake()
    {
        var t = UnityEngine.Random.Range(0, (int)TileMap.BasicTileType.TypeSize);
        type = (TileMap.BasicTileType)t;
        goals = this;
        GetComponent<Image>().sprite = images[t];
        animator = GetComponent<Animator>();
    }

    public int GetGoalForGameMode(LevelLoader.GameMode gm)
    {
        int moves = 0;
        gameMode = gm;
        if (gm == LevelLoader.GameMode.Time)
        {
            Config.LoadInt(goalTimeString, out goal, goal);
            moves = (goal / 3) * 5;
        }
        else
        {
            Config.LoadInt(goalMovesString, out goal, goal);
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

    public IEnumerator ToGoal(GameObject obj)
    {
        float elapsed = 0f;
        float duration = 0.75f;
        var InitialOffset = obj.transform.position;

        while (elapsed < duration)
        {
            obj.transform.position = Vector2.Lerp(InitialOffset, transform.position, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = transform.position;
        obj.SetActive(false);
    }

    public void Add(GameObject gameObject)
    {
        animator.SetTrigger("Add");
        gameObject.SetActive(false);
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
                Config.SaveInt(goalTimeString, next);
            }
            else
            {
                Config.SaveInt(goalMovesString, next);
            }
            reached = true;
        }
    }
}
