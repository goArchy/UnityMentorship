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
    /// Checks if the collided object is a weapon, monster, or sphere and handles accordingly.
    /// </summary>
    private void HandleCollision(GameObject other)
    {
        // Skip all collisions if the level is complete (game is paused)
        if (GameManager.Instance != null && GameManager.Instance.IsLevelComplete())
        {
            return;
        }
        
        // Check if it's a weapon (by name on self or any parent) — weapon pickup is not affected by invincibility
        GameObject weaponRoot = FindWeaponRoot(other);
        
        if (weaponRoot != null)
        {
            HandleWeaponPickup(weaponRoot);
            return;
        }
        
        // Skip if invincible
        if (isInvincible)
        {
            return;
        }
        
        // Check if it's a monster (by name or tag, on self or parent)
        bool isMonster = other.name.Contains("Monster") || 
                        (!string.IsNullOrEmpty(monsterTag) && other.CompareTag(monsterTag));
        
        // Check if it's a sphere (has RandomSphereMovement component)
        bool isSphere = other.GetComponent<RandomSphereMovement>() != null;
        
        // Only process monsters and spheres (not walls or other objects)
        if (isMonster || isSphere)
        {
            HandleMonsterCollision(other);
        }
    }
    
    /// <summary>
    /// Walks up the transform hierarchy to find a parent whose name contains "Weapon".
    /// Returns that GameObject, or null if none found.
    /// </summary>
    private GameObject FindWeaponRoot(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.name.Contains("Weapon"))
            {
                return current.gameObject;
            }
            current = current.parent;
        }
        return null;
    }
    
    /// <summary>
    /// Handles picking up a weapon: destroy the entire weapon object and set hasWeapon to true.
    /// </summary>
    private void HandleWeaponPickup(GameObject weaponObject)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PickUpWeapon();
        }
        
        // Destroy the entire weapon object (root) so it fully disappears from the scene
        Destroy(weaponObject);
        
        Debug.Log("Weapon picked up!");
    }
    
    /// <summary>
    /// Handles collision with a monster. If armed, completes the level. If unarmed, takes damage.
    /// </summary>
    private void HandleMonsterCollision(GameObject monsterObject)
    {
        if (GameManager.Instance != null && GameManager.Instance.HasWeapon())
        {
            // Player has weapon — defeat the monster and complete the level
            Destroy(monsterObject);
            Debug.Log("Monster defeated with weapon! Level complete!");
            GameManager.Instance.CompleteLevel();
        }
        else
        {
            // Player has no weapon — take damage
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

    /// <summary>
    /// Starts invincibility after falling in a hole (life already deducted by HoleTrigger).
    /// </summary>
    public void BeginInvincibilityAfterHole()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }
}
