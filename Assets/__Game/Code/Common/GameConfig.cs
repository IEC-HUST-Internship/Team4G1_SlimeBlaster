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

    [Header("⏱️ HP Loss")]
    [Tooltip("Seconds between each HP loss tick")]
    public float hpLossInterval = 1f;

    [Header("📐 EXP Formula")]
    [Tooltip("EXP needed = level × this value (100 = level 1 needs 100, level 2 needs 200...)")]
    public int expPerLevelMultiplier = 100;

    [Header("👾 Enemy Scaling")]
    [Tooltip("Multiply enemy HP by this (on top of level)")]
    public float enemyHealthMultiplier = 1f;
    
    [Tooltip("Multiply enemy reflection by this (on top of level)")]
    public float enemyReflectionMultiplier = 1f;
    
    [Tooltip("Multiply currency drop by this (on top of level)")]
    public float enemyCurrencyMultiplier = 1f;

    [Header("⚔️ Combat")]
    [Tooltip("Minimum damage after armor (can't go below this)")]
    public int minDamageAfterArmor = 1;
    
    [Tooltip("Minimum attack speed to prevent division by zero")]
    public float minAttackSpeed = 0.01f;

    [Header("🎯 Attack Size")]
    [Tooltip("Max attack size count (0-14 = 15 levels)")]
    public int maxAttackSizeCount = 14;
}
