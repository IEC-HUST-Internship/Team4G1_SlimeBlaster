using System;
using UnityEngine;

public class RewardedAds : MonoBehaviour
{
    private AppLovinConfig config;
    private int retryAttempt;
    private Action onRewardEarned;
    private Action onRewardFailed;
    private bool rewardGranted;

    void Start()
    {
        config = Resources.Load<AppLovinConfig>("AppLovinConfig");
        
        // Attach callback
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
    }

    /// <summary>
    /// Show a rewarded ad. When player finishes watching, onReward callback is invoked.
    /// If ad fails or player closes early, onFailed is invoked.
    /// </summary>
    public void ShowRewardedAd(Action onReward, Action onFailed = null)
    {
        onRewardEarned = onReward;
        onRewardFailed = onFailed;
        rewardGranted = false;
        LoadRewardedAd();
    }

    public void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(config.RewardId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
        MaxSdk.ShowRewardedAd(config.RewardId);
        // Reset retry attempt
        retryAttempt = 0;
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        retryAttempt++;
        if (retryAttempt > 3)
        {
            // Give up after 3 retries, notify failure
            onRewardFailed?.Invoke();
            onRewardFailed = null;
            onRewardEarned = null;
            retryAttempt = 0;
            return;
        }

        double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));
        Invoke("LoadRewardedAd", (float)retryDelay);
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. Notify failure and retry load.
        onRewardFailed?.Invoke();
        onRewardFailed = null;
        onRewardEarned = null;
        LoadRewardedAd();
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. If reward was not granted, notify failure.
        if (!rewardGranted)
        {
            onRewardFailed?.Invoke();
            onRewardFailed = null;
            onRewardEarned = null;
        }
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // The rewarded ad displayed and the user should receive the reward.
        rewardGranted = true;
        onRewardEarned?.Invoke();
        onRewardEarned = null;
        onRewardFailed = null;
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Ad revenue paid. Use this callback to track user revenue.
    }
}
