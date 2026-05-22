using UnityEngine;
using TMPro;

/// <summary>
/// Controls the HUD display showing lives and current level.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component displaying the number of lives")]
    public TextMeshProUGUI livesText;
    
    [Tooltip("Text component displaying the current level")]
    public TextMeshProUGUI levelText;
    
    [Tooltip("Text component displaying status (weapon state and level complete)")]
    public TextMeshProUGUI statusText;
    
    [Header("Display Settings")]
    [Tooltip("Prefix text for lives display (e.g., 'Lives: ')")]
    public string livesPrefix = "Lives: ";
    
    [Tooltip("Prefix text for level display (e.g., 'Level: ')")]
    public string levelPrefix = "Level: ";
    
    [Tooltip("Text shown when player has no weapon")]
    public string weaponUnarmedText = "Unarmed";
    
    [Tooltip("Text shown when player has a weapon")]
    public string weaponArmedText = "Armed!";
    
    [Tooltip("Message shown when the level is completed")]
    public string levelCompleteMessage = "Level Complete!\nPress any key to continue...";
    
    [Tooltip("Message shown after falling in a hole when lives remain")]
    public string holeFallRetryMessage = "You fell!\nPress any key to retry...";
    
    [Tooltip("Message shown when the player runs out of lives")]
    public string gameOverMessage = "Game Over!";
    
    void Start()
    {
        // GameOverUI may be on an inactive panel; ensure it subscribes to OnGameOver
        GameOverUI gameOverUI = FindObjectOfType<GameOverUI>(true);
        if (gameOverUI != null)
            gameOverUI.Initialize();
        
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged += UpdateLivesDisplay;
            GameManager.Instance.OnLevelChanged += UpdateLevelDisplay;
            GameManager.Instance.OnWeaponChanged += UpdateWeaponDisplay;
            GameManager.Instance.OnLevelComplete += ShowLevelComplete;
            GameManager.Instance.OnProceedToNextLevel += HideLevelComplete;
            GameManager.Instance.OnHoleFallPause += ShowHoleFallPause;
            GameManager.Instance.OnHoleFallRetry += HideHoleFallPause;
            GameManager.Instance.OnGameOver += ShowGameOver;
            
            // Initialize display with current values
            UpdateLivesDisplay(GameManager.Instance.GetLives());
            UpdateLevelDisplay(GameManager.Instance.GetCurrentLevel());
            UpdateWeaponDisplay(GameManager.Instance.HasWeapon());
        }
        else
        {
            Debug.LogWarning("HUDController: GameManager instance not found. Make sure GameManager exists in the scene.");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged -= UpdateLivesDisplay;
            GameManager.Instance.OnLevelChanged -= UpdateLevelDisplay;
            GameManager.Instance.OnWeaponChanged -= UpdateWeaponDisplay;
            GameManager.Instance.OnLevelComplete -= ShowLevelComplete;
            GameManager.Instance.OnProceedToNextLevel -= HideLevelComplete;
            GameManager.Instance.OnHoleFallPause -= ShowHoleFallPause;
            GameManager.Instance.OnHoleFallRetry -= HideHoleFallPause;
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
    }
    
    /// <summary>
    /// Updates the lives display.
    /// </summary>
    private void UpdateLivesDisplay(int lives)
    {
        if (livesText != null)
        {
            livesText.text = livesPrefix + lives;
        }
    }
    
    /// <summary>
    /// Updates the level display.
    /// </summary>
    private void UpdateLevelDisplay(int levelIndex)
    {
        if (levelText != null)
        {
            // Display level as 1-based (level 0 = Level 1)
            levelText.text = levelPrefix + (levelIndex + 1);
        }
    }
    
    /// <summary>
    /// Updates the status text to show weapon state.
    /// </summary>
    private void UpdateWeaponDisplay(bool hasWeapon)
    {
        if (statusText == null)
            return;
        
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
            return;
        
        statusText.text = hasWeapon ? weaponArmedText : weaponUnarmedText;
    }
    
    /// <summary>
    /// Shows the "Level Complete!" message in the status text.
    /// </summary>
    private void ShowLevelComplete()
    {
        if (statusText != null)
        {
            statusText.text = levelCompleteMessage;
        }
    }
    
    /// <summary>
    /// Restores the status text back to weapon state after level complete.
    /// </summary>
    private void HideLevelComplete()
    {
        if (statusText != null && GameManager.Instance != null)
        {
            UpdateWeaponDisplay(GameManager.Instance.HasWeapon());
        }
    }
    
    private void ShowHoleFallPause()
    {
        if (statusText != null)
            statusText.text = holeFallRetryMessage;
    }
    
    private void HideHoleFallPause()
    {
        if (statusText != null && GameManager.Instance != null)
            UpdateWeaponDisplay(GameManager.Instance.HasWeapon());
    }
    
    private void ShowGameOver()
    {
        if (statusText != null)
            statusText.text = gameOverMessage;
    }
}
