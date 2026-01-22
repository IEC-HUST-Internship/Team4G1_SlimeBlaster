using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class UIStageControl : MonoBehaviour
{
    [Header("🎮 Stage Buttons")]
    public List<Button> stageButtons;
    public RectTransform buttonContainer; // Parent that holds all buttons, will slide
    
    [Header("⬅️➡️ Arrow Buttons")]
    public Button leftArrowButton;
    public Button rightArrowButton;
    
    [Header("🎬 Slide Animation")]
    public float slideDuration = 0.3f;
    public Ease slideEase = Ease.OutQuad;
    
    [Header("🎬 Scale Animation")]
    public float selectedScale = 1.2f;
    public float unselectedScale = 1f;
    public float scaleDuration = 0.2f;
    public Ease scaleEase = Ease.OutBack;
    
    private int currentIndex = 0;
    private bool isAnimating = false;
    private float containerStartX;
    private float firstButtonLocalX;
    
    private void Start()
    {
        // Wait for layout to build, then store positions
        Canvas.ForceUpdateCanvases();
        
        // Store container's original position
        if (buttonContainer != null)
            containerStartX = buttonContainer.anchoredPosition.x;
        
        // Store first button's local X as the "center" reference
        if (stageButtons.Count > 0 && stageButtons[0] != null)
        {
            RectTransform firstBtn = stageButtons[0].GetComponent<RectTransform>();
            firstButtonLocalX = firstBtn.localPosition.x;
        }
        
        // Setup arrow buttons
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(OnLeftArrowClicked);
        
        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(OnRightArrowClicked);
        
        // Setup stage button clicks
        for (int i = 0; i < stageButtons.Count; i++)
        {
            int index = i; // Capture for closure
            if (stageButtons[i] != null)
                stageButtons[i].onClick.AddListener(() => SelectStage(index));
        }
        
        // Initialize - show first stage selected
        UpdateSelection(false);
        UpdateArrowVisibility();
    }
    
    private void OnLeftArrowClicked()
    {
        if (isAnimating || currentIndex <= 0) return;
        
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        currentIndex--;
        UpdateSelection(true);
        UpdateArrowVisibility();
    }
    
    private void OnRightArrowClicked()
    {
        if (isAnimating || currentIndex >= stageButtons.Count - 1) return;
        
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        currentIndex++;
        UpdateSelection(true);
        UpdateArrowVisibility();
    }
    
    private void SelectStage(int index)
    {
        if (isAnimating || index == currentIndex) return;
        
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        currentIndex = index;
        UpdateSelection(true);
        UpdateArrowVisibility();
    }
    
    private void UpdateSelection(bool animate)
    {
        if (stageButtons == null || stageButtons.Count == 0) return;
        if (currentIndex < 0 || currentIndex >= stageButtons.Count) return;
        if (stageButtons[currentIndex] == null) return;
        
        // Get selected button's local X position (from layout group)
        RectTransform selectedBtn = stageButtons[currentIndex].GetComponent<RectTransform>();
        float selectedButtonLocalX = selectedBtn.localPosition.x;
        
        // Calculate how far to move: difference from first button position
        float offsetFromFirst = selectedButtonLocalX - firstButtonLocalX;
        
        // Move container left by that offset (so selected button ends up where first button was)
        float targetX = containerStartX - offsetFromFirst;
        
        Debug.Log($"Index: {currentIndex}, SelectedLocalX: {selectedButtonLocalX}, FirstLocalX: {firstButtonLocalX}, Offset: {offsetFromFirst}, TargetX: {targetX}");
        
        if (animate)
        {
            isAnimating = true;
            buttonContainer.DOKill();
            buttonContainer.DOAnchorPosX(targetX, slideDuration)
                .SetEase(slideEase)
                .OnComplete(() => isAnimating = false);
        }
        else
        {
            buttonContainer.anchoredPosition = new Vector2(targetX, buttonContainer.anchoredPosition.y);
        }
        
        // Scale buttons
        for (int i = 0; i < stageButtons.Count; i++)
        {
            if (stageButtons[i] == null) continue;
            
            Transform btnTransform = stageButtons[i].transform;
            float targetScale = (i == currentIndex) ? selectedScale : unselectedScale;
            
            if (animate)
            {
                btnTransform.DOKill();
                btnTransform.DOScale(targetScale, scaleDuration).SetEase(scaleEase);
            }
            else
            {
                btnTransform.localScale = Vector3.one * targetScale;
            }
        }
    }
    
    private void UpdateArrowVisibility()
    {
        // Hide left arrow if at first stage
        if (leftArrowButton != null)
            leftArrowButton.gameObject.SetActive(currentIndex > 0);
        
        // Hide right arrow if at last stage
        if (rightArrowButton != null)
            rightArrowButton.gameObject.SetActive(currentIndex < stageButtons.Count - 1);
    }
    
    public int GetCurrentStageIndex()
    {
        return currentIndex;
    }
    
    public void SetStageIndex(int index)
    {
        if (index >= 0 && index < stageButtons.Count)
        {
            currentIndex = index;
            UpdateSelection(false);
            UpdateArrowVisibility();
        }
    }
    
    private void OnDestroy()
    {
        if (leftArrowButton != null)
            leftArrowButton.onClick.RemoveListener(OnLeftArrowClicked);
        
        if (rightArrowButton != null)
            rightArrowButton.onClick.RemoveListener(OnRightArrowClicked);
        
        for (int i = 0; i < stageButtons.Count; i++)
        {
            if (stageButtons[i] != null)
                stageButtons[i].onClick.RemoveAllListeners();
        }
    }
}
