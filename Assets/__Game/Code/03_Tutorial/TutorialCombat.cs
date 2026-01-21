using UnityEngine;

/// <summary>
/// ⚔️ Combat Tutorial
/// Shows tutorial when entering combat for the first time
/// </summary>
public class TutorialCombat : TutorialBase
{
    // 🔑 Unique save key for combat tutorial
    protected override string TutorialSaveKey => "tutorialCombatShown";
}
