using UnityEngine.Events;

    public static class Events
    {
        static public UnityEvent<bool> LevelComplete = new UnityEvent<bool>();
        static public UnityEvent PlayerInitialized = new UnityEvent();

    }
