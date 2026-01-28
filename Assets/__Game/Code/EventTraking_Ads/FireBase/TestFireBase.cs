using UnityEngine;
using Firebase.Analytics;

public static class FirebaseTracking
{
    public static void TrackPlayLevel(int level)
    {
        Debug.Log("Push Firebase Event Level: " + level);
        FirebaseAnalytics.LogEvent("play_level", new Parameter("level", level));
    }
}
