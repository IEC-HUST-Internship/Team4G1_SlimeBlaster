using UnityEngine;

public class Boss : Enemy
{
    public bool isDefeated = false;
    private bool isDying = false;  // 🚩 Flag to stop movement during death
    Vector2 bossDirection;
    
    // 🩷 Pink Slime Animation reference
    private PinkSlimeAnimation pinkSlimeAnim;

    protected override void OnEnable()
    {
        base.OnEnable();  // This initializes slimeAnim, playerCombatArena, and other base components
        
        isDefeated = false;
        isDying = false;  // ♻️ Reset dying flag on enable
        targetPosition = new Vector2(0, 0);
        
        // 🩷 Get PinkSlimeAnimation component
        pinkSlimeAnim = GetComponent<PinkSlimeAnimation>();
        
        // 🩷 Reset pink slime animation state
        if (pinkSlimeAnim != null)
            pinkSlimeAnim.ResetDeathState();
    }
    
    /// <summary>
    /// 👑 Initialize boss with boss-specific multipliers (not enemy multipliers)
    /// </summary>
    protected override void InitializeEnemy()
    {
        if (enemyData != null)
        {
            int stage = Stage.Instance != null ? Stage.Instance.GetStage() : 1;
            float healthMult = GameConfig.Instance != null ? GameConfig.Instance.GetBossHealthMultiplier(stage) : 1f;
            maxHealth = Mathf.RoundToInt(enemyData.hp * healthMult);
            currentHealth = maxHealth;
            MizuLog.Spawn($"👑 Boss initialized: HP={maxHealth} (x{healthMult} at stage {stage})");
        }
    }
    
    /// <summary>
    /// 👑 Get boss reflection multiplier (uses boss config, not enemy config)
    /// </summary>
    public new float GetEnemyMultiplierBaseReflection()
    {
        int stage = Stage.Instance != null ? Stage.Instance.GetStage() : 1;
        float reflectMult = GameConfig.Instance != null ? GameConfig.Instance.GetBossReflectionMultiplier(stage) : 1f;
        float baseMultiplier = enemyData.baseReflectionMultiplier * reflectMult;
        
        // 😠 Check for angry multiplier
        if (pinkSlimeAnim != null && pinkSlimeAnim.IsAngry())
        {
            baseMultiplier *= pinkSlimeAnim.GetAngryReflectionMultiplier();
        }
        
        return baseMultiplier;
    }

    protected override void OnDisable()
    {
        // Don't call base.OnDisable() to prevent returning to pool
    }
    
    /// <summary>
    /// 👑 Spawn currency with boss-specific multiplier
    /// </summary>
    protected override void SpawnCurrency()
    {
        if (enemyData == null || currencyReference == null) return;

        // Get the correct currency pool from StoreCurrencyReference
        ObjectPool selectedPool = currencyReference.GetCurrencyPool(enemyData.currencyType);
        
        if (selectedPool == null) return;

        int stage = Stage.Instance != null ? Stage.Instance.GetStage() : 1;
        // 👑 Use BOSS currency multiplier instead of enemy
        float currencyMult = GameConfig.Instance != null ? GameConfig.Instance.GetBossCurrencyMultiplier(stage) : 1f;
        int baseAmount = enemyData.baseCurrencyAmount;
        
        // Add additional currency drops based on the currency type
        int additionalAmount = 0;
        if (playerStats != null)
        {
            switch (enemyData.currencyType)
            {
                case EnumCurrency.blueBits:
                    additionalAmount = playerStats.GetStatValue(EnumStat.additionalBlueBitsDropPerEnemy);
                    break;
                case EnumCurrency.pinkBits:
                    additionalAmount = playerStats.GetStatValue(EnumStat.additionalPinkBitsDropPerEnemy);
                    break;
                case EnumCurrency.yellowBits:
                    additionalAmount = playerStats.GetStatValue(EnumStat.additionalYellowBitsDropPerEnemy);
                    break;
                case EnumCurrency.greenBits:
                    additionalAmount = playerStats.GetStatValue(EnumStat.additionalGreenBitsDropPerEnemy);
                    break;
            }
        }
        
        // Apply multiplier AFTER all bonuses: (base + bonus) * multiplier
        int totalAmount = Mathf.RoundToInt((baseAmount + additionalAmount) * currencyMult);
        MizuLog.Combat($"👑 Boss drops {totalAmount} currency (x{currencyMult} at stage {stage})");
        
        // Get collider bounds
        Collider2D col = GetComponent<Collider2D>();
        
        for (int i = 0; i < totalAmount; i++)
        {
            Vector3 spawnPos;
            
            if (col != null)
            {
                // Spawn random position inside collider bounds
                Bounds bounds = col.bounds;
                spawnPos = new Vector3(
                    UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                    UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                    transform.position.z
                );
            }
            else
            {
                // Fallback if no collider
                spawnPos = transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f;
            }
            
            GameObject currencyObj = selectedPool.Get(spawnPos, Quaternion.identity);
            
            // Set the pool reference so it can return to pool
            CurrencyControl currencyControl = currencyObj.GetComponent<CurrencyControl>();
            if (currencyControl != null)
            {
                currencyControl.pool = selectedPool;
            }
        }
    }

    // Set target position (uses base Enemy's targetPosition)
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
        bossDirection = ((Vector2)target - (Vector2)transform.position).normalized;
    }

    protected override void Update()
    {
        // 🚩 Don't move if dying
        if (isDying) return;
        
        // Always move in the locked direction
        transform.position += (Vector3)(bossDirection * moveSpeed * Time.deltaTime);
        CheckOffscreen();
    }

    protected override void Die()
    {
        // 🚩 Stop movement
        isDying = true;
        
        // Spawn currency
        SpawnCurrency();

        // 🩷 Play pink slime death animation if available
        if (pinkSlimeAnim != null)
        {
            pinkSlimeAnim.PlayPinkSlimeDeathAnimation(() =>
            {
                // ✅ After death animation + wait time, trigger ShowWin
                isDefeated = true;
                
                if (playerCombatArena != null)
                    playerCombatArena.TriggerShowWin();
                
                // Hide boss after animation completes
                gameObject.SetActive(false);
                
                MizuLog.Combat("Boss defeated with pink slime death animation!");
            });
        }
        else
        {
            // Fallback: original behavior if no PinkSlimeAnimation
            isDefeated = true;
            gameObject.SetActive(false);
            MizuLog.Combat("Boss defeated!");
        }
    }
}
