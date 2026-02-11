using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AdsUIController : MonoBehaviour
{
    [Header("🎁 Daily Reward")]
    public Button dailyRewardButton;
    public GameObject dailyRewardPanel;
    public Button dailyRewardCloseButton;
    public Button dailyRewardConfirmButton;

    [Header("🚫 No Ads")]
    public Button noAdsButton;
    public GameObject noAdsPanel;
    public Button noAdsCloseButton;
    public Button noAdsPurchaseButton;
    public GameObject noAdsBackground;

    [Header("🎬 Animation Settings")]
    public float panelAnimationDuration = 0.3f;
    public Ease panelOpenEase = Ease.OutBack;
    public Ease panelCloseEase = Ease.InBack;
    public float fadeAnimationDuration = 0.3f;
    public Ease fadeInEase = Ease.OutQuad;
    public Ease fadeOutEase = Ease.InQuad;

    [Header("🔗 Other Controllers to Disable")]
    public UIController uiController;
    public ControlUpgradeButton controlUpgradeButton;
    public BreachTerminateManager breachTerminateManager;
    public Button[] otherButtonsToDisable;

    private Vector3 dailyRewardPanelOriginalScale;
    private Vector3 noAdsPanelOriginalScale;
    private bool isDailyRewardPanelOpen = false;
    private bool isNoAdsPanelOpen = false;

    private void Start()
    {
        // Setup Daily Reward
        if (dailyRewardPanel != null)
        {
            dailyRewardPanelOriginalScale = dailyRewardPanel.transform.localScale;
            dailyRewardPanel.SetActive(false);
        }

        if (dailyRewardButton != null)
        {
            dailyRewardButton.onClick.AddListener(OpenDailyRewardPanel);
        }

        if (dailyRewardCloseButton != null)
        {
            dailyRewardCloseButton.onClick.AddListener(CloseDailyRewardPanel);
        }

        if (dailyRewardConfirmButton != null)
        {
            dailyRewardConfirmButton.onClick.AddListener(OnDailyRewardConfirm);
        }

        // Setup No Ads
        if (noAdsPanel != null)
        {
            noAdsPanelOriginalScale = noAdsPanel.transform.localScale;
            noAdsPanel.SetActive(false);
        }

        if (noAdsButton != null)
        {
            noAdsButton.onClick.AddListener(OpenNoAdsPanel);
        }

        if (noAdsCloseButton != null)
        {
            noAdsCloseButton.onClick.AddListener(CloseNoAdsPanel);
        }

        if (noAdsPurchaseButton != null)
        {
            noAdsPurchaseButton.onClick.AddListener(OnNoAdsPurchase);
        }

        // Setup No Ads Background
        if (noAdsBackground != null)
        {
            noAdsBackground.SetActive(false);
        }
    }

    // 🎁 Open Daily Reward Panel
    public void OpenDailyRewardPanel()
    {
        if (dailyRewardPanel != null && !isDailyRewardPanelOpen)
        {
            // 🔊 Play button click sound
            GlobalSoundManager.PlaySound(SoundType.buttonClick);

            // 🔒 Disable outside buttons
            SetOutsideButtonsInteractable(false);

            dailyRewardPanel.SetActive(true);
            dailyRewardPanel.transform.localScale = Vector3.zero;
            dailyRewardPanel.transform.DOScale(dailyRewardPanelOriginalScale, panelAnimationDuration)
                .SetEase(panelOpenEase)
                .SetUpdate(true);

            isDailyRewardPanelOpen = true;
        }
    }

    // 🎁 Close Daily Reward Panel
    public void CloseDailyRewardPanel()
    {
        if (dailyRewardPanel != null && isDailyRewardPanelOpen)
        {
            // 🔊 Play button click sound
            GlobalSoundManager.PlaySound(SoundType.buttonClick);

            dailyRewardPanel.transform.DOScale(Vector3.zero, panelAnimationDuration * 0.7f)
                .SetEase(panelCloseEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    dailyRewardPanel.SetActive(false);

                    // ✅ Enable outside buttons
                    SetOutsideButtonsInteractable(true);
                });

            isDailyRewardPanelOpen = false;
        }
    }

    // 🎁 Daily Reward Confirm Action
    private void OnDailyRewardConfirm()
    {
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);

        // TODO: Add daily reward logic here (watch ad, give reward, etc.)
        MizuLog.General("Daily Reward Confirmed!");

        CloseDailyRewardPanel();
    }

    // 🚫 Open No Ads Panel
    public void OpenNoAdsPanel()
    {
        if (noAdsPanel != null && !isNoAdsPanelOpen)
        {
            // 🔊 Play button click sound
            GlobalSoundManager.PlaySound(SoundType.buttonClick);

            // 🔒 Disable outside buttons
            SetOutsideButtonsInteractable(false);

            // Show background
            if (noAdsBackground != null)
                noAdsBackground.SetActive(true);

            noAdsPanel.SetActive(true);
            noAdsPanel.transform.localScale = Vector3.zero;
            noAdsPanel.transform.DOScale(noAdsPanelOriginalScale, panelAnimationDuration)
                .SetEase(panelOpenEase)
                .SetUpdate(true);

            isNoAdsPanelOpen = true;
        }
    }

    // 🚫 Close No Ads Panel
    public void CloseNoAdsPanel()
    {
        if (noAdsPanel != null && isNoAdsPanelOpen)
        {
            // 🔊 Play button click sound
            GlobalSoundManager.PlaySound(SoundType.buttonClick);

            noAdsPanel.transform.DOScale(Vector3.zero, panelAnimationDuration * 0.7f)
                .SetEase(panelCloseEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    noAdsPanel.SetActive(false);

                    // Hide background
                    if (noAdsBackground != null)
                        noAdsBackground.SetActive(false);

                    // ✅ Enable outside buttons
                    SetOutsideButtonsInteractable(true);
                });

            isNoAdsPanelOpen = false;
        }
    }

    // 🚫 No Ads Purchase Action
    private void OnNoAdsPurchase()
    {
        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);

        // TODO: Add IAP purchase logic here
        MizuLog.General("No Ads Purchase!");

        CloseNoAdsPanel();
    }

    //  Enable/disable buttons outside panels
    private void SetOutsideButtonsInteractable(bool interactable)
    {
        // Disable ads buttons themselves
        if (dailyRewardButton != null) dailyRewardButton.interactable = interactable;
        if (noAdsButton != null) noAdsButton.interactable = interactable;

        // Disable other specified buttons
        if (otherButtonsToDisable != null)
        {
            foreach (var btn in otherButtonsToDisable)
            {
                if (btn != null) btn.interactable = interactable;
            }
        }

        // Disable UIController buttons if linked
        if (uiController != null)
        {
            if (uiController.resourceButton != null)
                uiController.resourceButton.interactable = interactable;
            if (uiController.settingButton != null)
                uiController.settingButton.interactable = interactable;
            if (uiController.confirmUpgradeButton != null)
                uiController.confirmUpgradeButton.interactable = interactable;

            if (uiController.breachButtons != null)
            {
                foreach (var btn in uiController.breachButtons)
                {
                    if (btn != null) btn.interactable = interactable;
                }
            }
        }
        
        // 🔒 Lock upgrade movement
        if (controlUpgradeButton != null)
        {
            controlUpgradeButton.enabled = interactable;
        }

        // 🔒 Disable breach/terminate buttons
        if (breachTerminateManager != null)
        {
            if (breachTerminateManager.breachButton != null)
                breachTerminateManager.breachButton.interactable = interactable;
            if (breachTerminateManager.terminateButton != null)
                breachTerminateManager.terminateButton.interactable = interactable;
        }
    }

    // Check if any panel is open
    public bool IsAnyPanelOpen()
    {
        return isDailyRewardPanelOpen || isNoAdsPanelOpen;
    }

    private void OnDestroy()
    {
        if (dailyRewardButton != null)
            dailyRewardButton.onClick.RemoveListener(OpenDailyRewardPanel);

        if (dailyRewardCloseButton != null)
            dailyRewardCloseButton.onClick.RemoveListener(CloseDailyRewardPanel);

        if (dailyRewardConfirmButton != null)
            dailyRewardConfirmButton.onClick.RemoveListener(OnDailyRewardConfirm);

        if (noAdsButton != null)
            noAdsButton.onClick.RemoveListener(OpenNoAdsPanel);

        if (noAdsCloseButton != null)
            noAdsCloseButton.onClick.RemoveListener(CloseNoAdsPanel);

        if (noAdsPurchaseButton != null)
            noAdsPurchaseButton.onClick.RemoveListener(OnNoAdsPurchase);
    }
}
