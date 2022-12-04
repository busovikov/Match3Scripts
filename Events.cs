using UnityEngine.Events;

    public static class Events
    {
        static public UnityEvent<bool,int> LevelComplete = new UnityEvent<bool,int>();
        static public UnityEvent PlayerInitialized = new UnityEvent();

    }
