using UnityEngine;

/// <summary>
/// 🎯 Sets the target frame rate for the game
/// </summary>
public class FPSSetup : MonoBehaviour
{
    [SerializeField] private int targetFPS = 60;

    private void Awake()
    {
        Application.targetFrameRate = targetFPS;
    }
}
