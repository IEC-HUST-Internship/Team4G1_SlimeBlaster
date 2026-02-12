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

    /// <summary>
    /// Log when user wins or loses a stage
    /// </summary>
    /// <param name="stage">Current stage (player progress)</param>
    /// <param name="time">Time from stage start (seconds)</param>
    /// <param name="sessionCount">Number of attempts on this stage</param>
    /// <param name="result">True = win, False = lose</param>
    public void GameOver(int stage, int time, int sessionCount, bool result)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogGameOver - Stage: {stage}, Time: {time}, SessionCount: {sessionCount}, Result: {result}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "game_over",
                new Firebase.Analytics.Parameter("stage", stage),
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
    /// Log when user starts playing a stage
    /// </summary>
    /// <param name="stage">Current stage (player progress)</param>
    /// <param name="count">Number of times played this stage</param>
    public void PlayStage(int stage, int count)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogPlayStage - Stage: {stage}, Count: {count}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "play_stage",
                new Firebase.Analytics.Parameter("stage", stage),
                new Firebase.Analytics.Parameter("count", count)
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] LogPlayStage failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Log when user replays a stage
    /// </summary>
    /// <param name="stage">Current stage (player progress)</param>
    /// <param name="count">Number of replays</param>
    public void ReplayStage(int stage, int count)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogReplayStage - Stage: {stage}, Count: {count}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "replay_stage",
                new Firebase.Analytics.Parameter("stage", stage),
                new Firebase.Analytics.Parameter("count", count)
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] LogReplayStage failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Log when RewardAds shown successfully
    /// </summary>
    /// <param name="stage">Stage when watching reward ads (player progress)</param>
    /// <param name="position">Position where reward ads shown (e.g., "add_HelperShovel", "stage_win")</param>
    public void SraPosition(int stage, string position)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogSraPosition - Stage: {stage}, Position: {position}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "sra_position",
                new Firebase.Analytics.Parameter("stage", stage),
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
    /// <param name="stage">Stage when watching interstitial ads (player progress)</param>
    /// <param name="position">Position where interstitial ads shown (e.g., "stage_win")</param>
    public void SfaPosition(int stage, string position)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogSfaPosition - Stage: {stage}, Position: {position}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "sfa_position",
                new Firebase.Analytics.Parameter("stage", stage),
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
    /// <param name="stage">Current user stage (player progress)</param>
    /// <param name="bundleId">Bundle ID of purchased package (e.g., "com.slither.in.gold.pack.01")</param>
    public void BuySuccess(int stage, string bundleId)
    {
        if (!isFirebaseReady) return;
        Debug.Log($"[Firebase] LogBuySuccess - Stage: {stage}, BundleId: {bundleId}");
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "buy_success",
                new Firebase.Analytics.Parameter("stage", stage),
                new Firebase.Analytics.Parameter("bundleid", bundleId)
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] LogBuySuccess failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Log ad_impression event when MAX pays revenue for any ad type
    /// Called from OnAdRevenuePaidEvent callback of Rewarded, Interstitial, Banner
    /// </summary>
    public void AdImpression(MaxSdkBase.AdInfo adInfo)
    {
        if (!isFirebaseReady) return;
        
        double revenue = adInfo.Revenue;
        Debug.Log($"[Firebase] AdImpression - AdFormat: {adInfo.AdFormat}, Network: {adInfo.NetworkName}, Revenue: {revenue}");
        
        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                "ad_impression",
                new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
                new Firebase.Analytics.Parameter("ad_source", adInfo.NetworkName),
                new Firebase.Analytics.Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter("value", revenue),
                new Firebase.Analytics.Parameter("currency", "USD")
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] AdImpression failed: {ex.Message}");
        }
    }
}
