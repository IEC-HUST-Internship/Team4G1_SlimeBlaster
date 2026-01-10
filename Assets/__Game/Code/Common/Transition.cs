using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class TransitionTextEntry
{
    public string text;
    [Range(0f, 1f)]
    public float probability = 1f;
}

public class Transition : MonoBehaviour
{
    [Header("UI")]
    public Image transitionImage;
    public TextMeshProUGUI transitionText;
    
    [Header("Text Options")]
    public List<TransitionTextEntry> transitionTexts = new List<TransitionTextEntry>();
    
    [Header("Sprites")]
    public List<Sprite> transitionInSprites;   // Animation going IN (covering screen)
    public List<Sprite> transitionOutSprites;  // Animation going OUT (revealing screen)
    
    [Header("Settings")]
    public float frameDuration = 0.05f;
    public float transitionDelay = 1f;  // Wait time between fade in and fade out
    
    [Header("Text Timing")]
    public int showTextBeforeFadeInEnds = 10;   // Show text X frames before fade in completes
    public int hideTextAfterFadeOutStarts = 10; // Hide text X frames after fade out starts
    
    private Coroutine currentTransition;
    private bool isTransitioning = false;

    private void Awake()
    {
        // Start with image hidden
        if (transitionImage != null)
            transitionImage.gameObject.SetActive(false);
        
        // Start with text hidden
        if (transitionText != null)
            transitionText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 🎬 Play full transition: in → execute action → out
    /// </summary>
    public void PlayTransition(Action onMiddle, Action onComplete = null)
    {
        if (isTransitioning) return;
        StartCoroutine(PlayFullTransition(onMiddle, onComplete));
    }

    private IEnumerator PlayFullTransition(Action onMiddle, Action onComplete)
    {
        isTransitioning = true;
        
        // 🔲 Transition IN (cover screen) - text shows X frames before end
        yield return StartCoroutine(PlayTransitionInWithText());
        
        // 🔄 Execute middle action (scene switch)
        onMiddle?.Invoke();
        
        // ⏳ Wait (text is visible during this time)
        yield return new WaitForSecondsRealtime(transitionDelay);
        
        // 🔲 Transition OUT (reveal screen) - text hides X frames after start
        yield return StartCoroutine(PlayTransitionOutWithText());
        
        isTransitioning = false;
        onComplete?.Invoke();
    }

    public void TransitionIn(Action onComplete = null)
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);
        
        currentTransition = StartCoroutine(PlayTransitionWithCallback(false, onComplete));
    }

    public void TransitionOut(Action onComplete = null)
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);
        
        currentTransition = StartCoroutine(PlayTransitionWithCallback(true, onComplete));
    }

    private IEnumerator PlayTransitionWithCallback(bool reverse, Action onComplete)
    {
        yield return StartCoroutine(PlayTransitionCoroutine(reverse));
        onComplete?.Invoke();
    }

    private IEnumerator PlayTransitionCoroutine(bool reverse)
    {
        if (transitionImage == null)
            yield break;

        // Choose which sprite list to use
        List<Sprite> sprites = reverse ? transitionOutSprites : transitionInSprites;
        
        if (sprites == null || sprites.Count == 0)
            yield break;

        transitionImage.gameObject.SetActive(true);

        // Play through the sprite list
        for (int i = 0; i < sprites.Count; i++)
        {
            transitionImage.sprite = sprites[i];
            yield return new WaitForSecondsRealtime(frameDuration);
        }

        // Hide after out transition
        if (reverse)
        {
            transitionImage.gameObject.SetActive(false);
        }

        currentTransition = null;
    }

    /// <summary>
    /// 🔲 Fade in with text appearing X frames before end
    /// </summary>
    private IEnumerator PlayTransitionInWithText()
    {
        if (transitionImage == null)
            yield break;

        List<Sprite> sprites = transitionInSprites;
        
        if (sprites == null || sprites.Count == 0)
            yield break;

        transitionImage.gameObject.SetActive(true);
        
        int showTextAtFrame = Mathf.Max(0, sprites.Count - showTextBeforeFadeInEnds);

        for (int i = 0; i < sprites.Count; i++)
        {
            transitionImage.sprite = sprites[i];
            
            // Show text X frames before fade in ends
            if (i == showTextAtFrame && transitionText != null)
            {
                transitionText.text = GetRandomText();
                transitionText.gameObject.SetActive(true);
            }
            
            yield return new WaitForSecondsRealtime(frameDuration);
        }
    }

    /// <summary>
    /// 🔲 Fade out with text hiding X frames after start
    /// </summary>
    private IEnumerator PlayTransitionOutWithText()
    {
        if (transitionImage == null)
            yield break;

        List<Sprite> sprites = transitionOutSprites;
        
        if (sprites == null || sprites.Count == 0)
            yield break;

        transitionImage.gameObject.SetActive(true);
        
        int hideTextAtFrame = Mathf.Min(hideTextAfterFadeOutStarts, sprites.Count - 1);

        for (int i = 0; i < sprites.Count; i++)
        {
            transitionImage.sprite = sprites[i];
            
            // Hide text X frames after fade out starts
            if (i == hideTextAtFrame && transitionText != null)
            {
                transitionText.gameObject.SetActive(false);
            }
            
            yield return new WaitForSecondsRealtime(frameDuration);
        }

        transitionImage.gameObject.SetActive(false);
        currentTransition = null;
    }

    /// <summary>
    /// ❓ Check if currently transitioning
    /// </summary>
    public bool IsTransitioning() => isTransitioning;

    /// <summary>
    /// 🎲 Get random text based on probability
    /// </summary>
    private string GetRandomText()
    {
        if (transitionTexts == null || transitionTexts.Count == 0)
            return "";
        
        // Roll for each text entry based on probability
        foreach (var entry in transitionTexts)
        {
            if (UnityEngine.Random.value <= entry.probability)
                return entry.text;
        }
        
        // Fallback to first entry if nothing hit
        return transitionTexts[0].text;
    }
}