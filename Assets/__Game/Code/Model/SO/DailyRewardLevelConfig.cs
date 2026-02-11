using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 🎁 Single currency reward entry (type + amount)
/// </summary>
[System.Serializable]
public class DailyRewardEntry
{
    public EnumCurrency currencyType;
    public int amount;
}

/// <summary>
/// 🎁 Rewards for a single unlocked stage (contains 5 reward slots for 5 buttons)
/// </summary>
[System.Serializable]
public class DailyRewardLevel
{
    [Tooltip("List of rewards for each button at this stage (index 0 = button 1, etc.)")]
    public List<DailyRewardEntry> rewards = new List<DailyRewardEntry>();
}

/// <summary>
/// 🎁 Daily Reward Level Config - Rewards scale with max unlocked stage
/// Each entry contains 5 currency rewards (one per daily reward button)
/// Create: Assets > Create > Game > Daily Reward Level Config
/// Place in Resources folder!
/// </summary>
[CreateAssetMenu(fileName = "DailyRewardLevelConfig", menuName = "Game/Daily Reward Level Config")]
public class DailyRewardLevelConfig : ScriptableObject
{
    private static DailyRewardLevelConfig _instance;
    public static DailyRewardLevelConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<DailyRewardLevelConfig>("DailyRewardLevelConfig");
                if (_instance == null)
                    Debug.LogError("❌ DailyRewardLevelConfig not found! Create one in Resources folder!");
            }
            return _instance;
        }
    }

    [Header("🔧 Auto-Generate Settings")]
    [Tooltip("Base reward amount for Stage 1, Button 1")]
    public int baseValue = 100;

    [Tooltip("Within a stage: button 0 = base, button 1 = base×2, ... button 4 = base×5")]
    public int buttonsPerStage = 5;

    [Tooltip("Multiply base by this each stage (stage2 base = stage1 base × this)")]
    public float baseMultiplyPerStage = 1.5f;

    [Tooltip("Number of stages to generate")]
    public int stageCount = 8;

    [Tooltip("Index 0 = Stage 1 rewards, Index 1 = Stage 2 rewards, etc.")]
    public List<DailyRewardLevel> levelRewards = new List<DailyRewardLevel>();

    /// <summary>
    /// 🎁 Get reward entry for a specific unlocked stage and button index
    /// Stage is 1-based, buttonIndex is 0-based
    /// Falls back to last stage if maxUnlockedStage exceeds list
    /// </summary>
    public DailyRewardEntry GetReward(int maxUnlockedStage, int buttonIndex)
    {
        if (levelRewards == null || levelRewards.Count == 0) return null;

        int stageIndex = Mathf.Clamp(maxUnlockedStage - 1, 0, levelRewards.Count - 1);
        var stageRewards = levelRewards[stageIndex];

        if (stageRewards.rewards == null || stageRewards.rewards.Count == 0) return null;

        int rewardIndex = Mathf.Clamp(buttonIndex, 0, stageRewards.rewards.Count - 1);
        return stageRewards.rewards[rewardIndex];
    }

    /// <summary>
    /// 🎁 Get currency type for a specific unlocked stage and button index
    /// </summary>
    public EnumCurrency GetCurrencyType(int maxUnlockedStage, int buttonIndex)
    {
        var entry = GetReward(maxUnlockedStage, buttonIndex);
        return entry != null ? entry.currencyType : EnumCurrency.blueBits;
    }

    /// <summary>
    /// 🎁 Get reward amount for a specific unlocked stage and button index
    /// </summary>
    public int GetAmount(int maxUnlockedStage, int buttonIndex)
    {
        var entry = GetReward(maxUnlockedStage, buttonIndex);
        return entry != null ? entry.amount : 50;
    }
}
