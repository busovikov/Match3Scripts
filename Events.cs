using UnityEngine.Events;

public static class Events
{
    static public UnityEvent<bool,int> LevelComplete = new UnityEvent<bool,int>();
    static public UnityEvent<bool> PlayerInitialized = new UnityEvent<bool>();
    static public UnityEvent<bool> PlayerAuthorized = new UnityEvent<bool>();
    static public UnityEvent PlayerStatsUpdated = new UnityEvent();
}
