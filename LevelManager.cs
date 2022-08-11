using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LevelManager : MonoBehaviour
{
    public abstract class LevelContext
    {
        public LevelContext(Goals _goals, int _count)
        {
            Count = _count;
            goals = _goals;
        }
        private bool levelDone = false;
        private Goals goals;
        private int count = 0;

        public int Count { get => count; set => count = value; }

        public virtual bool Done() 
        {
            if (!levelDone && (Count <= 0 || goals.reached))
            {
                levelDone = true;
                return levelDone;
            }
            return false; 
        }

        public virtual void AddCount(int val)
        {
            Count += val;
        }
        public virtual void SubCount(int val)
        {
            Count -= val;
        }
    }

    class MovesContext : LevelContext
    {
        public MovesContext(Goals _goals, int _count) : base(_goals, _count)
        {
        }
    }
    class TimeContext : LevelContext
    {
        public override bool Done() 
        {
            if (!base.Done())
            {
                accumulator += Time.deltaTime;
                if (accumulator >= 1)
                {
                    accumulator -= 1;
                    SubCount(1);
                }
            }
            return base.Done(); 
        }

        public override void SubCount(int val)
        {
            base.SubCount(val);
        }
        private float accumulator = 0;

        public TimeContext(Goals _goals, int _count) : base(_goals, _count)
        {
        }
    }
    private static readonly string TriggerName = "Bonus";
    private static readonly string LevelMovesString = "Level.Moves";
    private static readonly string LevelTimesString = "Level.Times";
    private static readonly string MovesString = "Moves";
    private static readonly string TimeString = "Time";
    private static readonly string BonusMovesString = "More moves  +";
    private static readonly string BonusTimeString = "More time  +";

    private Text bonusVal;
    private Text bonusHeader;
    private Animator bonusAnimator;
    public ScoreUI stringValue;
    public ScoreUI stringLevel;
    private ScoreManager score;
    private LevelContext context;
    [SerializeField]private Goals goals;


    [HideInInspector]
    public int moves = 0;
    [HideInInspector]
    public int level;
    [HideInInspector]

    public Text label;
    public GameObject bonus;

    // Start is called before the first frame update
    void Awake()
    {
        bonusHeader = bonus.transform.Find("Header").GetComponent<Text>();
        bonusVal = bonus.transform.Find("Val").GetComponent<Text>();
        bonusAnimator = bonus.GetComponent<Animator>();
        level = 1;
    }

    private void Start()
    {
        score = FindObjectOfType<ScoreManager>();
        var movesOrTime = goals.GetGoalForGameMode(LevelLoader.Instance.mode);
        if (LevelLoader.Instance.mode == LevelLoader.GameMode.Moves)
        {
            StartAsMoves(LevelLoader.Instance.levelMoves > 0 ? LevelLoader.Instance.levelMoves : movesOrTime);
        }
        else
        {
            StartAsSeconds(LevelLoader.Instance.levelTime > 0 ? LevelLoader.Instance.levelTime : movesOrTime);
        }
        string LevelString = LevelLoader.Instance.mode == LevelLoader.GameMode.Moves ? LevelMovesString : LevelTimesString;
        Config.LoadInt(LevelString, out this.level, this.level);
        stringLevel.Set(level);
    }

    private void Update()
    {
        if (context.Done())
        {
            score.AddScore(moves * 10);

            if (goals.reached)
            {
                score.SetTotalScore();
                NextLevel();
            }
            else
                score.current = 0;
            LevelLoader.EndLevel(goals.reached);
        }
    }

    public void NextLevel()
    {
        level++;
        string LevelString = LevelLoader.Instance.mode == LevelLoader.GameMode.Moves ? LevelMovesString : LevelTimesString;
        Config.SaveInt(LevelString, level);
        stringLevel.Set(level);
        stringValue.Set(0);
    }

    public void AddMoves(int val)
    {
        context.AddCount(val);
        stringValue.Set(context.Count);
        bonusVal.text = val.ToString();
        bonusAnimator.SetTrigger(TriggerName);
    }

    public void SubMoves(int val)
    {
        context.SubCount(val);
        stringValue.Set(context.Count);
        
    }

    public void StartAsMoves(int val)
    {
        stringValue.Set(val);
        label.text = MovesString;
        bonusHeader.text = BonusMovesString;
        context = new MovesContext(goals, val);
    }

    public void StartAsSeconds(int seconds)
    {
        stringValue.Set(seconds);
        label.text = TimeString;
        bonusHeader.text = BonusTimeString;
        context = new TimeContext(goals, seconds);
    }

    
}
