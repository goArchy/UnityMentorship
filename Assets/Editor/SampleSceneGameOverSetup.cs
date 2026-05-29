#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to set up the Game Over UI in SampleScene.
/// Run this from the menu: Tools > Setup SampleScene Game Over UI
/// </summary>
public class SampleSceneGameOverSetup : EditorWindow
{
    [MenuItem("Tools/Setup SampleScene Game Over UI")]
    public static void SetupGameOverUI()
    {
        // Load SampleScene if not already loaded
        if (EditorSceneManager.GetActiveScene().name != "SampleScene")
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("Game Over UI setup cancelled.");
                return;
            }
            
            string scenePath = "Assets/Scenes/SampleScene.unity";
            EditorSceneManager.OpenScene(scenePath);
        }
        
        // Get or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found. Please run 'Setup SampleScene HUD' first to create the Canvas.");
            return;
        }
        
        // Check if Game Over UI already exists
        Transform gameOverTransform = canvas.transform.Find("GameOverPanel");
        if (gameOverTransform != null)
        {
            Debug.LogWarning("Game Over UI already exists in the scene. Removing old Game Over UI.");
            Undo.DestroyObjectImmediate(gameOverTransform.gameObject);
        }
        
        // Create Game Over Panel (centered on screen)
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = gameOverPanel.AddComponent<RectTransform>();
        
        // Center on screen
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600, 400);
        
        // Initially hide the panel
        gameOverPanel.SetActive(false);
        
        // Add background image
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark semi-transparent background
        
        // Create Game Over Text
        GameObject gameOverTextObj = new GameObject("GameOverText");
        gameOverTextObj.transform.SetParent(gameOverPanel.transform, false);
        RectTransform textRect = gameOverTextObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = new Vector2(0, 50);
        
        TextMeshProUGUI gameOverText = gameOverTextObj.AddComponent<TextMeshProUGUI>();
        gameOverText.text = "Game Over!\nReturning to start screen...";
        gameOverText.fontSize = 48;
        gameOverText.color = Color.white;
        gameOverText.alignment = TextAlignmentOptions.Center;
        
        // Create Return Button (optional - allows immediate return)
        GameObject returnButtonObj = new GameObject("ReturnButton");
        returnButtonObj.transform.SetParent(gameOverPanel.transform, false);
        RectTransform buttonRect = returnButtonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0);
        buttonRect.anchorMax = new Vector2(0.5f, 0);
        buttonRect.pivot = new Vector2(0.5f, 0);
        buttonRect.sizeDelta = new Vector2(300, 60);
        buttonRect.anchoredPosition = new Vector2(0, 50);
        
        Image buttonImage = returnButtonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.9f, 1f); // Blue button
        
        Button returnButton = returnButtonObj.AddComponent<Button>();
        
        // Create Button Text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(returnButtonObj.transform, false);
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Return to Start";
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        
        // Add GameOverUI component to Game Over Panel
        GameOverUI gameOverUI = gameOverPanel.AddComponent<GameOverUI>();
        gameOverUI.gameOverPanel = gameOverPanel;
        gameOverUI.gameOverText = gameOverText;
        gameOverUI.returnButton = returnButton;
        gameOverUI.gameOverMessage = "Game Over!";
        gameOverUI.subtitleText = "Returning to start screen...";
        
        // Check if GameManager exists
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found. Please ensure GameManager exists in the scene.");
        }
        
        // Check if Player exists and add PlayerCollisionHandler if needed
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerCollisionHandler collisionHandler = player.GetComponent<PlayerCollisionHandler>();
            if (collisionHandler == null)
            {
                // Ensure player has a Collider
                Collider playerCollider = player.GetComponent<Collider>();
                if (playerCollider == null)
                {
                    // Add a BoxCollider if no collider exists
                    playerCollider = player.AddComponent<BoxCollider>();
                    Debug.Log("Added BoxCollider to Player.");
                }
                
                // Add PlayerCollisionHandler
                collisionHandler = player.AddComponent<PlayerCollisionHandler>();
                collisionHandler.monsterTag = "Monster";
                Debug.Log("Added PlayerCollisionHandler to Player.");
            }
        }
        else
        {
            Debug.LogWarning("Player GameObject not found. Please ensure a Player GameObject exists in the scene.");
        }
        
        Undo.RegisterCreatedObjectUndo(gameOverPanel, "Create Game Over UI");
        
        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        Debug.Log("Game Over UI setup complete! The panel will appear when the player runs out of lives.");
    }
}
#endif
