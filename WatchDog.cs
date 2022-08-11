public static class WatchDog
{
    private static int processing = 0;
    private static bool dirty = false;

    public static void Inc() 
    {
        WatchDog.processing++;
    }

    public static void Dec()
    {
        if(WatchDog.processing > 0)
            WatchDog.processing--;
        if (WatchDog.processing == 0)
            dirty = true;
    }
    public static bool CheckIfDirtyAndReset()
    {
        bool tmp = dirty;
        dirty = false;
        return tmp;
    }
}