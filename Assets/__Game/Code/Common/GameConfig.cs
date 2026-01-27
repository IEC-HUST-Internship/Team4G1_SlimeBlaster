using UnityEngine;

/// <summary>
/// 🎮 Game Config - Store magic numbers for designers
/// Create: Assets > Create > Game > Game Config
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
public class GameConfig : ScriptableObject
{
    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfig>("GameConfig");
                if (_instance == null)
                    Debug.LogError("❌ GameConfig not found! Create one in Resources folder!");
            }
            return _instance;
        }
    }

    [Header("⏱️ HP Loss Scaling (Per Stage)")]
    [Tooltip("Seconds between each HP loss increase per stage (index 0 = stage 1, etc.)")]
    public float[] hpLossIncreaseIntervals = new float[] { 30f, 25f, 20f, 15f, 10f };
    
    [Tooltip("How much HP loss increases each interval per stage (index 0 = stage 1, etc.)")]
    public int[] hpLossIncreaseAmounts = new int[] { 1, 1, 2, 2, 3 };
    
    [Tooltip("After 3 minutes, divide HP loss intervals by this value (makes damage ramp faster)")]
    public float hpLossIncreaseIntervalsDivideAfter3Min = 2f;
    
    [Tooltip("Flat bonus added to enemy reflection after 3 minutes")]
    public float enemyReflectionBonusAfter3Min = 10f;
    
    /// <summary>
    /// ⏱️ Get HP loss increase interval for the given stage (1-based)
    /// Falls back to last value if stage exceeds array length
    /// Pass gameTime to apply the 3-minute speed boost
    /// </summary>
    public float GetHpLossIncreaseInterval(int stage, float gameTime = 0f)
    {
        if (hpLossIncreaseIntervals == null || hpLossIncreaseIntervals.Length == 0) return 30f;
        int index = Mathf.Clamp(stage - 1, 0, hpLossIncreaseIntervals.Length - 1);
        float interval = hpLossIncreaseIntervals[index];
        
        // After 3 minutes (180 seconds), divide interval to make damage ramp faster
        if (gameTime > 180f && hpLossIncreaseIntervalsDivideAfter3Min > 0f)
        {
            interval /= hpLossIncreaseIntervalsDivideAfter3Min;
        }
        
        return interval;
    }
    
    /// <summary>
    /// ⏱️ Get HP loss increase amount for the given stage (1-based)
    /// Falls back to last value if stage exceeds array length
    /// </summary>
    public int GetHpLossIncreaseAmount(int stage)
    {
        if (hpLossIncreaseAmounts == null || hpLossIncreaseAmounts.Length == 0) return 1;
        int index = Mathf.Clamp(stage - 1, 0, hpLossIncreaseAmounts.Length - 1);
        return hpLossIncreaseAmounts[index];
    }

    [Header("📐 EXP Formula")]
    [Tooltip("EXP needed = level × this value (100 = level 1 needs 100, level 2 needs 200...)")]
    public int expPerLevelMultiplier = 100;

    [Header("👾 Enemy Scaling (Per Stage)")]
    [Tooltip("Multiply enemy HP by this for each stage (index 0 = stage 1, index 1 = stage 2, etc.)")]
    public float[] enemyHealthMultipliers = new float[] { 1f, 1f, 1f, 1f, 1f };
    
    [Tooltip("Multiply enemy reflection by this for each stage (index 0 = stage 1, index 1 = stage 2, etc.)")]
    public float[] enemyReflectionMultipliers = new float[] { 1f, 1f, 1f, 1f, 1f };
    
    [Tooltip("Multiply currency drop by this for each stage (index 0 = stage 1, index 1 = stage 2, etc.)")]
    public float[] enemyCurrencyMultipliers = new float[] { 1f, 1f, 1f, 1f, 1f };
    
    [Header("👑 Boss Scaling (Per Stage)")]
    [Tooltip("Multiply boss HP by this for each stage (index 0 = stage 1, index 1 = stage 2, etc.)")]
    public float[] bossHealthMultipliers = new float[] { 1f, 1.5f, 2f, 2.5f, 3f };
    
    [Tooltip("Multiply boss reflection by this for each stage (index 0 = stage 1, index 1 = stage 2, etc.)")]
    public float[] bossReflectionMultipliers = new float[] { 1f, 1.2f, 1.5f, 1.8f, 2f };
    
    [Tooltip("Multiply boss currency drop by this for each stage (index 0 = stage 1, index 1 = stage 2, etc.)")]
    public float[] bossCurrencyMultipliers = new float[] { 1f, 1.5f, 2f, 2.5f, 3f };
    
    /// <summary>
    /// 🎯 Get enemy health multiplier for the given level (1-based)
    /// Falls back to last value if level exceeds array length
    /// </summary>
    public float GetEnemyHealthMultiplier(int level)
    {
        if (enemyHealthMultipliers == null || enemyHealthMultipliers.Length == 0) return 1f;
        int index = Mathf.Clamp(level - 1, 0, enemyHealthMultipliers.Length - 1);
        return enemyHealthMultipliers[index];
    }
    
    /// <summary>
    /// 🎯 Get enemy reflection multiplier for the given level (1-based)
    /// Falls back to last value if level exceeds array length
    /// </summary>
    public float GetEnemyReflectionMultiplier(int level)
    {
        if (enemyReflectionMultipliers == null || enemyReflectionMultipliers.Length == 0) return 1f;
        int index = Mathf.Clamp(level - 1, 0, enemyReflectionMultipliers.Length - 1);
        return enemyReflectionMultipliers[index];
    }
    
    /// <summary>
    /// 🎯 Get enemy currency multiplier for the given stage (1-based)
    /// Falls back to last value if stage exceeds array length
    /// </summary>
    public float GetEnemyCurrencyMultiplier(int stage)
    {
        if (enemyCurrencyMultipliers == null || enemyCurrencyMultipliers.Length == 0) return 1f;
        int index = Mathf.Clamp(stage - 1, 0, enemyCurrencyMultipliers.Length - 1);
        return enemyCurrencyMultipliers[index];
    }
    
    /// <summary>
    /// 👑 Get boss health multiplier for the given stage (1-based)
    /// Falls back to last value if stage exceeds array length
    /// </summary>
    public float GetBossHealthMultiplier(int stage)
    {
        if (bossHealthMultipliers == null || bossHealthMultipliers.Length == 0) return 1f;
        int index = Mathf.Clamp(stage - 1, 0, bossHealthMultipliers.Length - 1);
        return bossHealthMultipliers[index];
    }
    
    /// <summary>
    /// 👑 Get boss reflection multiplier for the given stage (1-based)
    /// Falls back to last value if stage exceeds array length
    /// </summary>
    public float GetBossReflectionMultiplier(int stage)
    {
        if (bossReflectionMultipliers == null || bossReflectionMultipliers.Length == 0) return 1f;
        int index = Mathf.Clamp(stage - 1, 0, bossReflectionMultipliers.Length - 1);
        return bossReflectionMultipliers[index];
    }
    
    /// <summary>
    /// 👑 Get boss currency multiplier for the given stage (1-based)
    /// Falls back to last value if stage exceeds array length
    /// </summary>
    public float GetBossCurrencyMultiplier(int stage)
    {
        if (bossCurrencyMultipliers == null || bossCurrencyMultipliers.Length == 0) return 1f;
        int index = Mathf.Clamp(stage - 1, 0, bossCurrencyMultipliers.Length - 1);
        return bossCurrencyMultipliers[index];
    }

    [Header("⚔️ Combat")]
    [Tooltip("Minimum damage after armor (can't go below this)")]
    public int minDamageAfterArmor = 1;
    
    [Tooltip("Minimum attack speed to prevent division by zero")]
    public float minAttackSpeed = 0.01f;

    [Header("🎯 Attack Size")]
    [Tooltip("Max attack size count (0-14 = 15 levels)")]
    public int maxAttackSizeCount = 14;
}
