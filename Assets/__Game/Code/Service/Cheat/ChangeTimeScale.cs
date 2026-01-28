using UnityEngine;

/// <summary>
/// ⏱️ Cheat: Change Time Scale
/// Attach to any GameObject to control game speed from Inspector
/// </summary>
public class ChangeTimeScale : MonoBehaviour
{
    [Header("⏱️ Time Scale Control")]
    [Range(0f, 10f)]
    [Tooltip("Drag to change game speed (1 = normal, 0 = paused, 2 = 2x speed)")]
    public float timeScale = 1f;
    
    [Header("🎮 Quick Presets")]
    [SerializeField] private bool pause = false;
    [SerializeField] private bool normalSpeed = false;
    [SerializeField] private bool doubleSpeed = false;
    [SerializeField] private bool tripleSpeed = false;
    [SerializeField] private bool maxSpeed = false;
    
    private float lastTimeScale = 1f;

    private void OnEnable()
    {
        Time.timeScale = timeScale;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f; // Reset to normal when disabled
    }

    private void OnValidate()
    {
        // Handle preset buttons
        if (pause)
        {
            pause = false;
            timeScale = 0f;
        }
        if (normalSpeed)
        {
            normalSpeed = false;
            timeScale = 1f;
        }
        if (doubleSpeed)
        {
            doubleSpeed = false;
            timeScale = 2f;
        }
        if (tripleSpeed)
        {
            tripleSpeed = false;
            timeScale = 3f;
        }
        if (maxSpeed)
        {
            maxSpeed = false;
            timeScale = 10f;
        }
        
        // Apply time scale if changed
        if (!Mathf.Approximately(timeScale, lastTimeScale))
        {
            Time.timeScale = timeScale;
            lastTimeScale = timeScale;
            MizuLog.General($"⏱️ Time Scale: {timeScale:F1}x");
        }
    }

    private void Update()
    {
        // Keep time scale in sync if changed externally
        if (!Mathf.Approximately(Time.timeScale, timeScale))
        {
            Time.timeScale = timeScale;
        }
    }
}
