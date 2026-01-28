using UnityEngine;

/// <summary>
/// 🔧 Upgrade Tutorial
/// Shows tutorial when entering upgrade screen for the first time
/// </summary>
public class TutorialUpgrade : TutorialBase
{
    // 🔑 Unique save key for upgrade tutorial
    protected override string TutorialSaveKey => "tutorialUpgradeShown";
}
