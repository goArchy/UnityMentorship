using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("The level index to load (0-based)")]
    public int levelIndex = 0;
    
    [Tooltip("Size of each grid cell in world units")]
    public float cellSize = 1f;
    
    [Header("Wall Settings")]
    [Tooltip("Use cubes for walls (true) or spheres (false)")]
    public bool useCubes = true;
    
    [Tooltip("Scale of wall objects")]
    public Vector3 wallScale = Vector3.one;
    
    [Tooltip("Material for walls (optional)")]
    public Material wallMaterial;
    
    [Tooltip("Parent object for all walls (optional)")]
    public Transform wallsParent;
    
    [Header("Monster Settings")]
    [Tooltip("Monster prefab to instantiate (e.g. FootmanHP). Falls back to sphere if not assigned.")]
    public GameObject monsterPrefab;
    
    [Tooltip("Scale of monster")]
    public Vector3 monsterScale = Vector3.one;
    
    [Tooltip("Movement speed of monsters")]
    public float monsterMoveSpeed = 3f;
    
    [Tooltip("How often monsters change direction (in seconds)")]
    public float monsterDirectionChangeInterval = 2f;
    
    [Tooltip("Material for monsters (optional)")]
    public Material monsterMaterial;
    
    [Tooltip("Parent object for all monsters (optional)")]
    public Transform monstersParent;
    
    [Header("Weapon Settings")]
    [Tooltip("Weapon prefab or GameObject to instantiate (drag Weapon from scene or use prefab)")]
    public GameObject weaponPrefab;
    
    [Tooltip("Scale of weapon objects")]
    public Vector3 weaponScale = Vector3.one;
    
    [Tooltip("Parent object for all weapons (optional)")]
    public Transform weaponsParent;
    
    [Header("Player Settings")]
    [Tooltip("Player prefab to instantiate (e.g. RPGHeroHP). Falls back to cube if not assigned.")]
    public GameObject playerPrefab;
    
    [Tooltip("Scale of player")]
    public Vector3 playerScale = Vector3.one;
    
    [Tooltip("Movement speed of the player")]
    public float playerMoveSpeed = 6f;
    
    [Tooltip("Jump force of the player")]
    public float playerJumpForce = 5f;
    
    [Header("Hole Settings")]
    [Tooltip("Scale of hole visuals and triggers (per axis, multiplied by cell size on X/Z)")]
    public Vector3 holeScale = Vector3.one;
    
    [Tooltip("Material for hole quads (optional)")]
    public Material holeMaterial;
    
    [Tooltip("Parent object for all holes (optional)")]
    public Transform holesParent;
    
    [Header("Kitty Settings")]
    [Tooltip("Scale of kitty spheres")]
    public Vector3 kittyScale = Vector3.one;
    
    [Tooltip("Material for kitties (optional)")]
    public Material kittyMaterial;
    
    [Tooltip("Parent object for all kitties (optional)")]
    public Transform kittiesParent;
    
    [Header("Ground Plane Settings")]
    [Tooltip("The ground plane Transform to resize per level (drag the Plane from scene)")]
    public Transform groundPlane;
    
    [Tooltip("Extra margin around the level grid for the ground plane and boundaries")]
    public float groundMargin = 1f;
    
    [Tooltip("Height of the invisible boundary walls")]
    public float boundaryWallHeight = 4f;
    
    private List<GameObject> wallObjects = new List<GameObject>();
    private List<GameObject> monsterObjects = new List<GameObject>();
    private List<GameObject> weaponObjects = new List<GameObject>();
    private List<GameObject> boundaryObjects = new List<GameObject>();
    private List<GameObject> holeObjects = new List<GameObject>();
    private List<GameObject> kittyObjects = new List<GameObject>();
    private GameObject playerInstance;
    private Vector3 playerSpawnPosition;
    
    void Start()
    {
        LoadLevel(levelIndex);
        
        // Sync with GameManager if it exists
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLevel(levelIndex);
            
            // Subscribe to proceed event to load next level after player presses a key
            GameManager.Instance.OnProceedToNextLevel += OnProceedToNextLevel;
        }
    }
    
    public void LoadLevel(int index)
    {
        // Clear existing walls, monsters, weapons, boundaries, and player
        ClearWalls();
        ClearMonsters();
        ClearWeapons();
        ClearBoundaries();
        ClearHoles();
        ClearKitties();
        ClearPlayer();
        
        // Validate level index
        if (index < 0 || index >= Levels.All.Count)
        {
            Debug.LogWarning($"Level index {index} is out of range. Available levels: 0-{Levels.All.Count - 1}");
            return;
        }
        
        // Update level index
        levelIndex = index;
        
        // Sync with GameManager if it exists
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLevel(levelIndex);
            
            // Reset weapon state at the start of each level attempt
            GameManager.Instance.ResetWeapon();
        }
        
        string[] levelData = Levels.All[index];
        
        // Calculate offset to center the level
        int rows = levelData.Length;
        int cols = levelData[0].Length;
        float offsetX = -(cols * cellSize) / 2f + cellSize / 2f;
        float offsetZ = (rows * cellSize) / 2f - cellSize / 2f;
        
        // Calculate level bounds for monster movement
        float minX = offsetX;
        float maxX = offsetX + (cols - 1) * cellSize;
        float minZ = offsetZ - (rows - 1) * cellSize;
        float maxZ = offsetZ;
        
        // Resize the ground plane to fit the level grid
        ResizeGroundPlane(cols, rows);
        
        // Create invisible boundary walls around the level perimeter
        CreateBoundaries(minX, maxX, minZ, maxZ);
        
        // Parse level data and create walls and monsters
        for (int row = 0; row < rows; row++)
        {
            string rowData = levelData[row];
            for (int col = 0; col < rowData.Length && col < cols; col++)
            {
                char cell = rowData[col];
                
                // Create wall for "=" character
                if (cell == '=')
                {
                    Vector3 position = new Vector3(
                        offsetX + col * cellSize,
                        wallScale.y / 2f, // Position at half height so bottom sits on ground
                        offsetZ - row * cellSize
                    );
                    
                    CreateWall(position);
                }
                // Create monster for "m" character
                else if (cell == 'm')
                {
                    Vector3 position = new Vector3(
                        offsetX + col * cellSize,
                        monsterScale.y / 2f, // Position at half height so bottom sits on ground
                        offsetZ - row * cellSize
                    );
                    
                    CreateMonster(position, minX, maxX, minZ, maxZ);
                }
                // Create weapon for "w" character
                else if (cell == 'w')
                {
                    Vector3 position = new Vector3(
                        offsetX + col * cellSize,
                        weaponScale.y / 2f, // Position at half height so bottom sits on ground
                        offsetZ - row * cellSize
                    );
                    
                    CreateWeapon(position);
                }
                // Create hole for "*" character
                else if (cell == '*')
                {
                    Vector3 position = new Vector3(
                        offsetX + col * cellSize,
                        0f,
                        offsetZ - row * cellSize
                    );
                    
                    CreateHole(position);
                }
                // Create kitty for "k" character
                else if (cell == 'k')
                {
                    Vector3 position = new Vector3(
                        offsetX + col * cellSize,
                        kittyScale.y / 2f,
                        offsetZ - row * cellSize
                    );
                    
                    CreateKitty(position);
                }
                // Create player for "u" character
                else if (cell == 'u')
                {
                    Vector3 position = new Vector3(
                        offsetX + col * cellSize,
                        playerScale.y / 2f,
                        offsetZ - row * cellSize
                    );
                    
                    CreatePlayer(position);
                }
            }
        }
        
        // Monsters must avoid hole cells (triggers are ignored by SphereCast)
        List<Vector3> holeWorldPositions = new List<Vector3>(holeObjects.Count);
        foreach (GameObject hole in holeObjects)
        {
            if (hole != null)
            {
                holeWorldPositions.Add(hole.transform.position);
            }
        }
        
        float monsterHoleAvoidRadius = cellSize * 0.5f;
        foreach (GameObject monster in monsterObjects)
        {
            if (monster == null)
            {
                continue;
            }
            
            RandomSphereMovement movement = monster.GetComponent<RandomSphereMovement>();
            if (movement != null)
            {
                movement.avoidPositions = new List<Vector3>(holeWorldPositions);
                movement.avoidRadius = monsterHoleAvoidRadius;
            }
        }
        
        Debug.Log($"Loaded level {index + 1} with {wallObjects.Count} walls, {monsterObjects.Count} monsters, {weaponObjects.Count} weapons, {holeObjects.Count} holes, and {kittyObjects.Count} kitties");
    }
    
    void CreateWall(Vector3 position)
    {
        // Create primitive (cube or sphere)
        PrimitiveType primitiveType = useCubes ? PrimitiveType.Cube : PrimitiveType.Sphere;
        GameObject wall = GameObject.CreatePrimitive(primitiveType);
        
        // Set position and scale
        wall.transform.position = position;
        wall.transform.localScale = wallScale;
        
        // Set name
        wall.name = $"Wall_{wallObjects.Count}";
        
        // Apply material if provided
        if (wallMaterial != null)
        {
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = wallMaterial;
            }
        }
        
        // Add to parent if specified, otherwise create a default parent
        if (wallsParent == null)
        {
            // Create a parent if it doesn't exist
            Transform existingParent = transform.Find("Walls");
            if (existingParent == null)
            {
                GameObject wallsContainer = new GameObject("Walls");
                wallsContainer.transform.SetParent(transform);
                wallsParent = wallsContainer.transform;
            }
            else
            {
                wallsParent = existingParent;
            }
        }
        wall.transform.SetParent(wallsParent);
        
        // Ensure the wall has a collider (CreatePrimitive already adds one, but make sure it's not a trigger)
        Collider collider = wall.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }
        
        wallObjects.Add(wall);
    }
    
    void CreateMonster(Vector3 position, float minX, float maxX, float minZ, float maxZ)
    {
        GameObject monster;
        
        if (monsterPrefab != null)
        {
            // Instantiate from prefab (carries its own mesh, materials, and animator)
            monster = Instantiate(monsterPrefab);
        }
        else
        {
            // Fallback to sphere primitive
            monster = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            if (monsterMaterial != null)
            {
                Renderer renderer = monster.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = monsterMaterial;
                }
            }
        }
        
        monster.transform.position = position;
        monster.transform.localScale = monsterScale;
        monster.name = $"Monster_{monsterObjects.Count}";
        
        // Add RandomSphereMovement component
        RandomSphereMovement movement = monster.AddComponent<RandomSphereMovement>();
        float speedMultiplier = GameManager.Instance != null ? GameManager.Instance.GetMonsterSpeedMultiplier() : 1f;
        movement.moveSpeed = monsterMoveSpeed * speedMultiplier;
        movement.directionChangeInterval = monsterDirectionChangeInterval;
        movement.useBounds = true;
        movement.minX = minX;
        movement.maxX = maxX;
        movement.minZ = minZ;
        movement.maxZ = maxZ;
        movement.canSpawnOnCollision = false;
        
        // Ensure the monster has a collider for collision detection
        if (monster.GetComponentInChildren<Collider>() == null)
        {
            CapsuleCollider capsule = monster.AddComponent<CapsuleCollider>();
            capsule.isTrigger = false;
        }
        else
        {
            foreach (Collider col in monster.GetComponentsInChildren<Collider>())
            {
                col.isTrigger = false;
            }
        }
        
        // Add to parent if specified, otherwise create a default parent
        if (monstersParent == null)
        {
            Transform existingParent = transform.Find("Monsters");
            if (existingParent == null)
            {
                GameObject monstersContainer = new GameObject("Monsters");
                monstersContainer.transform.SetParent(transform);
                monstersParent = monstersContainer.transform;
            }
            else
            {
                monstersParent = existingParent;
            }
        }
        monster.transform.SetParent(monstersParent);
        
        monsterObjects.Add(monster);
    }
    
    void CreateWeapon(Vector3 position)
    {
        // Check if weapon prefab is assigned
        if (weaponPrefab == null)
        {
            // Try to find Weapon GameObject in scene as fallback
            GameObject sceneWeapon = GameObject.Find("Weapon");
            if (sceneWeapon != null)
            {
                weaponPrefab = sceneWeapon;
                Debug.Log("Found Weapon GameObject in scene, using it as template.");
            }
            else
            {
                Debug.LogWarning("Weapon prefab is not assigned in LevelLoader and no 'Weapon' GameObject found in scene. Cannot create weapon.");
                return;
            }
        }
        
        // Instantiate weapon from prefab
        GameObject weapon = Instantiate(weaponPrefab);
        
        // Set position and scale
        weapon.transform.position = position;
        weapon.transform.localScale = weaponScale;
        
        // Set name
        weapon.name = $"Weapon_{weaponObjects.Count}";
        
        // Add to parent if specified, otherwise create a default parent
        if (weaponsParent == null)
        {
            // Create a parent if it doesn't exist
            Transform existingParent = transform.Find("Weapons");
            if (existingParent == null)
            {
                GameObject weaponsContainer = new GameObject("Weapons");
                weaponsContainer.transform.SetParent(transform);
                weaponsParent = weaponsContainer.transform;
            }
            else
            {
                weaponsParent = existingParent;
            }
        }
        weapon.transform.SetParent(weaponsParent);
        
        weaponObjects.Add(weapon);
    }
    
    /// <summary>
    /// World position where the player spawns for the current level (for hole respawn).
    /// </summary>
    public Vector3 GetPlayerSpawnPosition()
    {
        return playerSpawnPosition;
    }
    
    void CreateHole(Vector3 gridPosition)
    {
        GameObject hole = new GameObject($"Hole_{holeObjects.Count}");
        hole.transform.position = new Vector3(gridPosition.x, 0f, gridPosition.z);
        
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Collider quadCollider = quad.GetComponent<Collider>();
        if (quadCollider != null)
        {
            Destroy(quadCollider);
        }
        
        quad.transform.SetParent(hole.transform, false);
        quad.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        quad.transform.localScale = new Vector3(
            cellSize * holeScale.x,
            cellSize * holeScale.y,
            1f
        );
        
        if (holeMaterial != null)
        {
            Renderer renderer = quad.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = holeMaterial;
            }
        }
        
        BoxCollider box = hole.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 0.6f, 0f);
        box.size = new Vector3(
            cellSize * holeScale.x * 0.95f,
            1.2f,
            cellSize * holeScale.z * 0.95f
        );
        
        HoleTrigger holeTrigger = hole.AddComponent<HoleTrigger>();
        holeTrigger.levelLoader = this;
        
        if (holesParent == null)
        {
            Transform existingParent = transform.Find("Holes");
            if (existingParent == null)
            {
                GameObject holesContainer = new GameObject("Holes");
                holesContainer.transform.SetParent(transform);
                holesParent = holesContainer.transform;
            }
            else
            {
                holesParent = existingParent;
            }
        }
        hole.transform.SetParent(holesParent);
        
        holeObjects.Add(hole);
    }
    
    void CreateKitty(Vector3 position)
    {
        GameObject kitty = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        kitty.transform.position = position;
        kitty.transform.localScale = kittyScale;
        kitty.name = $"Kitty_{kittyObjects.Count}";
        
        if (kittyMaterial != null)
        {
            Renderer renderer = kitty.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = kittyMaterial;
            }
        }
        
        Collider col = kitty.GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        // Kinematic Rigidbody so OnCollisionEnter fires vs kinematic monsters (static collider alone does not)
        Rigidbody kittyRb = kitty.AddComponent<Rigidbody>();
        kittyRb.isKinematic = true;
        kittyRb.useGravity = false;

        if (kitty.GetComponent<KittyCollisionHandler>() == null)
        {
            kitty.AddComponent<KittyCollisionHandler>();
        }
        
        if (kittiesParent == null)
        {
            Transform existingParent = transform.Find("Kitties");
            if (existingParent == null)
            {
                GameObject kittiesContainer = new GameObject("Kitties");
                kittiesContainer.transform.SetParent(transform);
                kittiesParent = kittiesContainer.transform;
            }
            else
            {
                kittiesParent = existingParent;
            }
        }
        kitty.transform.SetParent(kittiesParent);
        
        kittyObjects.Add(kitty);
    }
    
    void CreatePlayer(Vector3 position)
    {
        // Clear any existing player before creating a new one
        ClearPlayer();
        
        playerSpawnPosition = position;
        
        GameObject player;
        
        if (playerPrefab != null)
        {
            player = Instantiate(playerPrefab);
        }
        else
        {
            // Fallback to cube primitive
            player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
        
        player.transform.position = position;
        player.transform.localScale = playerScale;
        player.name = "Player";
        
        // Add Rigidbody if not already present
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = player.AddComponent<Rigidbody>();
        }
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        // Add collider if not present anywhere in hierarchy
        if (player.GetComponentInChildren<Collider>() == null)
        {
            CapsuleCollider capsule = player.AddComponent<CapsuleCollider>();
            capsule.isTrigger = false;
        }
        else
        {
            foreach (Collider col in player.GetComponentsInChildren<Collider>())
            {
                col.isTrigger = false;
            }
        }
        
        // Add movement controller
        PlayerRigidbodyController controller = player.GetComponent<PlayerRigidbodyController>();
        if (controller == null)
        {
            controller = player.AddComponent<PlayerRigidbodyController>();
        }
        controller.moveSpeed = playerMoveSpeed;
        controller.jumpForce = playerJumpForce;
        
        // Add collision handler
        if (player.GetComponent<PlayerCollisionHandler>() == null)
        {
            player.AddComponent<PlayerCollisionHandler>();
        }
        
        // Wire the camera to follow the new player
        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null)
        {
            cam.target = player.transform;
        }
        
        playerInstance = player;
    }
    
    void ClearPlayer()
    {
        if (playerInstance != null)
        {
            DestroyImmediate(playerInstance);
            playerInstance = null;
        }
    }
    
    void ClearWalls()
    {
        foreach (GameObject wall in wallObjects)
        {
            if (wall != null)
            {
                DestroyImmediate(wall);
            }
        }
        wallObjects.Clear();
    }
    
    void ClearMonsters()
    {
        foreach (GameObject monster in monsterObjects)
        {
            if (monster != null)
            {
                DestroyImmediate(monster);
            }
        }
        monsterObjects.Clear();
    }
    
    void ClearWeapons()
    {
        foreach (GameObject weapon in weaponObjects)
        {
            if (weapon != null)
            {
                DestroyImmediate(weapon);
            }
        }
        weaponObjects.Clear();
    }
    
    void ClearHoles()
    {
        foreach (GameObject hole in holeObjects)
        {
            if (hole != null)
            {
                DestroyImmediate(hole);
            }
        }
        holeObjects.Clear();
    }
    
    void ClearKitties()
    {
        foreach (GameObject kitty in kittyObjects)
        {
            if (kitty != null)
            {
                DestroyImmediate(kitty);
            }
        }
        kittyObjects.Clear();
    }

    /// <summary>
    /// Resizes the ground plane to fit the level grid dimensions plus margin.
    /// Unity's default Plane mesh is 10x10 units, so localScale = worldSize / 10.
    /// </summary>
    void ResizeGroundPlane(int cols, int rows)
    {
        if (groundPlane == null)
        {
            Debug.LogWarning("Ground plane is not assigned in LevelLoader. Cannot resize.");
            return;
        }
        
        float worldWidth = cols * cellSize + groundMargin * 2f;
        float worldDepth = rows * cellSize + groundMargin * 2f;
        
        groundPlane.localScale = new Vector3(worldWidth / 10f, 1f, worldDepth / 10f);
        groundPlane.position = new Vector3(0f, groundPlane.position.y, 0f);
    }
    
    /// <summary>
    /// Creates 4 invisible boundary walls around the level perimeter.
    /// Each wall is a thin BoxCollider with no renderer.
    /// </summary>
    void CreateBoundaries(float minX, float maxX, float minZ, float maxZ)
    {
        ClearBoundaries();
        
        float halfMargin = groundMargin / 2f;
        float levelWidth = maxX - minX + cellSize;   // full grid width in world units
        float levelDepth = maxZ - minZ + cellSize;   // full grid depth in world units
        float centerX = (minX + maxX) / 2f;
        float centerZ = (minZ + maxZ) / 2f;
        float wallThickness = 0.5f;
        float wallY = boundaryWallHeight / 2f;
        
        // Boundary extends to cover the grid + margin
        float totalWidth = levelWidth + groundMargin * 2f;
        float totalDepth = levelDepth + groundMargin * 2f;
        
        // North wall (positive Z edge)
        CreateBoundaryWall(
            $"Boundary_North",
            new Vector3(centerX, wallY, maxZ + cellSize / 2f + halfMargin),
            new Vector3(totalWidth, boundaryWallHeight, wallThickness)
        );
        
        // South wall (negative Z edge)
        CreateBoundaryWall(
            $"Boundary_South",
            new Vector3(centerX, wallY, minZ - cellSize / 2f - halfMargin),
            new Vector3(totalWidth, boundaryWallHeight, wallThickness)
        );
        
        // East wall (positive X edge)
        CreateBoundaryWall(
            $"Boundary_East",
            new Vector3(maxX + cellSize / 2f + halfMargin, wallY, centerZ),
            new Vector3(wallThickness, boundaryWallHeight, totalDepth)
        );
        
        // West wall (negative X edge)
        CreateBoundaryWall(
            $"Boundary_West",
            new Vector3(minX - cellSize / 2f - halfMargin, wallY, centerZ),
            new Vector3(wallThickness, boundaryWallHeight, totalDepth)
        );
    }
    
    /// <summary>
    /// Creates a single invisible boundary wall with a BoxCollider.
    /// </summary>
    void CreateBoundaryWall(string name, Vector3 position, Vector3 size)
    {
        GameObject boundary = new GameObject(name);
        boundary.transform.position = position;
        
        BoxCollider collider = boundary.AddComponent<BoxCollider>();
        collider.size = size;
        collider.isTrigger = false;
        
        // Parent under a Boundaries container
        Transform boundariesParent = transform.Find("Boundaries");
        if (boundariesParent == null)
        {
            GameObject container = new GameObject("Boundaries");
            container.transform.SetParent(transform);
            boundariesParent = container.transform;
        }
        boundary.transform.SetParent(boundariesParent);
        
        boundaryObjects.Add(boundary);
    }
    
    /// <summary>
    /// Destroys all boundary wall objects.
    /// </summary>
    void ClearBoundaries()
    {
        foreach (GameObject boundary in boundaryObjects)
        {
            if (boundary != null)
            {
                DestroyImmediate(boundary);
            }
        }
        boundaryObjects.Clear();
    }
    
    /// <summary>
    /// Called when the player presses a key after level complete.
    /// Clears all dynamically generated objects from the previous level, then loads the next.
    /// </summary>
    private void OnProceedToNextLevel()
    {
        if (GameManager.Instance != null)
        {
            int nextLevel = GameManager.Instance.GetCurrentLevel();
            Debug.Log($"Clearing previous level and loading level: {nextLevel + 1}");
            LoadLevel(nextLevel);
        }
    }
    
    void OnDestroy()
    {
        ClearWalls();
        ClearMonsters();
        ClearWeapons();
        ClearBoundaries();
        ClearHoles();
        ClearKitties();
        ClearPlayer();
        
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnProceedToNextLevel -= OnProceedToNextLevel;
        }
    }
}

