using UnityEngine;
using AdjustSdk;
using System.Collections.Generic;

/// 📊 Initialize Adjust SDK
/// Add this to a GameObject in your first scene
public class AdjustInit : MonoBehaviour
{
    public static bool IsInitialized { get; private set; } = false;

    [Header("Adjust Settings")]
    [Tooltip("Get your App Token from Adjust Dashboard")]
    public string appToken = "YOUR_APP_TOKEN";
    
    [Tooltip("Use Sandbox for testing, Production for release")]
    public AdjustEnvironment environment = AdjustEnvironment.Sandbox;

    [Header("Event Tokens")]
    [Tooltip("Map event names to Adjust event tokens")]
    public List<EventTokenPair> eventTokenList = new List<EventTokenPair>();

    // Dictionary for quick lookup
    private static Dictionary<string, string> _eventDict;
    public static Dictionary<string, string> EventDict => _eventDict;

    private static AdjustInit _instance;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Build dictionary from list
        _eventDict = new Dictionary<string, string>();
        foreach (var pair in eventTokenList)
        {
            if (!string.IsNullOrEmpty(pair.eventName) && !string.IsNullOrEmpty(pair.eventToken))
            {
                _eventDict[pair.eventName] = pair.eventToken;
            }
        }
    }

    void Start()
    {
        InitializeAdjust();
    }

    private void InitializeAdjust()
    {
#if UNITY_EDITOR
        MizuLog.EventAndAds("⚠️ Adjust SDK does not work in Editor");
        IsInitialized = true; // Mark as initialized for testing flow
        return;
#else
        if (string.IsNullOrEmpty(appToken) || appToken == "YOUR_APP_TOKEN")
        {
            MizuLog.EventAndAds("❌ Adjust App Token not set!");
            return;
        }

        var config = new AdjustConfig(appToken, environment);
        config.LogLevel = AdjustLogLevel.Verbose; // For debugging
        
        Adjust.InitSdk(config);
        IsInitialized = true;
        
        MizuLog.EventAndAds($"✅ Adjust initialized! Environment: {environment}");
#endif
    }

    /// <summary>
    /// Push event to Adjust
    /// </summary>
    /// <param name="eventName">Event name (must exist in eventTokenList)</param>
    public static void PushEvent(string eventName)
    {
        if (_eventDict == null || !_eventDict.ContainsKey(eventName))
        {
            MizuLog.EventAndAds($"⚠️ Adjust event '{eventName}' not found in dictionary!");
            return;
        }

#if UNITY_EDITOR
        MizuLog.EventAndAds($"📊 [Editor] Adjust Event: {eventName} -> {_eventDict[eventName]}");
        return;
#else
        var adjustEvent = new AdjustEvent(_eventDict[eventName]);
        Adjust.TrackEvent(adjustEvent);
        MizuLog.EventAndAds($"📊 Adjust Event Sent: {eventName} -> {_eventDict[eventName]}");
#endif
    }

    /// <summary>
    /// Push event with revenue
    /// </summary>
    public static void PushEventWithRevenue(string eventName, double revenue, string currency = "USD")
    {
        if (_eventDict == null || !_eventDict.ContainsKey(eventName))
        {
            MizuLog.EventAndAds($"⚠️ Adjust event '{eventName}' not found in dictionary!");
            return;
        }

#if UNITY_EDITOR
        MizuLog.EventAndAds($"📊 [Editor] Adjust Revenue Event: {eventName} -> {revenue} {currency}");
        return;
#else
        var adjustEvent = new AdjustEvent(_eventDict[eventName]);
        adjustEvent.SetRevenue(revenue, currency);
        Adjust.TrackEvent(adjustEvent);
        MizuLog.EventAndAds($"📊 Adjust Revenue Event: {eventName} -> {revenue} {currency}");
#endif
    }
}

[System.Serializable]
public class EventTokenPair
{
    public string eventName;
    public string eventToken;
}
