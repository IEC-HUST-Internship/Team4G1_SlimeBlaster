using UnityEngine;

public class BannerAds : MonoBehaviour
{
    private AppLovinConfig config;
    private bool isInitialized = false;

    void Start()
    {
        config = Resources.Load<AppLovinConfig>("AppLovinConfig");
        
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            // AppLovin SDK is initialized, start loading ads
            MaxSdk.CreateBanner(config.BannerId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerBackgroundColor(config.BannerId, Color.black);
            isInitialized = true;
        };

        // Banner revenue callback
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerRevenuePaidEvent;

        MaxSdk.SetSdkKey(config.sdkKey);
        MaxSdk.SetUserId("USER_ID");
        MaxSdk.InitializeSdk();
    }

    public void ShowBanner()
    {
        if (isInitialized)
        {
            MaxSdk.ShowBanner(config.BannerId);
        }
    }

    public void HideBanner()
    {
        if (isInitialized)
        {
            MaxSdk.HideBanner(config.BannerId);
        }
    }

    private void OnBannerRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Ad revenue paid — send ad_impression to Firebase
        if (FireBaseAnalytics.Instance != null)
        {
            FireBaseAnalytics.Instance.AdImpression(adInfo);
        }
    }
}
