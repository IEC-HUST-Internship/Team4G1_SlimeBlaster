using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Transition : MonoBehaviour
{
    [Header("UI")]
    public Image transitionImage;
    
    [Header("Sprites")]
    public List<Sprite> transitionInSprites;   // Animation going IN (covering screen)
    public List<Sprite> transitionOutSprites;  // Animation going OUT (revealing screen)
    
    [Header("Settings")]
    public float frameDuration = 0.05f;
    
    private Coroutine currentTransition;
    private bool isTransitioning = false;

    private void Awake()
    {
        // Start with image hidden
        if (transitionImage != null)
            transitionImage.gameObject.SetActive(false);
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
        
        // 🔲 Transition IN (cover screen)
        yield return StartCoroutine(PlayTransitionCoroutine(false));
        
        // 🔄 Execute middle action (scene switch)
        onMiddle?.Invoke();
        
        // Wait a frame for scene to update
        yield return null;
        
        // 🔲 Transition OUT (reveal screen)
        yield return StartCoroutine(PlayTransitionCoroutine(true));
        
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
    /// ❓ Check if currently transitioning
    /// </summary>
    public bool IsTransitioning() => isTransitioning;
}