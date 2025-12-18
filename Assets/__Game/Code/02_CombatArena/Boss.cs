using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Boss : Enemy
{
    public bool isDefeated = false;
    
    protected override void OnEnable()
    {
        InitializeEnemy();
    }
    
    protected override void OnDisable()
    {
        // Don't call base.OnDisable() to prevent returning to pool
    }
    
    // Set target position (uses base Enemy's targetPosition)
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }
    
    // Boss inherits movement from Enemy base class
    
    protected virtual void Die()
    {
        // Spawn currency
        SpawnCurrency();
        
        // Mark as defeated
        isDefeated = true;
        
        // Hide boss
        gameObject.SetActive(false);
        
        Debug.Log("Boss defeated!");
    }
}
