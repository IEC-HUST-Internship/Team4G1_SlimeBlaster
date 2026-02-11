using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIStageControl : MonoBehaviour
{
    [Header("🎮 Stage Setup")]
    public RectTransform stagePrefab; // Prefab with Image + TMP_Text child
    public RectTransform stageContainer; // Parent that holds all stage images, will slide
    
    [Header("📱 Swipe Region")]
    public RectTransform swipeRegion; // Only swipes in this region will work
    
    [Header("⬅️➡️ Arrow Buttons (Optional)")]
    public Button leftArrowButton;
    public Button rightArrowButton;
    
    [Header("🎮 Play Button")]
    public Button playButton;
    public float errorShakeDuration = 0.3f;
    public float errorShakeStrength = 10f;
    
    [Header("🎮 Stage Sprites (3 States)")]
    public Sprite unlockedSelectedSprite;   // Unlocked and currently selected
    public Sprite unlockedUnselectedSprite; // Unlocked but not selected
    public Sprite lockedSprite;             // Locked stage
    
    [Header("🔒 Locked Stage Notification")]
    public GameObject startLockStageNotification; // Shows when trying to play locked stage
    public TMP_Text startText; // The always-visible start text that also flashes
    public float notificationDuration = 3f;
    public float fadeOutDuration = 0.5f;
    public Color flashColor = Color.red;
    public int flashCount = 3;
    public float flashSpeed = 0.15f;
    
    [Header("🎬 Slide Animation")]
    public float slideDuration = 0.3f;
    public Ease slideEase = Ease.OutQuad;
    public float swipeThreshold = 50f; // Minimum swipe distance to change stage
    
    [Header("🎬 Scale Animation")]
    public float selectedScale = 1.2f;
    public float unselectedScale = 1f;
    public float scaleDuration = 0.2f;
    public Ease scaleEase = Ease.OutBack;
    
    [Header("🌊 Swipe Fade")]
    public float swipeFadeAlpha = 0.7f; // Alpha when swiping (30% less)
    
    // Runtime generated stage images
    private List<RectTransform> stageImages = new List<RectTransform>();
    private List<Image> stageImageComponents = new List<Image>(); // Image components for sprite swapping
    private List<GameObject> stageLockObjects = new List<GameObject>(); // Lock child objects for each stage
    private float stageWidth;  // Width of each stage element
    private float stageSpacing;  // Spacing between stages (from layout group)
    
    private int currentIndex = 0;
    private int selectedStage = 1; // The stage number player selected (1-based)
    private bool isAnimating = false;
    private bool isCurrentStageUnlocked = true; // Track if current viewed stage is unlocked
    private int cachedUnlockedStage = 1; // Track unlocked stage count to detect changes
    private float containerStartX;
    
    // Swipe tracking
    private bool isDragging = false;
    private bool isSwiping = false; // Only true when finger actually moved (blocks play button)
    private float dragStartX;
    private float containerDragStartX;
    private const float SWIPE_MOVE_THRESHOLD = 15f; // Pixels moved before considered a swipe
    
    private Camera uiCamera;
    private bool isInitialized = false;
    
    // Notification state
    private Coroutine notificationCoroutine;
    private Color startTextOriginalColor;
    private Color notificationImageOriginalColor;
    private Color notificationTextOriginalColor;
    
    private void Start()
    {
        // Cache UI camera (null is correct for ScreenSpaceOverlay)
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                uiCamera = canvas.worldCamera;
            else
                uiCamera = null; // ScreenSpaceOverlay uses null camera
        }
        
        // Generate stage images based on Stage singleton
        GenerateStageImages();
        
        // Setup arrow buttons (optional)
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(OnLeftArrowClicked);
        
        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(OnRightArrowClicked);
        
        // Start at current stage from Stage singleton
        if (Stage.Instance != null)
        {
            currentIndex = Stage.Instance.GetStage() - 1; // Convert 1-based to 0-based
            currentIndex = Mathf.Clamp(currentIndex, 0, stageImages.Count - 1);
        }
        
        selectedStage = currentIndex + 1;
        
        // Wait for layout to fully build, then initialize positions
        StartCoroutine(InitializeAfterLayout());
    }
    
    /// <summary>
    /// 🔄 Wait for layout to be fully calculated, then store positions and initialize
    /// </summary>
    private IEnumerator InitializeAfterLayout()
    {
        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(stageContainer);
        
        // Wait for end of frame to ensure all layout calculations are done
        yield return new WaitForEndOfFrame();
        
        // Now recalculate and store positions
        RecalculatePositions();
        
        // Initialize lock status
        isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
        
        // Initialize - show current stage selected
        UpdateSelection(false);
        UpdateArrowVisibility();
        UpdateLockedStates();
        UpdateStageSprites();
        
        // Cache start text color and hide notification
        if (startText != null)
            startTextOriginalColor = startText.color;
        
        if (startLockStageNotification != null)
        {
            Image nImg = startLockStageNotification.GetComponent<Image>();
            if (nImg != null) notificationImageOriginalColor = nImg.color;
            
            TMP_Text nTxt = startLockStageNotification.GetComponentInChildren<TMP_Text>();
            if (nTxt != null) notificationTextOriginalColor = nTxt.color;
            
            startLockStageNotification.SetActive(false);
        }
        
        // Cache current unlocked stage
        if (Stage.Instance != null)
            cachedUnlockedStage = Stage.Instance.GetUnlockedStage();
        
        isInitialized = true;
    }
    
    /// <summary>
    /// 🔄 Recalculate container position and stage dimensions
    /// </summary>
    private void RecalculatePositions()
    {
        // Store container's original position
        if (stageContainer != null)
            containerStartX = stageContainer.anchoredPosition.x;
        
        // Get stage width from prefab
        if (stagePrefab != null)
            stageWidth = stagePrefab.sizeDelta.x;
        
        // Get spacing from HorizontalLayoutGroup if exists
        HorizontalLayoutGroup layoutGroup = stageContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            stageSpacing = layoutGroup.spacing;
        }
        else
        {
            stageSpacing = 0f;
        }
    }
    
    /// <summary>
    /// 🎮 Generate stage images based on max stage from Stage singleton
    /// </summary>
    private void GenerateStageImages()
    {
        if (stagePrefab == null || stageContainer == null) return;
        
        // Get max stage from Stage singleton
        int maxStage = 8; // Default fallback
        if (Stage.Instance != null)
        {
            maxStage = Stage.Instance.GetMaxStage();
        }
        
        // Set container height to match prefab height (so it aligns in middle)
        Vector2 prefabSize = stagePrefab.sizeDelta;
        stageContainer.sizeDelta = new Vector2(stageContainer.sizeDelta.x, prefabSize.y);
        
        // Clear any existing children (except prefab if it's in container)
        stageImages.Clear();
        stageImageComponents.Clear();
        stageLockObjects.Clear();
        
        // Generate stage images
        for (int i = 0; i < maxStage; i++)
        {
            int stageNumber = i + 1;
            
            RectTransform stageImage = Instantiate(stagePrefab, stageContainer);
            stageImage.gameObject.name = $"Stage_{stageNumber}";
            stageImage.gameObject.SetActive(true);
            
            // Ensure size matches prefab
            stageImage.sizeDelta = prefabSize;
            
            // Find TMP_Text child and set text to "Stage X"
            TMP_Text stageText = stageImage.GetComponentInChildren<TMP_Text>();
            if (stageText != null)
            {
                stageText.text = $"Stage {stageNumber}";
            }
            
            // Get Image component for sprite swapping
            Image imgComp = stageImage.GetComponent<Image>();
            stageImageComponents.Add(imgComp);
            
            // Find "Lock" child object for showing/hiding
            Transform lockTransform = stageImage.Find("Lock");
            GameObject lockObj = lockTransform != null ? lockTransform.gameObject : null;
            
            stageImages.Add(stageImage);
            stageLockObjects.Add(lockObj);
        }
        
        // Hide prefab if it's in container
        if (stagePrefab.parent == stageContainer)
        {
            stagePrefab.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 🔒 Update locked/unlocked states - show/hide Lock object based on Stage singleton
    /// </summary>
    private void UpdateLockedStates()
    {
        // Always get fresh unlocked stage from Stage singleton
        int unlockedStage = 1;
        if (Stage.Instance != null)
        {
            unlockedStage = Stage.Instance.GetUnlockedStage();
        }
        
        // Update cached value
        cachedUnlockedStage = unlockedStage;
        
        for (int i = 0; i < stageLockObjects.Count; i++)
        {
            int stageNumber = i + 1;
            bool isUnlocked = stageNumber <= unlockedStage;
            
            // Show Lock object if locked, hide if unlocked
            if (stageLockObjects[i] != null)
            {
                stageLockObjects[i].SetActive(!isUnlocked);
            }
        }
    }
    
    /// <summary>
    /// 🔓 Check if a stage index is unlocked
    /// </summary>
    private bool IsStageUnlocked(int index)
    {
        int stageNumber = index + 1;
        int unlockedStage = 1;
        if (Stage.Instance != null)
        {
            unlockedStage = Stage.Instance.GetUnlockedStage();
        }
        return stageNumber <= unlockedStage;
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        HandleSwipe();
        
        // Check for unlock changes in real-time
        CheckForUnlockChanges();
    }
    
    /// <summary>
    /// 🔄 Check if unlocked stage changed and refresh UI
    /// </summary>
    private void CheckForUnlockChanges()
    {
        if (Stage.Instance == null) return;
        
        int currentUnlockedStage = Stage.Instance.GetUnlockedStage();
        if (currentUnlockedStage != cachedUnlockedStage)
        {
            // Stages were unlocked - refresh everything
            cachedUnlockedStage = currentUnlockedStage;
            UpdateLockedStates();
            isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
            UpdateStageSprites();
        }
    }
    
    // ========== SWIPE HANDLING (Manual) ==========
    
    private void HandleSwipe()
    {
        if (isAnimating) return;
        
        // Use touch if available, otherwise use mouse
        bool hasTouch = Input.touchCount > 0;
        
        if (hasTouch)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }
    
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (IsInSwipeRegion(mousePos))
            {
                isDragging = true;
                dragStartX = mousePos.x;
                containerDragStartX = stageContainer.anchoredPosition.x;
            }
        }
        
        if (Input.GetMouseButton(0) && isDragging)
        {
            float deltaX = Input.mousePosition.x - dragStartX;
            stageContainer.anchoredPosition = new Vector2(containerDragStartX + deltaX, stageContainer.anchoredPosition.y);
            
            // Only mark as swiping once finger moves beyond threshold
            if (!isSwiping && Mathf.Abs(deltaX) > SWIPE_MOVE_THRESHOLD)
            {
                isSwiping = true;
                SetStageAlpha(swipeFadeAlpha);
            }
        }
        
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndSwipe(Input.mousePosition.x - dragStartX);
        }
    }
    
    private void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        
        if (touch.phase == TouchPhase.Began)
        {
            if (IsInSwipeRegion(touch.position))
            {
                isDragging = true;
                dragStartX = touch.position.x;
                containerDragStartX = stageContainer.anchoredPosition.x;
            }
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            float deltaX = touch.position.x - dragStartX;
            stageContainer.anchoredPosition = new Vector2(containerDragStartX + deltaX, stageContainer.anchoredPosition.y);
            
            // Only mark as swiping once finger moves beyond threshold
            if (!isSwiping && Mathf.Abs(deltaX) > SWIPE_MOVE_THRESHOLD)
            {
                isSwiping = true;
                SetStageAlpha(swipeFadeAlpha);
            }
        }
        else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isDragging)
        {
            EndSwipe(touch.position.x - dragStartX);
        }
    }
    
    private void EndSwipe(float deltaX)
    {
        isDragging = false;
        
        // Reset alpha back to full
        SetStageAlpha(1f);
        
        bool didSwipe = Mathf.Abs(deltaX) > SWIPE_MOVE_THRESHOLD;
        
        // If finger barely moved, it's a tap → just snap back, no action
        if (!didSwipe)
        {
            UpdateSelection(true);
            isSwiping = false;
            return;
        }
        
        int previousIndex = currentIndex;
        
        if (Mathf.Abs(deltaX) > swipeThreshold)
        {
            // Swipe detected
            if (deltaX > 0 && currentIndex > 0)
            {
                // Swiped right → go to previous (left)
                currentIndex--;
            }
            else if (deltaX < 0 && currentIndex < stageImages.Count - 1)
            {
                // Swiped left → go to next (right)
                currentIndex++;
            }
        }
        
        // Only allow selection of unlocked stages
        if (IsStageUnlocked(currentIndex))
        {
            selectedStage = currentIndex + 1;
            if (currentIndex != previousIndex)
            {
                GlobalSoundManager.PlaySound(SoundType.buttonClick);
            }
        }
        else
        {
            // Can view locked stages but selection stays on last unlocked
            // Don't update selectedStage
        }
        
        // Update lock status for current viewed stage
        isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
        UpdateStageSprites();
        
        // Snap to current index
        UpdateSelection(true);
        UpdateArrowVisibility();
        
        // Clear swiping flag after a frame so play button click doesn't sneak through
        StartCoroutine(ClearSwipingFlag());
    }
    
    /// <summary>
    /// ⏱ Clear swiping flag after one frame so play button is re-enabled
    /// </summary>
    private IEnumerator ClearSwipingFlag()
    {
        yield return null;
        isSwiping = false;
    }
    
    /// <summary>
    /// 🌊 Set alpha on all stage images
    /// </summary>
    private void SetStageAlpha(float alpha)
    {
        for (int i = 0; i < stageImageComponents.Count; i++)
        {
            if (stageImageComponents[i] != null)
            {
                Color c = stageImageComponents[i].color;
                c.a = alpha;
                stageImageComponents[i].color = c;
            }
        }
    }
    
    private bool IsInSwipeRegion(Vector2 screenPosition)
    {
        if (swipeRegion == null) return true; // If no region set, allow everywhere
        return RectTransformUtility.RectangleContainsScreenPoint(swipeRegion, screenPosition, uiCamera);
    }
    
    // ========== PLAY BUTTON ==========
    
    /// <summary>
    /// 🎮 Called when player taps (not swipes) on the stage area
    /// </summary>
    private void OnPlayTap()
    {
        if (!isCurrentStageUnlocked)
        {
            // Stage is locked - show error effect
            PlayLockedErrorEffect();
            GlobalSoundManager.PlaySound(SoundType.buttonClick);
            return;
        }
        
        // Stage is unlocked - simulate play button click
        if (playButton != null)
        {
            playButton.onClick.Invoke();
        }
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
    }
    
    /// <summary>
    /// 🔴 Play error shake effect when trying to play locked stage
    /// </summary>
    private void PlayLockedErrorEffect()
    {
        // Always stop and fully reset before replaying (fixes spam leaving red color)
        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);
        
        ResetNotificationState();
        
        // Start new notification sequence
        notificationCoroutine = StartCoroutine(ShowLockedNotification());
    }
    
    /// <summary>
    /// 🔔 Show locked stage notification with flash and jiggle effects
    /// </summary>
    private IEnumerator ShowLockedNotification()
    {
        // Show notification
        if (startLockStageNotification != null)
        {
            startLockStageNotification.SetActive(true);
            
            RectTransform notificationRect = startLockStageNotification.GetComponent<RectTransform>();
            if (notificationRect != null)
            {
                // Reset scale/position before animating
                notificationRect.DOKill();
                notificationRect.localScale = Vector3.one;
                
                // Jiggle the notification
                notificationRect.DOShakeAnchorPos(errorShakeDuration * flashCount, errorShakeStrength, 20, 90, false, true);
            }
            
            // Flash notification red
            Image notificationImage = startLockStageNotification.GetComponent<Image>();
            TMP_Text notificationText = startLockStageNotification.GetComponentInChildren<TMP_Text>();
            
            if (notificationImage != null)
            {
                Color originalColor = notificationImage.color;
                notificationImage.DOKill();
                
                // Flash sequence
                Sequence flashSeq = DOTween.Sequence();
                for (int i = 0; i < flashCount; i++)
                {
                    flashSeq.Append(notificationImage.DOColor(flashColor, flashSpeed));
                    flashSeq.Append(notificationImage.DOColor(originalColor, flashSpeed));
                }
            }
            
            if (notificationText != null)
            {
                Color originalTextColor = notificationText.color;
                notificationText.DOKill();
                
                // Flash sequence
                Sequence flashSeq = DOTween.Sequence();
                for (int i = 0; i < flashCount; i++)
                {
                    flashSeq.Append(notificationText.DOColor(flashColor, flashSpeed));
                    flashSeq.Append(notificationText.DOColor(originalTextColor, flashSpeed));
                }
            }
        }
        
        // Flash the start text (always visible)
        if (startText != null)
        {
            RectTransform startTextRect = startText.GetComponent<RectTransform>();
            if (startTextRect != null)
            {
                startTextRect.DOKill();
                startTextRect.localScale = Vector3.one;
                startTextRect.DOShakeAnchorPos(errorShakeDuration * flashCount, errorShakeStrength * 0.8f, 20, 90, false, true);
            }
            
            startText.DOKill();
            
            // Flash sequence
            Sequence flashSeq = DOTween.Sequence();
            for (int i = 0; i < flashCount; i++)
            {
                flashSeq.Append(startText.DOColor(flashColor, flashSpeed));
                flashSeq.Append(startText.DOColor(startTextOriginalColor, flashSpeed));
            }
        }
        
        // Wait for duration
        yield return new WaitForSeconds(notificationDuration);
        
        // Reset and hide
        ResetNotificationState();
        notificationCoroutine = null;
    }
    
    /// <summary>
    /// 🔄 Reset notification and text states to normal
    /// </summary>
    private void ResetNotificationState()
    {
        // Hide notification
        if (startLockStageNotification != null)
        {
            startLockStageNotification.SetActive(false);
            
            // Kill all tweens and reset
            RectTransform notificationRect = startLockStageNotification.GetComponent<RectTransform>();
            if (notificationRect != null)
            {
                notificationRect.DOKill();
                notificationRect.localScale = Vector3.one;
                notificationRect.anchoredPosition = Vector2.zero;
            }
            
            // Reset canvas group alpha
            CanvasGroup notificationCanvasGroup = startLockStageNotification.GetComponent<CanvasGroup>();
            if (notificationCanvasGroup != null)
            {
                notificationCanvasGroup.DOKill();
                notificationCanvasGroup.alpha = 1f;
            }
            
            Image notificationImage = startLockStageNotification.GetComponent<Image>();
            if (notificationImage != null)
            {
                notificationImage.DOKill();
                notificationImage.color = notificationImageOriginalColor;
            }
            
            TMP_Text notificationText = startLockStageNotification.GetComponentInChildren<TMP_Text>();
            if (notificationText != null)
            {
                notificationText.DOKill();
                notificationText.color = notificationTextOriginalColor;
            }
        }
        
        // Reset start text
        if (startText != null)
        {
            startText.DOKill();
            startText.color = startTextOriginalColor;
            
            RectTransform startTextRect = startText.GetComponent<RectTransform>();
            if (startTextRect != null)
            {
                startTextRect.DOKill();
                startTextRect.localScale = Vector3.one;
                startTextRect.anchoredPosition = Vector2.zero;
            }
        }
    }
    
    /// <summary>
    /// 🎨 Update all stage image sprites based on 3 states
    /// </summary>
    private void UpdateStageSprites()
    {
        for (int i = 0; i < stageImageComponents.Count; i++)
        {
            if (stageImageComponents[i] == null) continue;
            
            bool isUnlocked = IsStageUnlocked(i);
            bool isSelected = (i == currentIndex);
            
            Sprite targetSprite;
            
            if (!isUnlocked)
            {
                targetSprite = lockedSprite;
            }
            else if (isSelected)
            {
                targetSprite = unlockedSelectedSprite;
            }
            else
            {
                targetSprite = unlockedUnselectedSprite;
            }
            
            if (targetSprite != null)
            {
                stageImageComponents[i].sprite = targetSprite;
            }
        }
    }
    
    // ========== ARROW BUTTONS ==========
    
    private void OnLeftArrowClicked()
    {
        if (isAnimating || currentIndex <= 0) return;
        
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        currentIndex--;
        
        // Only update selected stage if unlocked
        if (IsStageUnlocked(currentIndex))
        {
            selectedStage = currentIndex + 1;
        }
        
        // Update lock status for current viewed stage
        isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
        UpdateStageSprites();
        
        UpdateSelection(true);
        UpdateArrowVisibility();
    }
    
    private void OnRightArrowClicked()
    {
        if (isAnimating || currentIndex >= stageImages.Count - 1) return;
        
        GlobalSoundManager.PlaySound(SoundType.buttonClick);
        
        currentIndex++;
        
        // Only update selected stage if unlocked
        if (IsStageUnlocked(currentIndex))
        {
            selectedStage = currentIndex + 1;
        }
        
        // Update lock status for current viewed stage
        isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
        UpdateStageSprites();
        
        UpdateSelection(true);
        UpdateArrowVisibility();
    }
    
    // ========== SELECTION & ANIMATION ==========
    
    /// <summary>
    /// 🔄 Called when object becomes active - refresh unlock states and navigate to newest unlocked level
    /// </summary>
    private void OnEnable()
    {
        if (!isInitialized) return;
        
        // Reset notification state when enabled
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }
        ResetNotificationState();
        
        // Reset swipe state
        isSwiping = false;
        isDragging = false;
        SetStageAlpha(1f);
        
        // Always refresh when enabled - get fresh data from Stage singleton
        if (Stage.Instance != null)
        {
            int currentUnlockedStage = Stage.Instance.GetUnlockedStage();
            
            // Always update to ensure sync with Stage singleton
            cachedUnlockedStage = currentUnlockedStage;
            
            // Refresh all locked states
            UpdateLockedStates();
            
            // 🎯 Navigate to newest unlocked level (0-based index)
            int newestUnlockedIndex = Mathf.Clamp(currentUnlockedStage - 1, 0, stageImages.Count - 1);
            currentIndex = newestUnlockedIndex;
            selectedStage = currentUnlockedStage;
            
            // Update current stage unlock status
            isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
            UpdateStageSprites();
            UpdateSelection(false);
        }
    }
    
    /// <summary>
    /// 🔄 Called when object becomes inactive - clean up notification
    /// </summary>
    private void OnDisable()
    {
        // Stop and reset notification when disabled
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }
        ResetNotificationState();
        
        // Reset swipe state
        isSwiping = false;
        isDragging = false;
        SetStageAlpha(1f);
    }
    
    // ========== SELECTION & ANIMATION ==========
    
    private void UpdateSelection(bool animate)
    {
        if (stageImages == null || stageImages.Count == 0) return;
        if (currentIndex < 0 || currentIndex >= stageImages.Count) return;
        if (stageImages[currentIndex] == null) return;
        
        // Calculate target position based on stage width and spacing
        // Move container left by (index * (width + spacing)) to center selected stage
        float offsetFromFirst = currentIndex * (stageWidth + stageSpacing);
        float targetX = containerStartX - offsetFromFirst;
        
        if (animate)
        {
            isAnimating = true;
            stageContainer.DOKill();
            stageContainer.DOAnchorPosX(targetX, slideDuration)
                .SetEase(slideEase)
                .OnComplete(() => isAnimating = false);
        }
        else
        {
            stageContainer.anchoredPosition = new Vector2(targetX, stageContainer.anchoredPosition.y);
        }
        
        // Scale images - selected stage gets big scale (locked or unlocked)
        for (int i = 0; i < stageImages.Count; i++)
        {
            if (stageImages[i] == null) continue;
            
            Transform imgTransform = stageImages[i].transform;
            // Scale up if this is the current viewed stage (regardless of lock status)
            bool isSelected = (i == currentIndex);
            float targetScale = isSelected ? selectedScale : unselectedScale;
            
            if (animate)
            {
                imgTransform.DOKill();
                imgTransform.DOScale(targetScale, scaleDuration).SetEase(scaleEase);
            }
            else
            {
                imgTransform.localScale = Vector3.one * targetScale;
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
            rightArrowButton.gameObject.SetActive(currentIndex < stageImages.Count - 1);
    }
    
    /// <summary>
    /// 🎯 Get the currently selected stage number (1-based)
    /// </summary>
    public int GetSelectedStage()
    {
        return selectedStage;
    }
    
    /// <summary>
    /// 🔓 Check if current viewed stage is playable (unlocked)
    /// </summary>
    public bool IsCurrentStagePlayable()
    {
        return isCurrentStageUnlocked;
    }
    
    /// <summary>
    /// 🎯 Get current view index (0-based)
    /// </summary>
    public int GetCurrentStageIndex()
    {
        return currentIndex;
    }
    
    /// <summary>
    /// 🎯 Set stage index and update view
    /// </summary>
    public void SetStageIndex(int index)
    {
        if (index >= 0 && index < stageImages.Count)
        {
            currentIndex = index;
            if (IsStageUnlocked(index))
            {
                selectedStage = index + 1;
            }
            UpdateSelection(false);
            UpdateArrowVisibility();
        }
    }
    
    /// <summary>
    /// 🔄 Refresh locked states (call after unlocking new stages)
    /// </summary>
    public void RefreshLockedStates()
    {
        UpdateLockedStates();
        isCurrentStageUnlocked = IsStageUnlocked(currentIndex);
        UpdateStageSprites();
        UpdateSelection(false);
        
        // Update cached value
        if (Stage.Instance != null)
            cachedUnlockedStage = Stage.Instance.GetUnlockedStage();
    }
    
    private void OnDestroy()
    {
        if (leftArrowButton != null)
            leftArrowButton.onClick.RemoveListener(OnLeftArrowClicked);
        
        if (rightArrowButton != null)
            rightArrowButton.onClick.RemoveListener(OnRightArrowClicked);
    }
}
