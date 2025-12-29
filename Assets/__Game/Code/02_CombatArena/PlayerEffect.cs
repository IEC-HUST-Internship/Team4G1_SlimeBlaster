using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PlayerEffect : MonoBehaviour
{
    [Header("Player Renderers")]
    public SpriteRenderer playerBorder;
    public SpriteRenderer playerBackground;

    [Header("Glow Settings")]
    public float glowAmount = 4f;
    public float glowDuration = 0.2f;

    [Header("Attack Color")]
    public Color attackColor = Color.cyan;
    public Color normalColor = Color.white;

    [Header("Rubber Scale")]
    public float stretchScale = 1.15f;
    public float squashScale = 0.9f;
    public float stretchDuration = 0.05f;
    public float squashDuration = 0.08f;
    public float returnDuration = 0.25f;
    public Ease rubberEase = Ease.OutElastic;

    [Header("Punch Jiggle (Rotation)")]
    public float punchRotation = 3f;
    public float punchDuration = 0.2f;
    public int punchVibrato = 10;
    public float punchElasticity = 0.9f;

    private MaterialPropertyBlock borderBlock;
    private MaterialPropertyBlock backgroundBlock;

    private static readonly int GlowAmountID = Shader.PropertyToID("_GlowAmount");
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private Coroutine glowCoroutine;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Sequence scaleSequence;

    void Awake()
    {
        if (playerBorder != null)
            borderBlock = new MaterialPropertyBlock();

        if (playerBackground != null)
            backgroundBlock = new MaterialPropertyBlock();

        originalScale = transform.localScale;
        originalRotation = transform.rotation;

        ResetGlow();
        ResetColor();
    }

    void OnDisable()
    {
        // Kill tweens safely
        scaleSequence?.Kill();
        transform.DOKill(true);

        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }

        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        ResetGlow();
        ResetColor();
    }

    // 🔥 CALL THIS ON ATTACK
    public void PlayerAttackEffect()
    {
        // Instant color change
        SetColor(attackColor);

        // Glow
        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);

        glowCoroutine = StartCoroutine(GlowRoutine());

        // Scale + Jiggle
        PlayAttackEffect();
    }

    // ================= GLOW =================
    private IEnumerator GlowRoutine()
    {
        SetGlow(glowAmount);
        yield return new WaitForSeconds(glowDuration);
        SetGlow(0f);

        // Instant return to normal color
        ResetColor();

        glowCoroutine = null;
    }

    private void SetGlow(float amount)
    {
        if (playerBorder != null && borderBlock != null)
        {
            playerBorder.GetPropertyBlock(borderBlock);
            borderBlock.SetFloat(GlowAmountID, amount);
            playerBorder.SetPropertyBlock(borderBlock);
        }

        if (playerBackground != null && backgroundBlock != null)
        {
            playerBackground.GetPropertyBlock(backgroundBlock);
            backgroundBlock.SetFloat(GlowAmountID, amount);
            playerBackground.SetPropertyBlock(backgroundBlock);
        }
    }

    private void ResetGlow()
    {
        SetGlow(0f);
    }

    // ================= COLOR =================
    private void SetColor(Color color)
    {
        if (playerBorder != null && borderBlock != null)
        {
            playerBorder.GetPropertyBlock(borderBlock);
            borderBlock.SetColor(ColorID, color);
            playerBorder.SetPropertyBlock(borderBlock);
        }

        if (playerBackground != null && backgroundBlock != null)
        {
            playerBackground.GetPropertyBlock(backgroundBlock);
            backgroundBlock.SetColor(ColorID, color);
            playerBackground.SetPropertyBlock(backgroundBlock);
        }
    }

    private void ResetColor()
    {
        SetColor(normalColor);
    }

    // ================= ATTACK EFFECT =================
    private void PlayAttackEffect()
    {
        scaleSequence?.Kill();
        transform.DOKill(true);

        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        float randomRot = Random.Range(-punchRotation, punchRotation);

        // Rubber scale
        scaleSequence = DOTween.Sequence();
        scaleSequence.Append(transform.DOScale(originalScale * stretchScale, stretchDuration));
        scaleSequence.Append(transform.DOScale(originalScale * squashScale, squashDuration));
        scaleSequence.Append(
            transform.DOScale(originalScale, returnDuration)
                     .SetEase(rubberEase)
        );

        // Punch jiggle rotation
        transform.DOPunchRotation(
            new Vector3(0f, 0f, randomRot),
            punchDuration,
            punchVibrato,
            punchElasticity
        );
    }
}
