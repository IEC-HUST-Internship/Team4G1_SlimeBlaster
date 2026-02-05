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

    // Banner
    public void ShowBanner() => bannerAds?.ShowBanner();
    public void HideBanner() => bannerAds?.HideBanner();

    // Interstitial
    public void ShowInterstitial() => interstitialAds?.TryShowInterstitial();

    // Rewarded
    public void ShowRewarded() => rewardedAds?.LoadRewardedAd();
}
