using UnityEngine;

[System.Serializable]
public class PlatformAdIds
{
    public string appId;
    public string interstitialId;
    public string bannerId;
    public string rewardId;
}

[CreateAssetMenu(fileName = "AppLovinConfig", menuName = "Configs/AppLovin Config")]
public class AppLovinConfig : ScriptableObject
{
    [Header("SDK Key")]
    public string sdkKey = "iBJBZKgGo-0qqAq7TEpTUwQvhiD-rH6vTDqDeKyQxBOFCH-OBJt8nFs7dP_-A715Z1pu4UC6HSTG-EISryHdg2";

    public PlatformAdIds android = new PlatformAdIds
    {
        appId = "HcGLca2V",
        interstitialId = "f4daaa8ca",
        bannerId = "1dc62ce8",
        rewardId = "cf7d537as"
    };

    public PlatformAdIds ios = new PlatformAdIds
    {
        appId = "HcGLca2V",
        interstitialId = "bf2ee78be",
        bannerId = "f4d50b0e",
        rewardId = "3065bf1an"
    };

    public PlatformAdIds CurrentPlatform
    {
        get
        {
#if UNITY_ANDROID
            return android;
#elif UNITY_IOS
            return ios;
#else
            return android;
#endif
        }
    }

    public string AppId => CurrentPlatform.appId;
    public string InterstitialId => CurrentPlatform.interstitialId;
    public string BannerId => CurrentPlatform.bannerId;
    public string RewardId => CurrentPlatform.rewardId;
}
