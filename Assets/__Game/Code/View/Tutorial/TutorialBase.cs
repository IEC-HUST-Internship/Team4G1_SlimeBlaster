using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 📚 Base class for all tutorials
/// Inherit from this to create specific tutorials (Combat, Upgrade, etc.)
/// </summary>
public abstract class TutorialBase : MonoBehaviour
{
    [Header("📚 Tutorial Button")]
    public Button tutorialButton; // Button to manually trigger tutorial

    [Header("📖 Tutorial Steps")]
    public List<GameObject> tutorialSteps = new List<GameObject>(); // List of UI elements to show in sequence

    [Header("🔒 Controllers to Lock")]
    public UIController uiController;
    public AdsUIController adsUIController;
    public ControlUpgradeButton controlUpgradeButton;

    protected int currentStepIndex = -1;
    protected bool isTutorialActive = false;
    protected bool hasShownTutorial = false;
    private bool hasStarted = false;

    // 🔑 Each tutorial must define its own save key
    protected abstract string TutorialSaveKey { get; }

    protected virtual void Start()
    {
        // Setup tutorial button
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(StartTutorial);
        }

        // Hide all tutorial steps at start
        HideAllSteps();

        hasStarted = true;

        // Load saved state and auto-start if needed (after HideAllSteps)
        if (SaveSystem.Instance != null)
        {
            hasShownTutorial = SaveSystem.Instance.GetTutorialShown(TutorialSaveKey);
        }

        if (!hasShownTutorial)
        {
            StartTutorial();
        }
    }

    protected virtual void OnEnable()
    {
        // Skip if Start() hasn't run yet — Start() will handle auto-start
        if (!hasStarted) return;

        // Load saved state from SaveSystem
        if (SaveSystem.Instance != null)
        {
            hasShownTutorial = SaveSystem.Instance.GetTutorialShown(TutorialSaveKey);
        }
        
        // If first time ever (never shown in entire game), auto start tutorial
        if (!hasShownTutorial)
        {
            StartTutorial();
        }
    }

    protected virtual void Update()
    {
        // Check for any input to go to next step
        if (isTutorialActive)
        {
            if (PlayerInputHandler.Instance.IsInputDown())
            {
                NextStep();
            }
        }
    }

    // 📚 Start tutorial from beginning
    public virtual void StartTutorial()
    {
        if (tutorialSteps == null || tutorialSteps.Count == 0) return;

        isTutorialActive = true;
        currentStepIndex = -1;

        // ⏸️ Freeze game
        Time.timeScale = 0f;
        
        // 🔒 Lock all buttons
        SetButtonsLocked(true);

        // Show first step
        NextStep();
    }

    // ➡️ Go to next step
    protected virtual void NextStep()
    {
        currentStepIndex++;

        // Check if tutorial is complete
        if (currentStepIndex >= tutorialSteps.Count)
        {
            EndTutorial();
            return;
        }

        // 🔊 Play button click sound
        GlobalSoundManager.PlaySound(SoundType.buttonClick);

        // Show current step, hide all others
        ShowStep(currentStepIndex);
    }

    // 👁️ Show specific step and hide all others
    protected virtual void ShowStep(int index)
    {
        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (tutorialSteps[i] != null)
            {
                tutorialSteps[i].SetActive(i == index);
            }
        }
    }

    // 🙈 Hide all tutorial steps
    protected virtual void HideAllSteps()
    {
        foreach (var step in tutorialSteps)
        {
            if (step != null)
            {
                step.SetActive(false);
            }
        }
    }

    // ✅ End tutorial
    protected virtual void EndTutorial()
    {
        isTutorialActive = false;
        currentStepIndex = -1;

        // Hide all steps
        HideAllSteps();

        // Mark as shown and save to JSON
        hasShownTutorial = true;
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveTutorialShown(TutorialSaveKey, true);
        }

        // ▶️ Unfreeze game
        Time.timeScale = 1f;
        
        // 🔓 Unlock all buttons
        SetButtonsLocked(false);
    }
    
    // 🔒 Lock/unlock all buttons
    protected virtual void SetButtonsLocked(bool locked)
    {
        bool interactable = !locked;
        
        // Lock UIController buttons
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

        // Lock AdsUIController buttons
        if (adsUIController != null)
        {
            if (adsUIController.dailyRewardButton != null)
                adsUIController.dailyRewardButton.interactable = interactable;
            if (adsUIController.noAdsButton != null)
                adsUIController.noAdsButton.interactable = interactable;
        }

        // Lock upgrade movement
        if (controlUpgradeButton != null)
        {
            controlUpgradeButton.enabled = interactable;
        }
        
        // Lock tutorial button itself
        if (tutorialButton != null)
        {
            tutorialButton.interactable = interactable;
        }
    }

    // 🔄 Reset tutorial (for testing or replay)
    public virtual void ResetTutorial()
    {
        hasShownTutorial = false;
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveTutorialShown(TutorialSaveKey, false);
        }
    }

    // 📊 Check if tutorial is currently active
    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }

    protected virtual void OnDestroy()
    {
        if (tutorialButton != null)
        {
            tutorialButton.onClick.RemoveListener(StartTutorial);
        }
    }
}
