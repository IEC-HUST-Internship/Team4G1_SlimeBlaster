using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[System.Serializable]
public class DailyRewardButton
{
    public Button mainButton;
    public Image buttonImage;
    public Sprite spriteBeforeClaimed;
    public Sprite spriteBeforeClaimedButLock;
    public Sprite spriteClaimed;
    public TextMeshProUGUI claimedText;
    public TextMeshProUGUI txtCurrencyAmount;
}

public class DailyReward : MonoBehaviour
{
    [Header("Daily Reward Buttons")]
    [SerializeField] private List<DailyRewardButton> rewardButtons = new List<DailyRewardButton>();

    [Header("Progress Bar")]
    [SerializeField] private Image progressBarImage;
    [SerializeField] private Sprite defaultProgressBarSprite; // Empty bar sprite (no claims)
    [SerializeField] private List<Sprite> progressBarSprites = new List<Sprite>(); // 4 sprites for each claim step

    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Animation Settings")]
    [SerializeField] private float buttonJumpHeight = 20f;
    [SerializeField] private float buttonJumpDuration = 0.15f;
    [SerializeField] private float textFadeDuration = 0.3f;
    [SerializeField] private float textScaleDuration = 0.3f;

    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Currency Icons")]
    [SerializeField] private List<CurrencySprite> currencySprites = new List<CurrencySprite>();

    [Header("Rewarded Ads")]
    [SerializeField] private RewardedAds rewardedAds;

    private const int RESET_HOUR = 0; // 12 PM

    private int claimedCount = 0;
    private int cachedMaxUnlockedStage = 1;
    private SaveData cachedSaveData;
    private DateTime nextResetTime;

    private void Start()
    {
        cachedSaveData = SaveSystem.Instance.LoadGame();
        cachedMaxUnlockedStage = cachedSaveData.maxUnlockedStage;
        LoadProgress();
        SetupButtons();
        UpdateButtonStates();
        UpdateCurrencyAmountTexts();
        CalculateNextResetTime();
    }

    private void OnEnable()
    {
        if (cachedSaveData == null) return; // Start() hasn't run yet
        cachedSaveData = SaveSystem.Instance.LoadGame();
        cachedMaxUnlockedStage = cachedSaveData.maxUnlockedStage;
        LoadProgress();
        UpdateButtonStates();
        UpdateCurrencyAmountTexts();
        CalculateNextResetTime();
    }

    private void Update()
    {
        UpdateCountdown();
        CheckStageChanged();
    }

    /// <summary>
    /// Check if max unlocked stage changed and refresh currency amount texts
    /// </summary>
    private void CheckStageChanged()
    {
        if (SaveSystem.Instance == null) return;
        var saveData = SaveSystem.Instance.LoadGame();
        int currentMaxStage = saveData.maxUnlockedStage;
        if (currentMaxStage != cachedMaxUnlockedStage)
        {
            cachedMaxUnlockedStage = currentMaxStage;
            UpdateCurrencyAmountTexts();
        }
    }

    /// <summary>
    /// Update all button currency amount texts based on current player level
    /// </summary>
    private void UpdateCurrencyAmountTexts()
    {
        if (DailyRewardLevelConfig.Instance == null)
        {
            Debug.LogError("[DailyReward] DailyRewardLevelConfig.Instance is NULL!");
            return;
        }

        Debug.Log($"[DailyReward] Updating currency texts for stage {cachedMaxUnlockedStage}, buttons: {rewardButtons.Count}");

        for (int i = 0; i < rewardButtons.Count; i++)
        {
            var rewardButton = rewardButtons[i];
            if (rewardButton.txtCurrencyAmount != null)
            {
                var entry = DailyRewardLevelConfig.Instance.GetReward(cachedMaxUnlockedStage, i);
                if (entry != null)
                {
                    // Set the correct TMP sprite asset for this currency type
                    SetCurrencySpriteAsset(rewardButton.txtCurrencyAmount, entry.currencyType);
                    rewardButton.txtCurrencyAmount.text = $"<sprite index=0> {entry.amount}";
                    Debug.Log($"[DailyReward] Button {i}: {entry.currencyType} x{entry.amount}");
                }
                else
                {
                    Debug.LogWarning($"[DailyReward] Button {i}: GetReward returned null for stage {cachedMaxUnlockedStage}");
                }
            }
            else
            {
                Debug.LogWarning($"[DailyReward] Button {i}: txtCurrencyAmount is NOT assigned!");
            }
        }
    }

    /// <summary>
    /// Set the TMP sprite asset on a text component based on currency type
    /// </summary>
    private void SetCurrencySpriteAsset(TextMeshProUGUI tmp, EnumCurrency currencyType)
    {
        if (tmp == null || currencySprites == null) return;

        foreach (var currencySprite in currencySprites)
        {
            if (currencySprite.currencyType == currencyType && currencySprite.spriteAsset != null)
            {
                tmp.spriteAsset = currencySprite.spriteAsset;
                return;
            }
        }
    }

    private void CalculateNextResetTime()
    {
        DateTime now = DateTime.Now;
        DateTime todayReset = now.Date.AddHours(RESET_HOUR);
        
        if (now >= todayReset)
        {
            // Next reset is tomorrow at 12 PM
            nextResetTime = todayReset.AddDays(1);
        }
        else
        {
            // Next reset is today at 12 PM
            nextResetTime = todayReset;
        }
    }

    private void UpdateCountdown()
    {
        if (countdownText == null) return;

        TimeSpan remaining = nextResetTime - DateTime.Now;
        
        if (remaining.TotalSeconds <= 0)
        {
            countdownText.text = "00:00:00";
            // Reset has occurred, reload progress
            cachedSaveData = SaveSystem.Instance.LoadGame();
            cachedMaxUnlockedStage = cachedSaveData.maxUnlockedStage;
            LoadProgress();
            UpdateButtonStates();
            UpdateCurrencyAmountTexts();
            CalculateNextResetTime();
        }
        else
        {
            countdownText.text = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }

    private void LoadProgress()
    {
        string lastClaimDateStr = cachedSaveData.dailyRewardLastClaimDate;
        
        if (!string.IsNullOrEmpty(lastClaimDateStr))
        {
            DateTime lastClaimDate = DateTime.Parse(lastClaimDateStr);
            DateTime now = DateTime.Now;
            DateTime todayReset = now.Date.AddHours(RESET_HOUR);

            // If current time is past today's reset and last claim was before today's reset
            if (now >= todayReset && lastClaimDate < todayReset)
            {
                // Reset progress
                claimedCount = 0;
                cachedSaveData.dailyRewardClaimedCount = 0;
                SaveSystem.Instance.SaveGame(cachedSaveData);
            }
            else
            {
                claimedCount = cachedSaveData.dailyRewardClaimedCount;
            }
        }
        else
        {
            claimedCount = cachedSaveData.dailyRewardClaimedCount;
        }
    }

    private void SetupButtons()
    {
        for (int i = 0; i < rewardButtons.Count; i++)
        {
            int index = i; // Capture for closure
            var rewardButton = rewardButtons[i];
            
            if (rewardButton.mainButton != null)
            {
                rewardButton.mainButton.onClick.AddListener(() => OnRewardButtonClicked(index));
            }

            // Set initial text: index 0 = "Free", index 1+ = "Ads" (or "Free" if No Ads purchased)
            if (rewardButton.claimedText != null)
            {
                bool isNoAds = ADSController.Instance != null && ADSController.Instance.IsNoAds;
                rewardButton.claimedText.text = (index == 0 || isNoAds) ? "Free" : "Ads";
                rewardButton.claimedText.alpha = 1f;
                rewardButton.claimedText.transform.localScale = Vector3.one;
            }
        }
    }

    private void UpdateButtonStates()
    {
        for (int i = 0; i < rewardButtons.Count; i++)
        {
            var rewardButton = rewardButtons[i];
            bool isClaimed = i < claimedCount;
            bool canClaim = i == claimedCount;

            // Update sprite
            if (rewardButton.buttonImage != null)
            {
                if (isClaimed)
                    rewardButton.buttonImage.sprite = rewardButton.spriteClaimed;
                else if (canClaim)
                    rewardButton.buttonImage.sprite = rewardButton.spriteBeforeClaimed;
                else
                    rewardButton.buttonImage.sprite = rewardButton.spriteBeforeClaimedButLock;
            }

            // Update button interactable (but don't fade when claimed)
            if (rewardButton.mainButton != null)
            {
                rewardButton.mainButton.interactable = canClaim;
                
                // Keep full color even when not interactable
                var colors = rewardButton.mainButton.colors;
                colors.disabledColor = Color.white;
                rewardButton.mainButton.colors = colors;
            }

            // Show claimed text for already claimed buttons
            if (isClaimed && rewardButton.claimedText != null)

            {
                rewardButton.claimedText.text = "Claimed";
                rewardButton.claimedText.alpha = 1f;
                rewardButton.claimedText.transform.localScale = Vector3.one;
            }
            else if (canClaim && rewardButton.claimedText != null)
            {
                // Index 0 = Free, Index 1+ = Ads (or Free if No Ads purchased)
                bool isNoAds = ADSController.Instance != null && ADSController.Instance.IsNoAds;
                rewardButton.claimedText.text = (i == 0 || isNoAds) ? "Free" : "Ads";
            }
            else if (rewardButton.claimedText != null)
            {
                // Locked — not yet claimable, show nothing
                rewardButton.claimedText.text = "";
            }
        }

        // Update progress bar sprite based on claimed count
        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        if (progressBarImage == null || progressBarSprites == null || progressBarSprites.Count == 0) return;

        if (claimedCount > 0)
        {
            int spriteIndex = Mathf.Clamp(claimedCount - 1, 0, progressBarSprites.Count - 1);
            progressBarImage.sprite = progressBarSprites[spriteIndex];
        }
        else
        {
            // No claims yet or reset - show empty/default bar
            progressBarImage.sprite = defaultProgressBarSprite != null ? defaultProgressBarSprite : progressBarSprites[0];
        }
    }

    private void OnRewardButtonClicked(int index)
    {
        // Can only claim the next button in sequence
        if (index != claimedCount) return;

        // Index 0 is free, index 1+ requires watching a rewarded ad
        bool isNoAds = ADSController.Instance != null && ADSController.Instance.IsNoAds;

        if (index == 0 || isNoAds)
        {
            // Free claim (first reward, or player purchased No Ads)
            PlayClaimAnimation(index);
        }
        else
        {
            // Show rewarded ad, claim only after player finishes watching
            if (rewardedAds != null)
            {
                // Disable button while ad is loading/showing
                var rewardButton = rewardButtons[index];
                if (rewardButton.mainButton != null)
                    rewardButton.mainButton.interactable = false;

                rewardedAds.ShowRewardedAd(
                    () => { PlayClaimAnimation(index); },
                    () => {
                        // Ad failed or player closed early — re-enable button
                        if (rewardButton.mainButton != null)
                            rewardButton.mainButton.interactable = true;
                    }
                );
            }
            else
            {
                Debug.LogWarning("[DailyReward] RewardedAds reference is missing! Giving reward for free.");
                PlayClaimAnimation(index);
            }
        }
    }

    private void PlayClaimAnimation(int index)
    {
        var rewardButton = rewardButtons[index];

        // Button jump animation (up then down)
        if (rewardButton.mainButton != null)
        {
            rewardButton.mainButton.interactable = false;
            Vector3 originalPos = rewardButton.mainButton.transform.localPosition;
            
            rewardButton.mainButton.transform
                .DOLocalMoveY(originalPos.y + buttonJumpHeight, buttonJumpDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    rewardButton.mainButton.transform
                        .DOLocalMoveY(originalPos.y, buttonJumpDuration)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => ClaimReward(index));
                });
        }
        else
        {
            ClaimReward(index);
        }
    }

    private void ClaimReward(int index)
    {
        var rewardButton = rewardButtons[index];

        // Change sprite to claimed
        if (rewardButton.buttonImage != null)
        {
            rewardButton.buttonImage.sprite = rewardButton.spriteClaimed;
        }

        // Animate claimed text (change from Free to Claimed)
        if (rewardButton.claimedText != null)
        {
            // Scale down, change text, scale up
            rewardButton.claimedText.transform
                .DOScale(0f, textScaleDuration * 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    rewardButton.claimedText.text = "Claimed";
                    rewardButton.claimedText.transform
                        .DOScale(1f, textScaleDuration)
                        .SetEase(Ease.OutBack);
                });
        }

        // Update claimed count
        claimedCount++;
        SaveProgress();

        // Give reward here
        GiveReward(index);

        // Update next button state
        UpdateButtonStates();
    }

    private void GiveReward(int index)
    {
        if (playerStats == null || DailyRewardLevelConfig.Instance == null) return;

        EnumCurrency currencyType = DailyRewardLevelConfig.Instance.GetCurrencyType(cachedMaxUnlockedStage, index);
        int amount = DailyRewardLevelConfig.Instance.GetAmount(cachedMaxUnlockedStage, index);

        playerStats.AddCurrency(currencyType, amount);

        Debug.Log($"[DailyReward] Claimed reward {index + 1} (Stage {cachedMaxUnlockedStage}): +{amount} {currencyType}");
    }

    private void SaveProgress()
    {
        cachedSaveData.dailyRewardLastClaimDate = DateTime.Now.ToString();
        cachedSaveData.dailyRewardClaimedCount = claimedCount;
        SaveSystem.Instance.SaveGame(cachedSaveData);
    }

    // Call this to check and reset if needed (e.g., when app comes to foreground)
    public void CheckAndResetIfNeeded()
    {
        cachedSaveData = SaveSystem.Instance.LoadGame();
        cachedMaxUnlockedStage = cachedSaveData.maxUnlockedStage;
        LoadProgress();
        UpdateButtonStates();
        UpdateCurrencyAmountTexts();
    }
}
