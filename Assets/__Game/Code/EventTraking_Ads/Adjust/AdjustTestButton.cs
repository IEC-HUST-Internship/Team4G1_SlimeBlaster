using UnityEngine;
using UnityEngine.UI;
using AdjustSdk;

/// 📊 Test Adjust Events
public class AdjustTestButton : MonoBehaviour
{
    public Button button;
    
    [Header("Test Event")]
    [Tooltip("Event name to test (must match name in AdjustInit)")]
    public string testEventName = "test_event";

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(TestAdjustEvent);
        }
    }

    [ContextMenu("📊 Test Adjust Event")]
    public void TestAdjustEvent()
    {
        MizuLog.EventAndAds("📊 AdjustEventStart");
        AdjustInit.PushEvent(testEventName);
        MizuLog.EventAndAds("📊 AdjustEventEnd");
    }

    [ContextMenu("📊 Test Direct Event (with token)")]
    public void TestDirectEvent()
    {
        // Direct test with event token (replace with your actual token)
        string eventToken = "abc123"; // Replace with your event token from Adjust dashboard
        
        MizuLog.EventAndAds($"📊 Direct Adjust Event: {eventToken}");
        
#if !UNITY_EDITOR
        var adjustEvent = new AdjustEvent(eventToken);
        Adjust.TrackEvent(adjustEvent);
#else
        MizuLog.EventAndAds("⚠️ Adjust doesn't work in Editor, build to device to test");
#endif
    }
}
