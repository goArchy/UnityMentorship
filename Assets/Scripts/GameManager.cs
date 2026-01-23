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
    
    // Events for HUD updates
    public System.Action<int> OnLivesChanged;
    public System.Action<int> OnLevelChanged;
    public System.Action OnGameOver;
    
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
    }
    
    /// <summary>
    /// Resets the game state.
    /// </summary>
    public void ResetGame()
    {
        currentLives = startingLives;
        currentLevel = 0;
        isGameOver = false;
        OnLivesChanged?.Invoke(currentLives);
        OnLevelChanged?.Invoke(currentLevel);
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
}
