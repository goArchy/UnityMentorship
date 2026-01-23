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
    [Tooltip("Scale of monster sphere")]
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
    
    private List<GameObject> wallObjects = new List<GameObject>();
    private List<GameObject> monsterObjects = new List<GameObject>();
    private List<GameObject> weaponObjects = new List<GameObject>();
    
    void Start()
    {
        LoadLevel(levelIndex);
        
        // Sync with GameManager if it exists
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLevel(levelIndex);
        }
    }
    
    public void LoadLevel(int index)
    {
        // Clear existing walls, monsters, and weapons
        ClearWalls();
        ClearMonsters();
        ClearWeapons();
        
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
            }
        }
        
        Debug.Log($"Loaded level {index + 1} with {wallObjects.Count} walls, {monsterObjects.Count} monsters, and {weaponObjects.Count} weapons");
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
        // Create sphere GameObject
        GameObject monster = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        // Set position and scale
        monster.transform.position = position;
        monster.transform.localScale = monsterScale;
        
        // Set name
        monster.name = $"Monster_{monsterObjects.Count}";
        
        // Apply material if provided
        if (monsterMaterial != null)
        {
            Renderer renderer = monster.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = monsterMaterial;
            }
        }
        
        // Add RandomSphereMovement component
        RandomSphereMovement movement = monster.AddComponent<RandomSphereMovement>();
        
        // Configure movement settings
        movement.moveSpeed = monsterMoveSpeed;
        movement.directionChangeInterval = monsterDirectionChangeInterval;
        movement.useBounds = true;
        movement.minX = minX;
        movement.maxX = maxX;
        movement.minZ = minZ;
        movement.maxZ = maxZ;
        movement.canSpawnOnCollision = false; // Disable spawning for monsters
        
        // Ensure the monster has a collider (CreatePrimitive already adds one, but make sure it's not a trigger)
        Collider collider = monster.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }
        
        // Add to parent if specified, otherwise create a default parent
        if (monstersParent == null)
        {
            // Create a parent if it doesn't exist
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
    
    void OnDestroy()
    {
        ClearWalls();
        ClearMonsters();
        ClearWeapons();
    }
}

