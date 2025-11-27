using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    [Header("Data")]
    public SOEnemyData enemyData;

    [Header("Movement")]
    public float moveSpeed = 2f;
    
    [HideInInspector] public ObjectPool pool;
    [HideInInspector] public EnemySpawner spawner;
    [HideInInspector] public StoreCurrencyReference currencyReference;

    public int currentHealth { get; private set; }
    private Camera mainCamera;
    private bool justSpawned = true;
    private float spawnIgnoreTime = 3f;
    private Vector2 moveDirection;

    private void OnEnable()
    {
        mainCamera = Camera.main;
        InitializeEnemy();
        justSpawned = true;

        // Pick a random target point inside target area and calculate movement direction
        if (spawner != null)
        {
            Vector2 targetPoint = spawner.GetRandomPointInsideTargetArea();
            moveDirection = (targetPoint - (Vector2)transform.position).normalized;
        }
        else
        {
            // Fallback: random direction
            moveDirection = Random.insideUnitCircle.normalized;
        }

        Invoke(nameof(DisableJustSpawned), spawnIgnoreTime);
    }

    private void OnDisable()
    {
        ReturnToPool();
    }

    private void Update()
    {
        MoveStraight();
        CheckOffscreen();
    }

    private void InitializeEnemy()
    {
        if (enemyData != null)
            currentHealth = enemyData.hp;
    }

    private void MoveStraight()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void CheckOffscreen()
    {
        if (justSpawned) return;

        if (!IsVisibleToCamera())
        {
            ReturnToPool();
        }
    }

    private void DisableJustSpawned() => justSpawned = false;

    private bool IsVisibleToCamera()
    {
        if (mainCamera == null) return true;
        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        float buffer = 0.1f;
        return (viewPos.x >= -buffer && viewPos.x <= 1 + buffer &&
                viewPos.y >= -buffer && viewPos.y <= 1 + buffer);
    }

    private void ReturnToPool()
    {
        pool?.ReturnToPool(gameObject);
        spawner?.RemoveEnemyFromActiveList(this);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    public float GetEnemyMultiplierBaseReflection()
    {
        // Return 1 as multiplier per enemy
        return enemyData.baseReflectionMultiplier;
    }

    private void Die()
    {
        SpawnCurrency();
        ReturnToPool();
    }

    private void SpawnCurrency()
    {
        if (enemyData == null || currencyReference == null) return;

        // Get the correct currency pool from StoreCurrencyReference
        ObjectPool selectedPool = currencyReference.GetCurrencyPool(enemyData.currencyType);
        
        if (selectedPool == null) return;

        int amount = enemyData.baseCurrencyAmount;
        
        for (int i = 0; i < amount; i++)
        {
            // Spawn currency at enemy position with slight random offset
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
            GameObject currencyObj = selectedPool.Get(spawnPos, Quaternion.identity);
            
            // Set the pool reference so it can return to pool
            CurrencyControl currencyControl = currencyObj.GetComponent<CurrencyControl>();
            if (currencyControl != null)
            {
                currencyControl.pool = selectedPool;
            }
        }
    }
}
