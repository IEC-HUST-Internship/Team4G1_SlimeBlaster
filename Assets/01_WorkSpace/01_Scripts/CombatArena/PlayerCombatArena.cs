using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerCombatArena : MonoBehaviour
{
    [Header("Stats")]
    public PlayerStats playerStats;

    [Header("Attack Settings")]
    public LayerMask enemyLayer;
    public float flashDuration = 0.1f;

    [Header("Currency Collection")]
    public LayerMask currencyLayer;
    public float currencyPickupRadius = 2f;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;     // Speed for following mouse/finger

    [Header("Combat Arena Temp Stats")]
    public int currentHp;
    public int currentExp;

    [Header("UI")]
    public Image healthBarImage;
    public Image expBarImage;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Boss")]
    public Boss bossEnemy;

    public SpriteRenderer rend;
    private Camera mainCamera;
    private bool isDead = false;

    private void OnEnable() 
    {
        mainCamera = Camera.main;
        
        // Reset player position and state
        transform.position = Vector3.zero;
        isDead = false;
        
        // Hide UI panels
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (winPanel != null)
            winPanel.SetActive(false);
        
        // Initialize current stats from PlayerStats
        if (playerStats != null)
        {
            currentHp = playerStats.GetStatValue(EnumStat.hp);
            currentExp = playerStats.GetStatValue(EnumStat.exp);
        }
        
        StartCoroutine(AttackRoutine());
        StartCoroutine(HpLossRoutine());
    }

    private void OnDisable()
    {
        // Move player out of scene
        transform.position = new Vector3(100f, 0f, 0f);
        
        // Stop all coroutines
        StopAllCoroutines();
        
        if (rend != null)
        {
            Color color = rend.color;
            color.a = 0f;
            rend.color = color;
        }
    }
    private void Update()
    {
        if (!isDead)
        {
            HandleMovement();
            CheckCurrencyPickup();
            CheckBossDefeat();
        }
        UpdateHealthBar();
        UpdateExpBar();
    }

    private void CheckBossDefeat()
    {
        // Check if boss is defeated
        if (bossEnemy != null && bossEnemy.isDefeated)
        {
            StartCoroutine(ShowWinAfterDelay());
            bossEnemy = null; // Prevent checking multiple times
        }
    }

    private IEnumerator ShowWinAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        
        isDead = true;
        
        // Move player out of scene
        transform.position = new Vector3(100f, 0f, 0f);
        
        // Show win panel
        if (winPanel != null)
            winPanel.SetActive(true);
        
        Debug.Log("Player won!");
    }

    private void CheckCurrencyPickup()
    {
        // Detect all currency within pickup radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currencyPickupRadius, currencyLayer);

        foreach (var hit in hits)
        {
            CurrencyControl currency = hit.GetComponent<CurrencyControl>();
            if (currency != null && !currency.IsFlying())
            {
                // Start flying towards player (currency will be added after 1 second)
                currency.StartFlyingToPlayer(transform, playerStats);
            }
        }
    }
    
    private void UpdateHealthBar()
    {
        if (healthBarImage != null && playerStats != null)
        {
            int maxHp = playerStats.GetStatValue(EnumStat.hp);
            if (maxHp > 0)
            {
                float fillAmount = (float)currentHp / maxHp;
                healthBarImage.fillAmount = Mathf.Clamp01(fillAmount);
            }
        }
    }
    private void UpdateExpBar()
    {
        if (expBarImage != null)
        {
            // Assuming max exp is 100 for now (you can change this logic)
            float fillAmount = (float)currentExp / 100f;
            expBarImage.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }
    private void HandleMovement()
    {
        Vector3 targetPos = transform.position;

#if UNITY_EDITOR || UNITY_STANDALONE
        // PC: follow mouse
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            targetPos = mainCamera.ScreenToWorldPoint(mousePos);
            targetPos.z = 0f; // Keep player on Z=0 plane
        }
#elif UNITY_IOS || UNITY_ANDROID
        // Mobile: follow first touch
        if (Input.touchCount > 0)
        {
            Vector3 touchPos = Input.GetTouch(0).position;
            touchPos.z = Mathf.Abs(mainCamera.transform.position.z);
            targetPos = mainCamera.ScreenToWorldPoint(touchPos);
            targetPos.z = 0f;
        }
#endif

        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }
    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            float attackSpeed = Mathf.Max(playerStats.GetStatValue(EnumStat.attackSpeed), 0.01f);
            yield return new WaitForSeconds(1f / attackSpeed);
            
            if (!isDead)
                Attack();
        }
    }

    private IEnumerator HpLossRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            if (!isDead && playerStats != null)
            {
                int hpLoss = playerStats.GetStatValue(EnumStat.hpLossPerSecond);
                if (hpLoss > 0)
                {
                    TakeDamage(hpLoss);
                }
            }
        }
    }

    private void Attack()
    {
        // Detect all 2D colliders inside the box
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0f, enemyLayer);

        int damage = playerStats.GetStatValue(EnumStat.damage);
        int baseReflection = playerStats.GetStatValue(EnumStat.baseReflection);
        float totalMultiplier = 0;

        foreach (var hit in hits)
        {
            // Apply damage if enemy has Enemy script
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                
                // Get multiplier from each enemy
                totalMultiplier += enemy.GetEnemyMultiplierBaseReflection();
            }
        }

        // Calculate reflection damage: baseReflection * totalMultiplier
        float reflectionDamage = baseReflection * totalMultiplier;
        if (reflectionDamage > 0)
        {
            TakeDamage((int)reflectionDamage);
        }

        StartCoroutine(FlashAlpha());
    }
    public void TakeDamage(int damage)
    {
        // Apply armor reduction
        int armor = playerStats.GetStatValue(EnumStat.armor);
        int finalDamage = Mathf.Max(1, damage - armor); // Minimum 1 damage
        
        currentHp -= finalDamage;
        currentHp = Mathf.Max(0, currentHp); // Prevent negative HP
        
        if (currentHp <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        isDead = true;
        
        // Show death UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        // Move player out of scene
        transform.position = new Vector3(100f, 0f, 0f);
        
        Debug.Log("Player died!");
    }

    private IEnumerator FlashAlpha()
    {
        if (rend == null) yield break;

        Color original = rend.color;

        // Set alpha to 0.4 (keep original color)
        Color flashColor = new Color(original.r, original.g, original.b, 0.4f);
        rend.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        // Reset to original alpha
        rend.color = original;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw attack range (box)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Draw currency pickup range (circle)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currencyPickupRadius);
    }
#endif
}
