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
}
