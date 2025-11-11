using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public SOEnemyData enemyData;

    [Header("Movement Settings")]
    public Vector2 moveDirection = Vector2.left;  // Set direction in inspector
    public float moveSpeed = 2f;                  // Units per second

    [SerializeField] private int currentHealth;

    private void Start()
    {
        // Initialize current health from the ScriptableObject
        currentHealth = enemyData.hp;
    }

    private void Update()
    {
        // Move enemy in one direction
        transform.position += (Vector3)(moveDirection.normalized * moveSpeed * Time.deltaTime);

        // Check if enemy is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    private void Die()
    {
        // Optional: play death effect here
        Destroy(gameObject);
    }
}
