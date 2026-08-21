using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Controls the game over UI panel that appears when the player runs out of lives.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The game over panel GameObject")]
    public GameObject gameOverPanel;
    
    [Tooltip("Text component displaying the game over message")]
    public TextMeshProUGUI gameOverText;
    
    [Tooltip("Button to return to landing scene immediately (optional)")]
    public Button returnButton;
    
    [Header("Display Settings")]
    [Tooltip("Game over message text")]
    public string gameOverMessage = "Game Over!";
    
    [Tooltip("Subtitle text (optional)")]
    public string subtitleText = "Returning to start screen...";
    
    private bool isInitialized;
    
    void Start()
    {
        Initialize();
    }
    
    /// <summary>
    /// Subscribes to game events. Safe to call when this component lives on an inactive panel
    /// (Start does not run until the object is active).
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;
        
        isInitialized = true;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += ShowGameOver;
        else
            Debug.LogWarning("GameOverUI: GameManager instance not found. Make sure GameManager exists in the scene.");
        
        if (returnButton != null)
            returnButton.onClick.AddListener(ReturnToLandingScene);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
        
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(ReturnToLandingScene);
        }
    }
    
    /// <summary>
    /// Shows the game over panel.
    /// </summary>
    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Update text if provided
        if (gameOverText != null)
        {
            if (!string.IsNullOrEmpty(subtitleText))
            {
                gameOverText.text = gameOverMessage + "\n" + subtitleText;
            }
            else
            {
                gameOverText.text = gameOverMessage;
            }
        }
    }
    
    /// <summary>
    /// Returns to the landing scene immediately.
    /// </summary>
    public void ReturnToLandingScene()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToLandingScene();
        }
        else
        {
            Debug.LogError("GameOverUI: Cannot return to landing scene - GameManager instance not found!");
        }
    }
}
