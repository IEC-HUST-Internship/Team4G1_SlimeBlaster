using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADS : MonoBehaviour
{
    string bannerAdUnitId = "c1586ad841a06c22"; // Retrieve the ID from your account

    // Start is called before the first frame update
    void Start()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            
            // AppLovin SDK is initialized, start loading ads
            // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.black);
            MaxSdk.ShowBanner(bannerAdUnitId);
        };

        MaxSdk.SetSdkKey("iBJBZKgGo-0qqAq7TEpTUwQvhiD-rH6vTDqDeKyQxBOFCH-OBJt8nFs7dP_-A715Z1pu4UC6HSTG-EISryHdg2");
        MaxSdk.SetUserId("USER_ID");
        MaxSdk.InitializeSdk();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
