using UnityEngine;

/// <summary>
/// 🔧 Upgrade Tutorial
/// Shows tutorial when entering upgrade screen for the first time
/// No time stop, allows player to press outside during tutorial
/// When this object is disabled, the tutorial ends
/// </summary>
public class TutorialUpgrade : TutorialBase
{
    // 🔑 Unique save key for upgrade tutorial
    protected override string TutorialSaveKey => "tutorialUpgradeShown";

    // ⏸️ Don't freeze time for upgrade tutorial
    public override void StartTutorial()
    {
        if (tutorialSteps == null || tutorialSteps.Count == 0) return;

        isTutorialActive = true;
        currentStepIndex = -1;

        // No Time.timeScale = 0 — keep game running
        // No button locking — allow player to press outside

        NextStep();
    }

    // ✅ Don't unfreeze time since we never froze it
    protected override void EndTutorial()
    {
        isTutorialActive = false;
        currentStepIndex = -1;

        HideAllSteps();

        hasShownTutorial = true;
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveTutorialShown(TutorialSaveKey, true);
        }

        // No Time.timeScale = 1 or button unlocking needed
    }

    // 🔒 No-op: don't lock/unlock any buttons
    protected override void SetButtonsLocked(bool locked) { }

    // When this object is disabled, end the tutorial
    private void OnDisable()
    {
        if (isTutorialActive && currentStepIndex >= 0)
        {
            EndTutorial();
        }
    }
}
