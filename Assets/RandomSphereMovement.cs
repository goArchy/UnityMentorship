using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSphereMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of the sphere movement")]
    public float moveSpeed = 3f;
    
    [Tooltip("How often the direction changes (in seconds)")]
    public float directionChangeInterval = 2f;
    
    [Header("Movement Bounds (Optional)")]
    [Tooltip("If enabled, sphere will stay within these bounds")]
    public bool useBounds = false;
    
    [Tooltip("Minimum X position")]
    public float minX = -10f;
    
    [Tooltip("Maximum X position")]
    public float maxX = 10f;
    
    [Tooltip("Minimum Z position")]
    public float minZ = -10f;
    
    [Tooltip("Maximum Z position")]
    public float maxZ = 10f;
    
    [Header("Spawning Settings")]
    [Tooltip("If enabled, sphere will spawn a new sphere on collision")]
    public bool canSpawnOnCollision = true;
    
    [Tooltip("Cooldown time before this sphere can spawn again (in seconds)")]
    public float spawnCooldown = 1f;
    
    [Tooltip("Offset distance for spawning the new sphere")]
    public float spawnOffset = 1.5f;
    
    [Tooltip("Maximum total number of spheres allowed in the scene")]
    public static int maxSphereCount = 20;
    
    // Static counter to track total spheres in scene
    private static int currentSphereCount = 0;
    
    private Vector3 currentDirection;
    private float timeSinceLastDirectionChange;
    private float timeSinceLastSpawn;
    private bool canSpawn = true;
    private Rigidbody rb;
    private float maxMoveStep = 0.1f; // Maximum movement step to prevent tunneling
    
    void Start()
    {
        // Ensure there's a Rigidbody for collision detection
        // (needed even for kinematic movement)
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        // Use continuous collision detection to properly detect collisions with static walls
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Set max move step based on sphere radius to prevent tunneling
        float radius = GetSphereRadius();
        maxMoveStep = radius * 0.5f; // Move in steps of half the radius
        
        // Increment the sphere count
        currentSphereCount++;
        
        // Initialize with a random direction
        ChangeDirection();
    }
    
    void OnDestroy()
    {
        // Decrement the sphere count when this sphere is destroyed
        currentSphereCount--;
        if (currentSphereCount < 0)
        {
            currentSphereCount = 0; // Safety check to prevent negative count
        }
    }
    
    void Update()
    {
        // Update timers in Update (frame-rate independent)
        timeSinceLastDirectionChange += Time.deltaTime;
        
        // Change direction at intervals
        if (timeSinceLastDirectionChange >= directionChangeInterval)
        {
            ChangeDirection();
            timeSinceLastDirectionChange = 0f;
        }
        
        // Update spawn cooldown
        if (!canSpawn)
        {
            timeSinceLastSpawn += Time.deltaTime;
            if (timeSinceLastSpawn >= spawnCooldown)
            {
                canSpawn = true;
            }
        }
    }
    
    void FixedUpdate()
    {
        // Movement and collision detection in FixedUpdate for consistent physics timing
        if (rb == null) return;
        
        // Calculate desired movement
        Vector3 desiredMovement = currentDirection * moveSpeed * Time.fixedDeltaTime;
        float desiredDistance = desiredMovement.magnitude;
        
        if (desiredDistance > 0f)
        {
            // Subdivide movement to prevent tunneling
            Vector3 finalPosition = MoveWithCollisionCheck(rb.position, desiredMovement, desiredDistance);
            
            // Apply bounds if enabled
            if (useBounds)
            {
                finalPosition.x = Mathf.Clamp(finalPosition.x, minX, maxX);
                finalPosition.z = Mathf.Clamp(finalPosition.z, minZ, maxZ);
                
                // If we hit a bound, change direction
                if (Mathf.Approximately(finalPosition.x, rb.position.x) || Mathf.Approximately(finalPosition.z, rb.position.z))
                {
                    ChangeDirection();
                    timeSinceLastDirectionChange = 0f;
                }
            }
            
            // Use Rigidbody.MovePosition for proper physics integration
            rb.MovePosition(finalPosition);
            
            // Post-movement validation to catch any penetration
            ValidatePosition();
        }
    }
    
    Vector3 MoveWithCollisionCheck(Vector3 startPosition, Vector3 movement, float totalDistance)
    {
        Vector3 currentPos = startPosition;
        Vector3 moveDirection = movement.normalized;
        float remainingDistance = totalDistance;
        
        // Subdivide movement into smaller steps
        while (remainingDistance > 0f)
        {
            float stepDistance = Mathf.Min(remainingDistance, maxMoveStep);
            Vector3 stepMovement = moveDirection * stepDistance;
            
            // Check for collision along this step
            float safeDistance = CheckWallCollision(currentPos, moveDirection, stepDistance);
            
            if (safeDistance < stepDistance)
            {
                // Hit a wall, move only to collision point
                currentPos += moveDirection * safeDistance;
                // Change direction
                ChangeDirection();
                timeSinceLastDirectionChange = 0f;
                break; // Stop moving this frame
            }
            else
            {
                // No collision, move the full step
                currentPos += stepMovement;
                remainingDistance -= stepDistance;
            }
        }
        
        return currentPos;
    }
    
    float CheckWallCollision(Vector3 fromPosition, Vector3 direction, float maxDistance)
    {
        // Get the actual radius of the sphere collider accounting for scale
        float radius = GetSphereRadius();
        
        // Cast from slightly behind current position to catch edge cases
        Vector3 castOrigin = fromPosition - direction * (radius * 0.1f);
        
        // SphereCast sweeps a sphere along the ray
        // hit.distance is the distance along the ray to where the sphere first touches
        // Since we're casting from the center, hit.distance tells us how far the center can move
        RaycastHit hit;
        float castDistance = maxDistance + radius * 0.1f; // Add buffer for the offset origin
        
        if (Physics.SphereCast(castOrigin, radius, direction, out hit, castDistance))
        {
            // Check if we hit a wall
            if (hit.collider.gameObject.name.Contains("Wall") || hit.collider.CompareTag("Wall"))
            {
                // Adjust distance for the offset origin
                float adjustedDistance = hit.distance - radius * 0.1f;
                
                // hit.distance is how far the center can move before sphere touches wall
                // Subtract a small buffer (0.01f) to prevent floating point penetration
                float safeDistance = Mathf.Max(0f, adjustedDistance - 0.01f);
                
                // If we're already very close or touching, don't move
                if (safeDistance < 0.01f)
                {
                    return 0f;
                }
                
                return safeDistance;
            }
        }
        
        // Additional check: cast at slight angles to catch edge cases
        // Check slightly to the left and right of movement direction
        Vector3[] checkDirections = new Vector3[]
        {
            direction,
            Quaternion.Euler(0, 5f, 0) * direction,
            Quaternion.Euler(0, -5f, 0) * direction
        };
        
        float minDistance = maxDistance;
        foreach (Vector3 checkDir in checkDirections)
        {
            if (Physics.SphereCast(castOrigin, radius, checkDir, out hit, castDistance))
            {
                if (hit.collider.gameObject.name.Contains("Wall") || hit.collider.CompareTag("Wall"))
                {
                    float adjustedDistance = hit.distance - radius * 0.1f;
                    float safeDistance = Mathf.Max(0f, adjustedDistance - 0.01f);
                    if (safeDistance < minDistance)
                    {
                        minDistance = safeDistance;
                    }
                }
            }
        }
        
        // Return the minimum safe distance found
        return minDistance;
    }
    
    float GetSphereRadius()
    {
        // Get the radius of the sphere collider accounting for scale
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            // For a sphere, the radius is uniform, so we use the max scale component
            float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            return sphereCollider.radius * maxScale;
        }
        
        // Fallback: default sphere primitive has radius 0.5
        float defaultRadius = 0.5f;
        float fallbackScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        return defaultRadius * fallbackScale;
    }
    
    void ValidatePosition()
    {
        // Check if the sphere is overlapping with any walls after movement
        float radius = GetSphereRadius();
        Collider[] overlappingColliders = Physics.OverlapSphere(rb.position, radius);
        
        foreach (Collider col in overlappingColliders)
        {
            // Check if we're overlapping with a wall
            if (col.gameObject.name.Contains("Wall") || col.gameObject.CompareTag("Wall"))
            {
                // Calculate push-back direction and distance
                Vector3 directionToWall = (rb.position - col.bounds.center);
                directionToWall.y = 0f; // Keep it on the horizontal plane
                
                if (directionToWall.sqrMagnitude > 0.01f)
                {
                    directionToWall.Normalize();
                    
                    // Calculate how much we need to push back
                    // Get the closest point on the wall's bounds to our position
                    Vector3 closestPoint = col.bounds.ClosestPoint(rb.position);
                    Vector3 pushDirection = (rb.position - closestPoint);
                    pushDirection.y = 0f;
                    
                    if (pushDirection.sqrMagnitude > 0.01f)
                    {
                        pushDirection.Normalize();
                        
                        // Calculate penetration depth
                        float penetrationDepth = radius - Vector3.Distance(rb.position, closestPoint);
                        
                        if (penetrationDepth > 0f)
                        {
                            // Push back to resolve penetration
                            Vector3 correctedPosition = rb.position + pushDirection * (penetrationDepth + 0.01f);
                            rb.MovePosition(correctedPosition);
                            
                            // Change direction to prevent getting stuck
                            ChangeDirection();
                            timeSinceLastDirectionChange = 0f;
                        }
                    }
                }
                break; // Only handle one wall at a time
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if we hit a wall
        if (collision.gameObject.name.Contains("Wall") || collision.gameObject.CompareTag("Wall"))
        {
            // Push the sphere back to prevent penetration
            CorrectPenetration(collision);
            
            // Change direction when colliding with a wall
            ChangeDirection();
            timeSinceLastDirectionChange = 0f;
        }
        
        if (canSpawnOnCollision && canSpawn && currentSphereCount < maxSphereCount)
        {
            SpawnNewSphere(collision);
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        // Keep pushing back if still in contact with a wall
        if (collision.gameObject.name.Contains("Wall") || collision.gameObject.CompareTag("Wall"))
        {
            // Continuously correct penetration until resolved
            CorrectPenetration(collision);
        }
    }
    
    void CorrectPenetration(Collision collision)
    {
        if (rb == null || collision.contacts.Length == 0) return;
        
        float radius = GetSphereRadius();
        Vector3 totalPush = Vector3.zero;
        int validContacts = 0;
        
        // Process all contact points for more accurate correction
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 pushDirection = contact.normal;
            float penetration = contact.separation;
            
            if (penetration < 0) // Negative means penetration
            {
                // Calculate push-back needed
                float pushDistance = Mathf.Abs(penetration) + 0.01f; // Add small buffer
                totalPush += pushDirection * pushDistance;
                validContacts++;
            }
        }
        
        // Average the push direction if multiple contacts
        if (validContacts > 0)
        {
            totalPush /= validContacts;
            
            // Apply correction using Rigidbody for proper physics
            Vector3 correctedPosition = rb.position + totalPush;
            rb.MovePosition(correctedPosition);
        }
        else
        {
            // Fallback: use bounds-based correction if separation data is unreliable
            Bounds wallBounds = collision.collider.bounds;
            Vector3 closestPoint = wallBounds.ClosestPoint(rb.position);
            Vector3 pushDirection = (rb.position - closestPoint);
            pushDirection.y = 0f;
            
            if (pushDirection.sqrMagnitude > 0.01f)
            {
                pushDirection.Normalize();
                float distanceToWall = Vector3.Distance(rb.position, closestPoint);
                float penetrationDepth = radius - distanceToWall;
                
                if (penetrationDepth > 0f)
                {
                    Vector3 correctedPosition = rb.position + pushDirection * (penetrationDepth + 0.01f);
                    rb.MovePosition(correctedPosition);
                }
            }
        }
    }
    
    void SpawnNewSphere(Collision collision)
    {
        // Calculate spawn position (offset from current position)
        Vector3 spawnPosition = transform.position + (collision.contacts[0].normal * spawnOffset);
        
        // Create a new sphere GameObject
        GameObject newSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newSphere.transform.position = spawnPosition;
        newSphere.transform.localScale = transform.localScale;
        
        // Copy the material if this sphere has one
        Renderer thisRenderer = GetComponent<Renderer>();
        Renderer newRenderer = newSphere.GetComponent<Renderer>();
        if (thisRenderer != null && newRenderer != null && thisRenderer.material != null)
        {
            newRenderer.material = thisRenderer.material;
        }
        
        // Add Rigidbody if it doesn't exist (needed for collisions)
        if (newSphere.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = newSphere.AddComponent<Rigidbody>();
            // Copy rigidbody settings if this object has one
            Rigidbody thisRb = GetComponent<Rigidbody>();
            if (thisRb != null)
            {
                rb.mass = thisRb.mass;
                rb.drag = thisRb.drag;
                rb.angularDrag = thisRb.angularDrag;
                rb.useGravity = thisRb.useGravity;
                rb.isKinematic = thisRb.isKinematic;
            }
            else
            {
                // Default settings for kinematic movement
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
        
        // Add the RandomSphereMovement script to the new sphere
        RandomSphereMovement newMovement = newSphere.AddComponent<RandomSphereMovement>();
        
        // Copy all the movement settings
        newMovement.moveSpeed = this.moveSpeed;
        newMovement.directionChangeInterval = this.directionChangeInterval;
        newMovement.useBounds = this.useBounds;
        newMovement.minX = this.minX;
        newMovement.maxX = this.maxX;
        newMovement.minZ = this.minZ;
        newMovement.maxZ = this.maxZ;
        newMovement.canSpawnOnCollision = this.canSpawnOnCollision;
        newMovement.spawnCooldown = this.spawnCooldown;
        newMovement.spawnOffset = this.spawnOffset;
        
        // Note: The new sphere's Start() will increment currentSphereCount automatically
        
        // Reset spawn cooldown
        canSpawn = false;
        timeSinceLastSpawn = 0f;
    }
    
    // Static method to get current sphere count (useful for UI or debugging)
    public static int GetCurrentSphereCount()
    {
        return currentSphereCount;
    }
    
    // Static method to reset the counter (useful when starting a new game/level)
    public static void ResetSphereCount()
    {
        currentSphereCount = 0;
    }
    
    void ChangeDirection()
    {
        // Generate a random direction in 2D space (X and Z plane)
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        currentDirection = new Vector3(Mathf.Cos(randomAngle), 0f, Mathf.Sin(randomAngle)).normalized;
    }
    
    // Optional: Call this to immediately change direction (useful for external triggers)
    public void SetRandomDirection()
    {
        ChangeDirection();
        timeSinceLastDirectionChange = 0f;
    }
}

