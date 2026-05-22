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
    [Tooltip("Scale of hole triggers (per axis, multiplied by cell size on X/Z)")]
    public Vector3 holeScale = Vector3.one;
    
    [Tooltip("Parent object for all holes (optional)")]
    public Transform holesParent;
    
    [Header("Kitty Settings")]
    [Tooltip("Kitty prefab to instantiate. Falls back to sphere if not assigned.")]
    public GameObject kittyPrefab;
    
    [Tooltip("Scale of kitty spheres")]
    public Vector3 kittyScale = Vector3.one;
    
    [Tooltip("Radius when LevelLoader adds a CapsuleCollider (prefab has no collider). Unity default is 0.5.")]
    public float kittyColliderRadius = 0.15f;
    
    [Tooltip("Material for kitties (optional)")]
    public Material kittyMaterial;
    
    [Tooltip("Parent object for all kitties (optional)")]
    public Transform kittiesParent;
    
    [Header("Ground Settings")]
    [Tooltip("Material for the procedurally generated ground mesh")]
    public Material groundMaterial;
    
    [Tooltip("Parent object for the ground mesh (optional)")]
    public Transform groundParent;
    
    [Tooltip("Extra margin around the level grid for the ground mesh and boundaries")]
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
    private GameObject groundInstance;
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
        // Clear existing walls, monsters, weapons, boundaries, ground, holes, and player
        ClearWalls();
        ClearMonsters();
        ClearWeapons();
        ClearBoundaries();
        ClearGround();
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
        
        BuildGround(levelData, offsetX, offsetZ);
        
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
                // Create kitty for "k" character (Y from grid is ground level; CreateKitty snaps mesh/collider bottom to y=0)
                else if (cell == 'k')
                {
                    Vector3 position = new Vector3(
                        offsetX + col * cellSize,
                        0f,
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
    /// World position where the player spawns for the current level.
    /// </summary>
    public Vector3 GetPlayerSpawnPosition()
    {
        return playerSpawnPosition;
    }
    
    /// <summary>
    /// Reloads the current level from scratch (player, monsters, weapon, and ground reset).
    /// </summary>
    public void ReloadCurrentLevel()
    {
        int index = levelIndex;
        if (GameManager.Instance != null)
            index = GameManager.Instance.GetCurrentLevel();
        
        LoadLevel(index);
    }
    
    void CreateHole(Vector3 gridPosition)
    {
        GameObject hole = new GameObject($"Hole_{holeObjects.Count}");
        hole.transform.position = new Vector3(gridPosition.x, 0f, gridPosition.z);
        
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
        GameObject kitty;
        
        if (kittyPrefab != null)
        {
            kitty = Instantiate(kittyPrefab);
        }
        else
        {
            kitty = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            if (kittyMaterial != null)
            {
                Renderer renderer = kitty.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = kittyMaterial;
                }
            }
        }
        
        kitty.transform.position = position;
        kitty.transform.localScale = kittyScale;
        kitty.name = $"Kitty_{kittyObjects.Count}";
        
        // Ensure the kitty has a collider for physics and KittyCollisionHandler OverlapSphere context
        if (kitty.GetComponentInChildren<Collider>() == null)
        {
            CapsuleCollider capsule = kitty.AddComponent<CapsuleCollider>();
            capsule.radius = Mathf.Max(0.01f, kittyColliderRadius);
            capsule.isTrigger = false;
        }
        else
        {
            foreach (Collider col in kitty.GetComponentsInChildren<Collider>())
            {
                col.isTrigger = false;
            }
        }

        // Kinematic Rigidbody so OnCollisionEnter fires vs kinematic monsters (static collider alone does not)
        Rigidbody kittyRb = kitty.GetComponent<Rigidbody>();
        if (kittyRb == null)
        {
            kittyRb = kitty.AddComponent<Rigidbody>();
        }
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
        
        SnapKittyBottomToGround(kitty);
        
        kittyObjects.Add(kitty);
    }
    
    /// <summary>
    /// Moves the kitty vertically so the lowest point of renderers (or colliders) rests on y=0.
    /// Works for prefabs with pivot at feet or center; sphere fallback included.
    /// </summary>
    static void SnapKittyBottomToGround(GameObject kitty)
    {
        bool hasBounds = false;
        Bounds worldBounds = default;
        
        foreach (Renderer r in kitty.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled)
            {
                continue;
            }
            
            if (!hasBounds)
            {
                worldBounds = r.bounds;
                hasBounds = true;
            }
            else
            {
                worldBounds.Encapsulate(r.bounds);
            }
        }
        
        if (!hasBounds)
        {
            foreach (Collider c in kitty.GetComponentsInChildren<Collider>())
            {
                if (!c.enabled)
                {
                    continue;
                }
                
                if (!hasBounds)
                {
                    worldBounds = c.bounds;
                    hasBounds = true;
                }
                else
                {
                    worldBounds.Encapsulate(c.bounds);
                }
            }
        }
        
        if (!hasBounds)
        {
            return;
        }
        
        float dy = -worldBounds.min.y;
        kitty.transform.position += new Vector3(0f, dy, 0f);
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
        
        GameObject stray = GameObject.Find("Player");
        while (stray != null)
        {
            DestroyImmediate(stray);
            stray = GameObject.Find("Player");
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

    void BuildGround(string[] levelData, float offsetX, float offsetZ)
    {
        ClearGround();
        
        Mesh mesh = GroundMeshBuilder.Build(
            levelData,
            cellSize,
            offsetX,
            offsetZ,
            groundMargin,
            0f
        );
        
        groundInstance = new GameObject("GroundMesh");
        
        if (groundParent == null)
        {
            Transform existingParent = transform.Find("Ground");
            if (existingParent == null)
            {
                GameObject groundContainer = new GameObject("Ground");
                groundContainer.transform.SetParent(transform);
                groundParent = groundContainer.transform;
            }
            else
            {
                groundParent = existingParent;
            }
        }
        groundInstance.transform.SetParent(groundParent);
        groundInstance.transform.localPosition = Vector3.zero;
        
        MeshFilter meshFilter = groundInstance.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        
        MeshRenderer meshRenderer = groundInstance.AddComponent<MeshRenderer>();
        if (groundMaterial != null)
        {
            meshRenderer.material = groundMaterial;
        }
        
        MeshCollider meshCollider = groundInstance.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;
    }
    
    void ClearGround()
    {
        if (groundInstance != null)
        {
            DestroyImmediate(groundInstance);
            groundInstance = null;
        }
        
        if (groundParent != null)
        {
            for (int i = groundParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(groundParent.GetChild(i).gameObject);
            }
        }
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
        ClearGround();
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

