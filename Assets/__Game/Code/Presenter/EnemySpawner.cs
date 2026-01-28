using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemySpawnEntry
{
    public ObjectPool enemyPool;
    public SOEnemyData enemyData;
}

[System.Serializable]
public class LevelSpawnConfig
{
    public int level;
    public List<EnemySpawnEntry> enemiesForThisLevel;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;        // For spawn rate
    public BoxCollider2D targetArea;       // Area enemies move toward
    public StoreCurrencyReference currencyReference;  // Currency pools reference

    [Header("Level Configurations")]
    public List<LevelSpawnConfig> levelConfigs;

    [Header("Spawn Settings")]
    public float spawnOffsetX = 2f;  // Horizontal distance outside camera to spawn (left/right sides)
    public float spawnOffsetY = 2f;  // Vertical distance outside camera to spawn (top/bottom sides)
    
    [Header("Debug Visualization")]
    public bool showSpawnGizmos = true;     // Show spawn zones in Scene view
    public Color gizmoColor = Color.cyan;   // Color for spawn zone gizmos

    private Camera mainCamera;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private List<EnemySpawnEntry> currentLevelEnemies = new List<EnemySpawnEntry>();
    private Dictionary<SOEnemyData, float> spawnTimers = new Dictionary<SOEnemyData, float>();  // Per-enemy-type timers

    private void Awake()
    {
        mainCamera = Camera.main;
        MizuLog.Spawn($"EnemySpawner Awake - levelConfigs count: {levelConfigs?.Count ?? 0}");
    }

    private void OnEnable()
    {
        spawnTimers.Clear();  // Reset all timers on start
        activeEnemies.Clear();  // Clear active enemies list
        LoadCurrentLevelEnemies();
    }

    private void LoadCurrentLevelEnemies()
    {
        currentLevelEnemies.Clear();

        if (Stage.Instance == null)
        {
            Debug.LogWarning("Stage.Instance is null");
            return;
        }

        int currentStage = Stage.Instance.GetStage();
        MizuLog.Spawn($"Loading enemies for stage {currentStage}. Total configs: {levelConfigs?.Count ?? 0}");

        // Find the config for current stage
        if (levelConfigs != null)
        {
            foreach (var config in levelConfigs)
            {
                MizuLog.Spawn($"Checking config for stage {config.level}, enemies in config: {config.enemiesForThisLevel?.Count ?? 0}");
                if (config.level == currentStage)
                {
                    if (config.enemiesForThisLevel != null && config.enemiesForThisLevel.Count > 0)
                    {
                        // Copy the list to avoid reference issues
                        currentLevelEnemies = new List<EnemySpawnEntry>(config.enemiesForThisLevel);
                        MizuLog.Spawn($"✓ Loaded {currentLevelEnemies.Count} enemy types for stage {currentStage}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠ Stage {currentStage} config exists but enemiesForThisLevel list is empty or null!");
                    }
                    return;
                }
            }
        }

        Debug.LogWarning($"⚠ No spawn configuration found for stage {currentStage}");
    }

    private void Update()
    {
        foreach (var entry in currentLevelEnemies)
        {
            if (Time.timeSinceLevelLoad < entry.enemyData.startTime) continue;

            // Initialize timer for this enemy type if not exists
            if (!spawnTimers.ContainsKey(entry.enemyData))
                spawnTimers[entry.enemyData] = 0f;
            
            // Increment this enemy type's timer
            spawnTimers[entry.enemyData] += Time.deltaTime;
            
            // Calculate adjusted interval based on spawn rate percent
            float adjustedInterval = entry.enemyData.spawnInterval / Mathf.Max(0.01f, playerStats.GetStatValue(EnumStat.spawnRatePercent) / 100f);
            
            // Only spawn when timer exceeds interval, then RESET this timer
            if (spawnTimers[entry.enemyData] >= adjustedInterval)
            {
                SpawnEnemyType(entry);
                spawnTimers[entry.enemyData] = 0f;  // Reset THIS enemy type's timer
            }
        }
    }

    private void SpawnEnemyType(EnemySpawnEntry entry)
    {
        // Clean null or inactive references
        activeEnemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

        // Count current active enemies of this type
        int activeCount = activeEnemies.FindAll(e => e.enemyData == entry.enemyData).Count;

        // Calculate how many can actually spawn (respect maxCapacity)
        int canSpawn = entry.enemyData.spawnAmount;
        if (entry.enemyData.maxCapacity > 0)
            canSpawn = Mathf.Min(entry.enemyData.spawnAmount, entry.enemyData.maxCapacity - activeCount);

        if (canSpawn <= 0) return;

        for (int i = 0; i < canSpawn; i++)
        {
            Vector2 spawnPos = GetRandomPositionOutsideCamera();
            GameObject enemyObj = entry.enemyPool.Get(spawnPos, Quaternion.identity);
            Enemy enemyScript = enemyObj.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                enemyScript.pool = entry.enemyPool;
                enemyScript.spawner = this;
                enemyScript.currencyReference = currencyReference;
                enemyScript.playerStats = playerStats;
                enemyScript.targetPosition = GetRandomPointInsideTargetArea();
                activeEnemies.Add(enemyScript);
            }
        }
    }

    private Vector2 GetRandomPositionOutsideCamera()
    {
        Vector2 camPos = mainCamera.transform.position;
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;

        int side = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        switch (side)
        {
            case 0: // top
                spawnPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2),
                                       camPos.y + camHeight / 2 + spawnOffsetY);
                break;
            case 1: // bottom
                spawnPos = new Vector2(Random.Range(camPos.x - camWidth / 2, camPos.x + camWidth / 2),
                                       camPos.y - camHeight / 2 - spawnOffsetY);
                break;
            case 2: // left
                spawnPos = new Vector2(camPos.x - camWidth / 2 - spawnOffsetX,
                                       Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                break;
            case 3: // right
                spawnPos = new Vector2(camPos.x + camWidth / 2 + spawnOffsetX,
                                       Random.Range(camPos.y - camHeight / 2, camPos.y + camHeight / 2));
                break;
        }

        return spawnPos;
    }

    // Returns a random point inside the target area
    public Vector2 GetRandomPointInsideTargetArea()
    {
        Bounds bounds = targetArea.bounds;
        return new Vector2(Random.Range(bounds.min.x, bounds.max.x),
                           Random.Range(bounds.min.y, bounds.max.y));
    }

    // Called by Enemy when it dies or returns to pool
    public void RemoveEnemyFromActiveList(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
    }
    
    // Called when splitting enemies to add children to active list
    public void AddEnemyToActiveList(Enemy enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showSpawnGizmos || mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        
        Vector2 camPos = mainCamera.transform.position;
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        float offsetX = spawnOffsetX;
        float offsetY = spawnOffsetY;
        
        // Draw camera border in white
        Gizmos.color = Color.white;
        Vector3 topLeft = new Vector3(camPos.x - camWidth / 2, camPos.y + camHeight / 2, 0);
        Vector3 topRight = new Vector3(camPos.x + camWidth / 2, camPos.y + camHeight / 2, 0);
        Vector3 bottomLeft = new Vector3(camPos.x - camWidth / 2, camPos.y - camHeight / 2, 0);
        Vector3 bottomRight = new Vector3(camPos.x + camWidth / 2, camPos.y - camHeight / 2, 0);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        // Draw spawn zones with custom color
        Gizmos.color = gizmoColor;
        
        // Top spawn zone (uses Y offset)
        Vector3 topSpawnTL = new Vector3(camPos.x - camWidth / 2, camPos.y + camHeight / 2 + offsetY, 0);
        Vector3 topSpawnTR = new Vector3(camPos.x + camWidth / 2, camPos.y + camHeight / 2 + offsetY, 0);
        Vector3 topSpawnBL = new Vector3(camPos.x - camWidth / 2, camPos.y + camHeight / 2, 0);
        Vector3 topSpawnBR = new Vector3(camPos.x + camWidth / 2, camPos.y + camHeight / 2, 0);
        Gizmos.DrawLine(topSpawnTL, topSpawnTR);
        Gizmos.DrawLine(topSpawnTR, topSpawnBR);
        Gizmos.DrawLine(topSpawnBR, topSpawnBL);
        Gizmos.DrawLine(topSpawnBL, topSpawnTL);
        
        // Bottom spawn zone (uses Y offset)
        Vector3 bottomSpawnTL = new Vector3(camPos.x - camWidth / 2, camPos.y - camHeight / 2, 0);
        Vector3 bottomSpawnTR = new Vector3(camPos.x + camWidth / 2, camPos.y - camHeight / 2, 0);
        Vector3 bottomSpawnBL = new Vector3(camPos.x - camWidth / 2, camPos.y - camHeight / 2 - offsetY, 0);
        Vector3 bottomSpawnBR = new Vector3(camPos.x + camWidth / 2, camPos.y - camHeight / 2 - offsetY, 0);
        Gizmos.DrawLine(bottomSpawnTL, bottomSpawnTR);
        Gizmos.DrawLine(bottomSpawnTR, bottomSpawnBR);
        Gizmos.DrawLine(bottomSpawnBR, bottomSpawnBL);
        Gizmos.DrawLine(bottomSpawnBL, bottomSpawnTL);
        
        // Left spawn zone (uses X offset)
        Vector3 leftSpawnTL = new Vector3(camPos.x - camWidth / 2 - offsetX, camPos.y + camHeight / 2, 0);
        Vector3 leftSpawnTR = new Vector3(camPos.x - camWidth / 2, camPos.y + camHeight / 2, 0);
        Vector3 leftSpawnBL = new Vector3(camPos.x - camWidth / 2 - offsetX, camPos.y - camHeight / 2, 0);
        Vector3 leftSpawnBR = new Vector3(camPos.x - camWidth / 2, camPos.y - camHeight / 2, 0);
        Gizmos.DrawLine(leftSpawnTL, leftSpawnTR);
        Gizmos.DrawLine(leftSpawnTR, leftSpawnBR);
        Gizmos.DrawLine(leftSpawnBR, leftSpawnBL);
        Gizmos.DrawLine(leftSpawnBL, leftSpawnTL);
        
        // Right spawn zone (uses X offset)
        Vector3 rightSpawnTL = new Vector3(camPos.x + camWidth / 2, camPos.y + camHeight / 2, 0);
        Vector3 rightSpawnTR = new Vector3(camPos.x + camWidth / 2 + offsetX, camPos.y + camHeight / 2, 0);
        Vector3 rightSpawnBL = new Vector3(camPos.x + camWidth / 2, camPos.y - camHeight / 2, 0);
        Vector3 rightSpawnBR = new Vector3(camPos.x + camWidth / 2 + offsetX, camPos.y - camHeight / 2, 0);
        Gizmos.DrawLine(rightSpawnTL, rightSpawnTR);
        Gizmos.DrawLine(rightSpawnTR, rightSpawnBR);
        Gizmos.DrawLine(rightSpawnBR, rightSpawnBL);
        Gizmos.DrawLine(rightSpawnBL, rightSpawnTL);
    }
}
