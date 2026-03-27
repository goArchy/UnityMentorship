using UnityEngine;

/// <summary>
/// Each frame checks whether any monster is within detectionRadius using Physics.OverlapSphere.
/// Collision callbacks cannot be used because both the monster and kitty have kinematic Rigidbodies,
/// and Unity does not fire OnCollisionEnter between two kinematic bodies.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KittyCollisionHandler : MonoBehaviour
{
    [Tooltip("Radius to scan for nearby monsters each frame")]
    public float detectionRadius = 0.7f;

    [Tooltip("Minimum seconds between life loss from this kitty")]
    public float damageCooldown = 1f;

    float lastPenaltyTime = -999f;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsLevelComplete())
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
            return;

        if (Time.time - lastPenaltyTime < damageCooldown)
            return;

        PlayerCollisionHandler playerHandler = FindObjectOfType<PlayerCollisionHandler>();
        if (playerHandler != null && playerHandler.IsInvincible())
            return;

        Collider[] nearby = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject)
                continue;

            if (IsMonster(col.gameObject))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoseLife();
                    lastPenaltyTime = Time.time;
                    Debug.Log("Monster is near the kitty! Player lost a life.");
                }
                break;
            }
        }
    }

    static bool IsMonster(GameObject go)
    {
        Transform t = go.transform;
        while (t != null)
        {
            if (t.name.Contains("Monster"))
                return true;
            if (t.GetComponent<RandomSphereMovement>() != null)
                return true;
            t = t.parent;
        }

        return false;
    }
}
