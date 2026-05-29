using UnityEngine;

/// <summary>
/// When the player is inside a hole trigger and within fall radius of center,
/// starts the hole-fall sequence (fall, lose a life, pause, retry or game over).
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoleTrigger : MonoBehaviour
{
    [Tooltip("LevelLoader that reloads the level after a fall (assigned when the hole is created)")]
    public LevelLoader levelLoader;

    [Tooltip("XZ radius from hole center within which the player actually falls")]
    public float fallRadius = 0.3f;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null)
            c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        HandlePlayerInHole(other);
    }

    void OnTriggerStay(Collider other)
    {
        HandlePlayerInHole(other);
    }

    void HandlePlayerInHole(Collider other)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsHoleFallSequence())
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsLevelComplete())
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsDamagePaused())
            return;

        PlayerCollisionHandler handler = other.GetComponentInParent<PlayerCollisionHandler>();
        if (handler == null)
            handler = other.GetComponent<PlayerCollisionHandler>();
        if (handler == null)
            return;

        Rigidbody playerRb = handler.GetComponent<Rigidbody>();
        if (playerRb == null)
            playerRb = handler.GetComponentInParent<Rigidbody>();
        if (playerRb == null)
            return;

        float dx = playerRb.position.x - transform.position.x;
        float dz = playerRb.position.z - transform.position.z;
        if (dx * dx + dz * dz > fallRadius * fallRadius)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.BeginHoleFallSequence(levelLoader);
    }
}
