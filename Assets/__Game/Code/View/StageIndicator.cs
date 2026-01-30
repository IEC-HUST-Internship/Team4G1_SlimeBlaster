using UnityEngine;
using DG.Tweening;

public class StageIndicator : MonoBehaviour
{
    [Header("Scale Animation")]
    public float minScale = 0.9f;
    public float maxScale = 1.1f;
    public float duration = 0.8f;
    
    private Tween scaleTween;
    
    private void OnEnable()
    {
        StartScaleAnimation();
    }
    
    private void StartScaleAnimation()
    {
        // Kill any existing tween
        scaleTween?.Kill();
        
        // Start at min scale
        transform.localScale = Vector3.one * minScale;
        
        // Animate to max scale with elastic ease, loop forever (yoyo)
        scaleTween = transform.DOScale(maxScale, duration)
            .SetEase(Ease.InOutElastic)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    private void OnDisable()
    {
        scaleTween?.Kill();
    }
    
    private void OnDestroy()
    {
        scaleTween?.Kill();
    }
}
