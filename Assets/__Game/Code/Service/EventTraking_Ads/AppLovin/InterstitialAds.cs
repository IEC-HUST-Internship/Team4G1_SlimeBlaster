using System;
using UnityEngine;

public class InterstitialAds : MonoBehaviour
{
    private const int REQUIRED_PLAYS_BEFORE_ADS = 5;
    private const float COOLDOWN_SECONDS = 1f; // 2 minutes

    private AppLovinConfig config;
    private int retryAttempt;
    private float lastAdShownTime = float.MinValue;
    private bool isAdReady = false;

    void Start()
    {
        config = Resources.Load<AppLovinConfig>("AppLovinConfig");
        
        // Attach callback
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

        // Pre-load ad
        LoadInterstitialAd();
    }

    private void LoadInterstitialAd()
    {
        MaxSdk.LoadInterstitial(config.InterstitialId);
    }

    /// <summary>
    /// Call this when player presses Play or Replay
    /// </summary>
    public void TryShowInterstitial()
    {
        // Increment play count and save to JSON
        var saveData = SaveSystem.Instance.LoadGame();
        saveData.interstitialPlayCount++;
        int playCount = saveData.interstitialPlayCount;
        SaveSystem.Instance.SaveGame(saveData);

        // Check if we've played enough times
        if (playCount <= REQUIRED_PLAYS_BEFORE_ADS)
        {
            Debug.Log($"[InterstitialAds] Play count: {playCount}/{REQUIRED_PLAYS_BEFORE_ADS} - Not showing ad yet");
            return;
        }

        // Check cooldown
        float timeSinceLastAd = Time.realtimeSinceStartup - lastAdShownTime;
        if (timeSinceLastAd < COOLDOWN_SECONDS)
        {
            Debug.Log($"[InterstitialAds] Cooldown active: {COOLDOWN_SECONDS - timeSinceLastAd:F0}s remaining");
            return;
        }

        // Show ad if ready
        if (isAdReady && MaxSdk.IsInterstitialReady(config.InterstitialId))
        {
            MaxSdk.ShowInterstitial(config.InterstitialId);
            lastAdShownTime = Time.realtimeSinceStartup;
        }
        else
        {
            // Load for next time
            LoadInterstitialAd();
        }
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isAdReady = true;
        retryAttempt = 0;
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        isAdReady = false;
        retryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));
        Invoke("LoadInterstitialAd", (float)retryDelay);
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) 
    {
        isAdReady = false;
        // Pause game
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        isAdReady = false;
        LoadInterstitialAd();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Resume game
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        // Pre-load next ad
        LoadInterstitialAd();
    }
}
