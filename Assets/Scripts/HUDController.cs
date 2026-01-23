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
    
    [Header("Display Settings")]
    [Tooltip("Prefix text for lives display (e.g., 'Lives: ')")]
    public string livesPrefix = "Lives: ";
    
    [Tooltip("Prefix text for level display (e.g., 'Level: ')")]
    public string levelPrefix = "Level: ";
    
    void Start()
    {
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged += UpdateLivesDisplay;
            GameManager.Instance.OnLevelChanged += UpdateLevelDisplay;
            
            // Initialize display with current values
            UpdateLivesDisplay(GameManager.Instance.GetLives());
            UpdateLevelDisplay(GameManager.Instance.GetCurrentLevel());
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
}
