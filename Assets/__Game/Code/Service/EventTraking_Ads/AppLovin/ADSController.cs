using UnityEngine;

public class ADSController : Singleton<ADSController>
{
    [SerializeField] private BannerAds bannerAds;
    [SerializeField] private InterstitialAds interstitialAds;
    [SerializeField] private RewardedAds rewardedAds;

    // Banner
    public void ShowBanner() => bannerAds?.ShowBanner();
    public void HideBanner() => bannerAds?.HideBanner();

    // Interstitial
    public void ShowInterstitial() => interstitialAds?.LoadInterstitial();

    // Rewarded
    public void ShowRewarded() => rewardedAds?.LoadRewardedAd();
}
