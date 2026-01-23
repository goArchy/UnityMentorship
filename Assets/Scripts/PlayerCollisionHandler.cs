using UnityEngine;

/// <summary>
/// Handles player collisions with monsters and spheres, causing life loss.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("Invincibility duration after taking damage (in seconds)")]
    public float invincibilityDuration = 1f;
    
    [Tooltip("Tag to identify monsters (optional, will check name if not set)")]
    public string monsterTag = "Monster";
    
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    
    void Update()
    {
        // Update invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    /// <summary>
    /// Checks if the collided object is a monster or sphere and handles damage.
    /// </summary>
    private void HandleCollision(GameObject other)
    {
        // Skip if invincible
        if (isInvincible)
        {
            return;
        }
        
        // Check if it's a monster (by name or tag)
        bool isMonster = other.name.Contains("Monster") || 
                        (!string.IsNullOrEmpty(monsterTag) && other.CompareTag(monsterTag));
        
        // Check if it's a sphere (has RandomSphereMovement component)
        bool isSphere = other.GetComponent<RandomSphereMovement>() != null;
        
        // Only process monsters and spheres (not walls or other objects)
        if (isMonster || isSphere)
        {
            // Apply damage
            TakeDamage();
        }
    }
    
    /// <summary>
    /// Applies damage to the player, losing one life.
    /// </summary>
    private void TakeDamage()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
            
            // Start invincibility period
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            
            Debug.Log($"Player took damage! Lives remaining: {GameManager.Instance.GetLives()}");
        }
        else
        {
            Debug.LogWarning("PlayerCollisionHandler: GameManager instance not found!");
        }
    }
    
    /// <summary>
    /// Checks if the player is currently invincible.
    /// </summary>
    public bool IsInvincible()
    {
        return isInvincible;
    }
}
