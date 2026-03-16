using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages game state including lives and current level.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    [Tooltip("Starting number of lives")]
    public int startingLives = 3;
    
    [Tooltip("Current level index (0-based)")]
    public int currentLevel = 0;
    
    [Header("Scene Settings")]
    [Tooltip("Name of the landing scene to return to on game over")]
    public string landingSceneName = "LandingScene";
    
    [Tooltip("Delay before loading landing scene after game over (in seconds)")]
    public float gameOverDelay = 2f;
    
    private int currentLives;
    private bool isGameOver = false;
    private bool hasWeapon = false;
    private bool isLevelComplete = false;
    private float monsterSpeedMultiplier = 1f;
    
    // Events for HUD updates
    public System.Action<int> OnLivesChanged;
    public System.Action<int> OnLevelChanged;
    public System.Action<bool> OnWeaponChanged;
    public System.Action OnGameOver;
    public System.Action OnLevelComplete;
    public System.Action OnProceedToNextLevel;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize lives
        currentLives = startingLives;
    }
    
    void Start()
    {
        // Notify HUD of initial values
        OnLivesChanged?.Invoke(currentLives);
        OnLevelChanged?.Invoke(currentLevel);
    }
    
    void Update()
    {
        // Wait for any key press to proceed after level complete
        if (isLevelComplete && Input.anyKeyDown)
        {
            ProceedToNextLevel();
        }
    }
    
    /// <summary>
    /// Gets the current number of lives.
    /// </summary>
    public int GetLives()
    {
        return currentLives;
    }
    
    /// <summary>
    /// Sets the current number of lives.
    /// </summary>
    public void SetLives(int lives)
    {
        currentLives = Mathf.Max(0, lives);
        OnLivesChanged?.Invoke(currentLives);
    }
    
    /// <summary>
    /// Decreases lives by 1.
    /// </summary>
    public void LoseLife()
    {
        if (isGameOver)
        {
            return; // Don't process if game is already over
        }
        
        SetLives(currentLives - 1);
        
        // Check for game over
        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
    }
    
    /// <summary>
    /// Increases lives by 1.
    /// </summary>
    public void GainLife()
    {
        SetLives(currentLives + 1);
    }
    
    /// <summary>
    /// Gets whether the player currently has a weapon.
    /// </summary>
    public bool HasWeapon()
    {
        return hasWeapon;
    }
    
    /// <summary>
    /// Called when the player picks up a weapon.
    /// </summary>
    public void PickUpWeapon()
    {
        hasWeapon = true;
        OnWeaponChanged?.Invoke(hasWeapon);
        Debug.Log("Player picked up a weapon!");
    }
    
    /// <summary>
    /// Resets the weapon state to false (called at start of each level attempt).
    /// </summary>
    public void ResetWeapon()
    {
        hasWeapon = false;
        OnWeaponChanged?.Invoke(hasWeapon);
    }
    
    /// <summary>
    /// Called when the player defeats a monster with a weapon, completing the level.
    /// Freezes the game and waits for a key press before loading the next level.
    /// </summary>
    public void CompleteLevel()
    {
        if (isGameOver || isLevelComplete) return;
        
        isLevelComplete = true;
        Debug.Log($"Level {currentLevel + 1} complete! Press any key to continue...");
        
        // Freeze the game (stops physics and Time.deltaTime-based movement)
        Time.timeScale = 0f;
        
        // Notify UI to show "Level Complete!" message
        OnLevelComplete?.Invoke();
    }
    
    /// <summary>
    /// Called when the player presses a key after level complete.
    /// Unfreezes the game, clears the level, and loads the next one.
    /// </summary>
    private void ProceedToNextLevel()
    {
        isLevelComplete = false;
        
        // Unfreeze the game
        Time.timeScale = 1f;
        
        // Reset weapon for next level
        ResetWeapon();
        
        // Advance to the next level
        NextLevel();
        
        // Notify LevelLoader to clear previous level and load the next one
        OnProceedToNextLevel?.Invoke();
    }
    
    /// <summary>
    /// Checks if the level is currently in the "complete" paused state.
    /// </summary>
    public bool IsLevelComplete()
    {
        return isLevelComplete;
    }
    
    /// <summary>
    /// Gets the current level index (0-based).
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    /// <summary>
    /// Gets the current level number (1-based for display).
    /// </summary>
    public int GetCurrentLevelNumber()
    {
        return currentLevel + 1;
    }
    
    /// <summary>
    /// Sets the current level index.
    /// </summary>
    public void SetLevel(int levelIndex)
    {
        currentLevel = Mathf.Clamp(levelIndex, 0, Levels.All.Count - 1);
        OnLevelChanged?.Invoke(currentLevel);
    }
    
    /// <summary>
    /// Advances to the next level.
    /// </summary>
    public void NextLevel()
    {
        if (currentLevel < Levels.All.Count - 1)
        {
            SetLevel(currentLevel + 1);
        }
        else
        {
            monsterSpeedMultiplier *= 1.5f;
            SetLevel(0);
        }
    }
    
    /// <summary>
    /// Resets the game state.
    /// </summary>
    public void ResetGame()
    {
        currentLives = startingLives;
        currentLevel = 0;
        isGameOver = false;
        hasWeapon = false;
        isLevelComplete = false;
        monsterSpeedMultiplier = 1f;
        Time.timeScale = 1f;
        OnLivesChanged?.Invoke(currentLives);
        OnLevelChanged?.Invoke(currentLevel);
        OnWeaponChanged?.Invoke(hasWeapon);
    }
    
    /// <summary>
    /// Triggers game over state and loads the landing scene.
    /// </summary>
    private void TriggerGameOver()
    {
        if (isGameOver)
        {
            return; // Already triggered
        }
        
        isGameOver = true;
        Debug.Log("Game Over! Returning to landing scene...");
        
        // Notify listeners (e.g., UI)
        OnGameOver?.Invoke();
        
        // Load landing scene after delay
        Invoke(nameof(LoadLandingScene), gameOverDelay);
    }
    
    /// <summary>
    /// Loads the landing scene.
    /// </summary>
    private void LoadLandingScene()
    {
        if (string.IsNullOrEmpty(landingSceneName))
        {
            Debug.LogError("Landing scene name is not set!");
            return;
        }
        
        // Reset game state before loading
        ResetGame();
        
        // Load the scene
        SceneManager.LoadScene(landingSceneName);
    }
    
    /// <summary>
    /// Checks if the game is over.
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOver;
    }
    
    /// <summary>
    /// Gets the landing scene name.
    /// </summary>
    public string GetLandingSceneName()
    {
        return landingSceneName;
    }
    
    public float GetMonsterSpeedMultiplier()
    {
        return monsterSpeedMultiplier;
    }
}
