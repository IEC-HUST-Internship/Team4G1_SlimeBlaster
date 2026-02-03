using UnityEngine;
using Firebase.Extensions;
using Firebase.Analytics;

public class FireBaseAnalytics : MonoBehaviour
{
    public static FireBaseAnalytics Instance;
    private bool isFirebaseReady = false;
    private Firebase.FirebaseApp app;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;
                Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Debug;
                isFirebaseReady = true;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void LogLevelComplete(int levelNumber, int score)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] Attempting to log level_complete - Level: {levelNumber}, Score: {score}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "level_complete",
                new Firebase.Analytics.Parameter("level_number", levelNumber),
                new Firebase.Analytics.Parameter("score", score)
            );
            Debug.Log("[Firebase] LogEvent call completed without exception");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] LogEvent failed: {ex.Message}");
        }
    }

    public void LogLevelReset(int levelNumber)
    {
        if (!isFirebaseReady) return;
        Debug.Log("LogLevelReset Firebase");
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "level_reset",
            new Firebase.Analytics.Parameter("level_number", levelNumber)
        );
        Debug.Log("LogLevelReset Firebase End");
    }

    /// <summary>
    /// Log when user wins or loses a level
    /// </summary>
    /// <param name="level">Current level</param>
    /// <param name="time">Time from level start (seconds)</param>
    /// <param name="sessionCount">Number of attempts on this level</param>
    /// <param name="result">True = win, False = lose</param>
    public void GameOver(int level, int time, int sessionCount, bool result)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogGameOver - Level: {level}, Time: {time}, SessionCount: {sessionCount}, Result: {result}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "game_over",
                new Firebase.Analytics.Parameter("level", level),
                new Firebase.Analytics.Parameter("time", time),
                new Firebase.Analytics.Parameter("session_count", sessionCount),
                new Firebase.Analytics.Parameter("result", result ? "win" : "lose")
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] LogGameOver failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Log when user starts playing a level
    /// </summary>
    /// <param name="level">Current level</param>
    /// <param name="count">Number of times played this level</param>
    public void PlayLevel(int level, int count)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogPlayLevel - Level: {level}, Count: {count}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "play_level",
                new Firebase.Analytics.Parameter("level", level),
                new Firebase.Analytics.Parameter("count", count)
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] LogPlayLevel failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Log when user replays a level
    /// </summary>
    /// <param name="level">Current level</param>
    /// <param name="count">Number of replays</param>
    public void ReplayLevel(int level, int count)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogReplayLevel - Level: {level}, Count: {count}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "replay_level",
                new Firebase.Analytics.Parameter("level", level),
                new Firebase.Analytics.Parameter("count", count)
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] LogReplayLevel failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Log when RewardAds shown successfully
    /// </summary>
    /// <param name="level">Level when watching reward ads</param>
    /// <param name="position">Position where reward ads shown (e.g., "add_HelperShovel", "level_win")</param>
    public void SraPosition(int level, string position)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogSraPosition - Level: {level}, Position: {position}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "sra_position",
                new Firebase.Analytics.Parameter("level", level),
                new Firebase.Analytics.Parameter("position", position)
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] LogSraPosition failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Log when InterAds (Interstitial Ads) shown successfully
    /// </summary>
    /// <param name="level">Level when watching interstitial ads</param>
    /// <param name="position">Position where interstitial ads shown (e.g., "level_win")</param>
    public void SfaPosition(int level, string position)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogSfaPosition - Level: {level}, Position: {position}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "sfa_position",
                new Firebase.Analytics.Parameter("level", level),
                new Firebase.Analytics.Parameter("position", position)
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] LogSfaPosition failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Log when IAP purchase is successful
    /// </summary>
    /// <param name="level">Current user level</param>
    /// <param name="bundleId">Bundle ID of purchased package (e.g., "com.slither.in.gold.pack.01")</param>
    public void BuySuccess(int level, string bundleId)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogBuySuccess - Level: {level}, BundleId: {bundleId}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "buy_success",
                new Firebase.Analytics.Parameter("level", level),
                new Firebase.Analytics.Parameter("bundleid", bundleId)
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] LogBuySuccess failed: {ex.Message}");
        }
    }
}
