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
    public Sprite spriteClaimed;
    public TextMeshProUGUI claimedText;
}

public class DailyReward : MonoBehaviour
{
    [Header("Daily Reward Buttons")]
    [SerializeField] private List<DailyRewardButton> rewardButtons = new List<DailyRewardButton>();

    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Animation Settings")]
    [SerializeField] private float buttonJumpHeight = 20f;
    [SerializeField] private float buttonJumpDuration = 0.15f;
    [SerializeField] private float textFadeDuration = 0.3f;
    [SerializeField] private float textScaleDuration = 0.3f;

    private const int RESET_HOUR = 0; // 12 PM

    private int claimedCount = 0;
    private SaveData cachedSaveData;
    private DateTime nextResetTime;

    private void Start()
    {
        cachedSaveData = SaveSystem.Instance.LoadGame();
        LoadProgress();
        SetupButtons();
        UpdateButtonStates();
        CalculateNextResetTime();
    }

    private void Update()
    {
        UpdateCountdown();
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
            LoadProgress();
            UpdateButtonStates();
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

            // Set initial text to "Free"
            if (rewardButton.claimedText != null)
            {
                rewardButton.claimedText.text = "Free";
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
                rewardButton.buttonImage.sprite = isClaimed ? rewardButton.spriteClaimed : rewardButton.spriteBeforeClaimed;
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
            else if (rewardButton.claimedText != null)
            {
                rewardButton.claimedText.text = "Free";
            }
        }
    }

    private void OnRewardButtonClicked(int index)
    {
        // Can only claim the next button in sequence
        if (index != claimedCount) return;

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
        // TODO: Add your reward logic here based on index
        // Example:
        // switch (index)
        // {
        //     case 0: // Day 1 reward
        //         break;
        //     case 1: // Day 2 reward
        //         break;
        //     // etc...
        // }
        Debug.Log($"[DailyReward] Claimed reward {index + 1}!");
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
        LoadProgress();
        UpdateButtonStates();
    }
}
