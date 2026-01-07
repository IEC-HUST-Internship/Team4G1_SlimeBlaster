using UnityEngine;

/// <summary>
/// 📷 Camera Shake Manager - Shakes camera and optionally UI canvas
/// Inherits from Singleton for easy global access
/// Usage: CameraShakeManager.Instance.Shake(0.3f, 0.5f);
/// </summary>
public class CameraShakeManager : Singleton<CameraShakeManager>
{
    [Header("Shake Targets")]
    [SerializeField] private Transform cameraTransform;     // Main camera transform
    [SerializeField] private RectTransform canvasTransform; // Optional: Canvas to shake UI too
    
    [Header("Default Settings")]
    [SerializeField] private float defaultDuration = 0.2f;
    [SerializeField] private float defaultIntensity = 0.3f;
    [SerializeField] private bool shakeUI = true;           // Whether to shake canvas/UI
    
    private Vector3 originalCameraPos;
    private Vector3 originalCanvasPos;
    
    private float currentDuration = 0f;
    private float currentIntensity = 0f;
    private float maxDuration = 0f;
    
    private bool isShaking = false;

    protected override void Awake()
    {
        base.Awake();
        
        // Auto-find camera if not assigned
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Store original positions
        if (cameraTransform != null)
            originalCameraPos = cameraTransform.localPosition;
        if (canvasTransform != null)
            originalCanvasPos = canvasTransform.localPosition;
    }

    private void Update()
    {
        if (!isShaking) return;
        
        if (currentDuration > 0)
        {
            // Calculate shake with falloff (stronger at start, weaker at end)
            float progress = currentDuration / maxDuration;
            float currentShake = currentIntensity * progress;
            
            // Generate random offset
            Vector2 shakeOffset = Random.insideUnitCircle * currentShake;
            
            // Apply to camera
            if (cameraTransform != null)
            {
                cameraTransform.localPosition = originalCameraPos + (Vector3)shakeOffset;
            }
            
            // Apply to canvas (for UI shake)
            if (shakeUI && canvasTransform != null)
            {
                canvasTransform.localPosition = originalCanvasPos + (Vector3)shakeOffset;
            }
            
            currentDuration -= Time.deltaTime;
        }
        else
        {
            // Reset positions when shake ends
            StopShake();
        }
    }

    /// <summary>
    /// Start a camera shake with default values
    /// </summary>
    public void Shake()
    {
        Shake(defaultDuration, defaultIntensity);
    }

    /// <summary>
    /// Start a camera shake with custom duration and intensity
    /// </summary>
    /// <param name="duration">How long the shake lasts (seconds)</param>
    /// <param name="intensity">How strong the shake is (units)</param>
    public void Shake(float duration, float intensity)
    {
        // Allow stacking - use the stronger/longer shake
        if (isShaking)
        {
            currentDuration = Mathf.Max(currentDuration, duration);
            currentIntensity = Mathf.Max(currentIntensity, intensity);
            maxDuration = Mathf.Max(maxDuration, duration);
        }
        else
        {
            currentDuration = duration;
            currentIntensity = intensity;
            maxDuration = duration;
            
            // Store current positions as original
            if (cameraTransform != null)
                originalCameraPos = cameraTransform.localPosition;
            if (canvasTransform != null)
                originalCanvasPos = canvasTransform.localPosition;
        }
        
        isShaking = true;
    }

    /// <summary>
    /// Immediately stop any active shake
    /// </summary>
    public void StopShake()
    {
        isShaking = false;
        currentDuration = 0f;
        
        // Reset to original positions
        if (cameraTransform != null)
            cameraTransform.localPosition = originalCameraPos;
        if (canvasTransform != null)
            canvasTransform.localPosition = originalCanvasPos;
    }

    /// <summary>
    /// Quick shake presets for common use cases
    /// </summary>
    public void ShakeLight() => Shake(0.1f, 0.15f);
    public void ShakeMedium() => Shake(0.2f, 0.3f);
    public void ShakeHeavy() => Shake(0.35f, 0.6f);
    public void ShakeExplosion() => Shake(0.5f, 1f);
}
