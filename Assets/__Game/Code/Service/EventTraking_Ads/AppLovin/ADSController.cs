using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ADSController : Singleton<ADSController>
{
    [SerializeField] private BannerAds bannerAds;
    [SerializeField] private InterstitialAds interstitialAds;
    [SerializeField] private RewardedAds rewardedAds;

    [Header("Play Buttons")]
    [SerializeField] private List<Button> playButtons;

    /// <summary>
    /// True if player has purchased No Ads
    /// </summary>
    public bool IsNoAds { get; private set; } = false;

    protected override void Awake()
    {
        base.Awake();
        // Load no ads state EARLY so it's ready before any OnEnable() calls
        LoadNoAdsState();
    }

    private void Start()
    {
        // Add listener to all play buttons
        foreach (var button in playButtons)
        {
            if (button != null)
            {
                button.onClick.AddListener(ShowInterstitial);
            }
        }
    }

    // 🚫 No Ads
    public void PurchaseNoAds()
    {
        Debug.Log("[ADSController] PurchaseNoAds CALLED!");
        IsNoAds = true;
        SaveNoAdsState();
        HideBanner();
        Debug.Log($"[ADSController] No Ads purchased! IsNoAds = {IsNoAds}");
    }

    private void LoadNoAdsState()
    {
        if (SaveSystem.Instance != null)
        {
            var saveData = SaveSystem.Instance.LoadGame();
            IsNoAds = saveData.noAdsPurchased;
            Debug.Log($"[ADSController] LoadNoAdsState: noAdsPurchased = {saveData.noAdsPurchased}, IsNoAds = {IsNoAds}");
        }
        else
        {
            IsNoAds = false;
            Debug.LogWarning("[ADSController] SaveSystem.Instance is NULL! IsNoAds set to false.");
        }
    }

    private void SaveNoAdsState()
    {
        if (SaveSystem.Instance != null)
        {
            var saveData = SaveSystem.Instance.LoadGame();
            saveData.noAdsPurchased = true;
            SaveSystem.Instance.SaveGame(saveData);
        }
    }

    // Banner
    public void ShowBanner()
    {
        if (IsNoAds) return;
        bannerAds?.ShowBanner();
    }

    public void HideBanner() => bannerAds?.HideBanner();

    // Interstitial
    public void ShowInterstitial()
    {
        if (IsNoAds) return;
        interstitialAds?.TryShowInterstitial();
    }

    // Rewarded (still show even with No Ads — player chooses to watch)
    public void ShowRewarded() => rewardedAds?.LoadRewardedAd();
}
