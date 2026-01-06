using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerEffect : MonoBehaviour
{
    [Header("Player Renderers")]
    public List<GameObject> playerParts = new List<GameObject>();

    [Header("Attack Size Sprites (15 sizes: 100%, 110%, 120%... 240%)")]
    public List<Sprite> borderSprites = new List<Sprite>();
    public List<Sprite> backgroundSprites = new List<Sprite>();
    
    [Header("Sprite Renderers for Size Change")]
    public SpriteRenderer borderRenderer;
    public SpriteRenderer backgroundRenderer;

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

    private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    private List<MaterialPropertyBlock> propertyBlocks = new List<MaterialPropertyBlock>();

    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Sequence scaleSequence;

    void OnEnable()
    {
        // 🎨 Collect all SpriteRenderers from playerParts list
        foreach (var part in playerParts)
        {
            if (part != null)
            {
                var sr = part.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    spriteRenderers.Add(sr);
                    propertyBlocks.Add(new MaterialPropertyBlock());
                }
            }
        }

        originalScale = transform.localScale;
        originalRotation = transform.rotation;

        ResetColor();
    }

    void OnDisable()
    {
        // Kill tweens safely
        scaleSequence?.Kill();
        transform.DOKill(true);

        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        ResetColor();
    }

    // 🔥 CALL THIS ON ATTACK
    public void PlayerAttackEffect()
    {
        // Instant color change
        SetColor(attackColor);

        // Scale + Jiggle
        PlayAttackEffect();
    }

    // ================= ATTACK SIZE =================
    /// <summary>
    /// 📐 Set attack size by count (0-14)
    /// Count 0 = size 15, each count adds 10% area
    /// Swaps sprites instead of scaling
    /// </summary>
    public void SetAttackSizeLevel(int count)
    {
        // 🔒 Clamp count to valid range (0-14)
        int index = Mathf.Clamp(count, 0, 14);
        
        Debug.Log($"📐 Setting attack size to count {index}");
        
        // 🖼️ Swap border sprite
        if (borderRenderer != null && borderSprites != null && borderSprites.Count > index)
        {
            borderRenderer.sprite = borderSprites[index];
            Debug.Log($"🖼️ Border sprite set to index {index}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Cannot set border sprite! renderer={borderRenderer != null}, sprites={borderSprites?.Count ?? 0}, index={index}");
        }
        
        // 🖼️ Swap background sprite
        if (backgroundRenderer != null && backgroundSprites != null && backgroundSprites.Count > index)
        {
            backgroundRenderer.sprite = backgroundSprites[index];
            Debug.Log($"🖼️ Background sprite set to index {index}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Cannot set background sprite! renderer={backgroundRenderer != null}, sprites={backgroundSprites?.Count ?? 0}, index={index}");
        }
    }

    // ================= COLOR =================
    private void SetColor(Color color)
    {
        // 🎨 Apply color to all SpriteRenderers in the list (instant, no fade)
        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            var sr = spriteRenderers[i];
            var block = propertyBlocks[i];
            
            if (sr != null && block != null)
            {
                sr.GetPropertyBlock(block);
                block.SetColor(ColorID, color);
                sr.SetPropertyBlock(block);
            }
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
