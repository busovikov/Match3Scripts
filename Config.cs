using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Config : Singletone<Config>
{
    [Serializable]
    public class PlayerStats
    {
        public string lastAuthBonusDate = "";
        public int coins;
               
        public int scoreBestMoves;
        public int scoreBestTime;
        public int scoreTotal;
               
        public int goalsMoves = 9;
        public int goalsTime = 9;
        public int levelMoves = 1;
        public int levelTime = 1;
               
        public int[] boosters = new int[(int)Boosters.BoosterType.Count];

        public PlayerStats()
        {
            for (int i = 0; i < (int)Boosters.BoosterType.Count; i++)
            {
                boosters[i] = 1;
            }
        }

        public void Load()
        {
#if UNITY_EDITOR
            string data;
            LoadString(typeof(PlayerStats).Name, out data, JsonUtility.ToJson(Instance.playerStats));
            Instance.playerStats = JsonUtility.FromJson<PlayerStats>(data);
#else
#if PLATFORM_WEBGL
            // Keys(typeof(PlayerStats).Name) not used at this time 
            Yandex.LoadData();
#endif
#endif
        }
        public void Save()
        {
            var data = JsonUtility.ToJson(Instance.playerStats);

#if UNITY_EDITOR
            SaveString(typeof(PlayerStats).Name, data);
#else
#if PLATFORM_WEBGL
            Yandex.SaveData(data);
#endif
#endif

        }
    };

    PlayerStats playerStats = new PlayerStats();

    public void Reset()
    {
        playerStats = new PlayerStats();
        playerStats.Save();
    }
    public static PlayerStats GetStats()
    {
        return Instance.playerStats;
    }

    private static readonly string Ver = "Ver.1.0.";
    static StringBuilder builder = new StringBuilder();

    float timer = 0;
    void SetTimer()
    {
        timer = 2;
    }

    public override void Init()
    {
        playerStats.Load();
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                playerStats.Save();
            }
        }
    }

#if PLATFORM_WEBGL && !UNITY_EDITOR
#endif
    public void PlayerStatsReceived(string data)
    {
        var received = JsonUtility.FromJson<PlayerStats>(data);
        Debug.Log(data);
        if (received != null)
        {
            Instance.playerStats = received;
        }
        Events.PlayerInitialized.Invoke();
    }

    public void PlayerReinitialized()
    {
        playerStats.Load();
        Advert.Instance.CheckForReview();
    }

    static string Keys(params string[] list)
    {
        StringBuilder arrBuilder = new StringBuilder();
        arrBuilder.Append("[");
        foreach (string s in list)
        {
            arrBuilder.AppendFormat("\"{0}\",", s);
        }
        arrBuilder.Remove(arrBuilder.Length - 1, 1);
        arrBuilder.Append("]");
        string str = arrBuilder.ToString();
        arrBuilder.Clear();
        return str;
    }
    internal void LoadGoalTime(out int goal)
    {
        goal = Instance.playerStats.goalsTime;
    }

    internal void LoadGoalMoves(out int goal)
    {
        goal = Instance.playerStats.goalsMoves;
    }

    internal static void SaveGoalTime(int next)
    {
        Instance.playerStats.goalsTime = next;
        Instance.SetTimer();
    }

    internal static void SaveLevelTime(int level)
    {
        Instance.playerStats.levelTime = level;
        Instance.SetTimer();
    }

    internal static void SaveScoreCoin(int coin)
    {
        Instance.playerStats.coins = coin;
        Instance.SetTimer();
    }

    internal static void SaveLevelMoves(int level)
    {
        Instance.playerStats.levelMoves = level;
        Instance.SetTimer();
    }

    internal static void SaveGoalMoves(int next)
    {
        Instance.playerStats.goalsMoves = next;
        Instance.SetTimer();
    }

    internal static void SaveScoreTotal(int total)
    {
        Instance.playerStats.scoreTotal = total;
        Instance.SetTimer();
    }

    internal void LoadLevelTime(out int level)
    {
        level = Instance.playerStats.levelTime;
    }

    internal void LoadLevelMoves(out int level)
    {
        level = Instance.playerStats.levelMoves;
    }

    internal void LoadScoreBestTime(out int bestTime)
    {
        bestTime = Instance.playerStats.scoreBestTime;
    }

    internal void LoadScoreBestMoves(out int bestMoves)
    {
        bestMoves = Instance.playerStats.scoreBestMoves;
    }

    internal void LoadScoreTotal(out int total)
    {
        total = Instance.playerStats.scoreTotal;
    }

    internal static void SaveScoreBestMoves(int current)
    {
        Instance.playerStats.scoreBestMoves = current;
        Instance.SetTimer();
    }

    internal static void SaveScoreBestTime(int current)
    {
        Instance.playerStats.scoreBestTime = current;
        Instance.SetTimer();
    }

    internal void LoadScoreCoin(out int coin)
    {
        coin = Instance.playerStats.coins;
    }

    internal static DateTime LoadLastAuthBonusDate()
    {
        return Instance.playerStats.lastAuthBonusDate == "" ? DateTime.UnixEpoch : DateTime.Parse(Instance.playerStats.lastAuthBonusDate);
    }

    internal static void SaveBooster(Boosters.BoosterType type, int amount)
    {
        Instance.playerStats.boosters[(int)type] = amount;
        Instance.SetTimer();
    }

    internal static void SaveLastAuthBonusDate(DateTime lastAuthBonus)
    {
        Instance.playerStats.lastAuthBonusDate = lastAuthBonus.ToString();
        Instance.SetTimer();
    }

    internal void LoadBooster(Boosters.BoosterType type, out int amount)
    {
        amount = Instance.playerStats.boosters[(int)type];
    }

    
    static string Name(string name)
    {
        string str = builder.Append(Ver).Append(name).ToString();
        builder.Clear();
        return str;
    }

    static public void LoadString(string name, out string val, string def)
    {
        string str = Name(name);
        if (PlayerPrefs.HasKey(str))
        {
            val = PlayerPrefs.GetString(str);
        }
        else
        {
            val = def;
        }
    }
    

    static public void LoadBool(string name, out bool val, bool def)
    {
        string str = Name(name);
        if (PlayerPrefs.HasKey(str))
        {
            val = Convert.ToBoolean(PlayerPrefs.GetInt(str));
        }
        else
        {
            val = def;
        }
    }

    static public void LoadFloat(string name, out float val, float def)
    {
        string str = Name(name);
        if (PlayerPrefs.HasKey(str))
        {
            val = PlayerPrefs.GetFloat(str);
        }
        else
        {
            val = def;
        }
    }

    static public void SaveString(string name, string val)
    {
        PlayerPrefs.SetString(Name(name), val);
    }
    static public void SaveInt(string name, int val)
    {
        PlayerPrefs.SetInt(Name(name), val);
    }

    static public void SaveBool(string name, bool val)
    {
        PlayerPrefs.SetInt(Name(name), Convert.ToInt32(val));
    }

    static public void SaveFloat(string name, float val)
    {
        PlayerPrefs.SetFloat(Name(name), val);
    }
}
