using UnityEngine;
using Firebase;
using Firebase.Extensions;

/// 🔥 Initialize Firebase
/// Add this to a GameObject in your first scene
public class FireBaseInit : MonoBehaviour
{
    public static bool IsInitialized { get; private set; } = false;
    private Firebase.FirebaseApp app;

    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                // Create and hold a reference to your FirebaseApp
                app = Firebase.FirebaseApp.DefaultInstance;
                IsInitialized = true;
                
                MizuLog.EventAndAds("✅ Firebase initialized successfully!");
            } else {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
}
