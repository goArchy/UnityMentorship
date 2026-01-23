#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to set up the HUD in SampleScene.
/// Run this from the menu: Tools > Setup SampleScene HUD
/// </summary>
public class SampleSceneHUDSetup : EditorWindow
{
    [MenuItem("Tools/Setup SampleScene HUD")]
    public static void SetupHUD()
    {
        // Load SampleScene if not already loaded
        if (EditorSceneManager.GetActiveScene().name != "SampleScene")
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("HUD setup cancelled.");
                return;
            }
            
            string scenePath = "Assets/Scenes/SampleScene.unity";
            EditorSceneManager.OpenScene(scenePath);
        }
        
        // Check if Canvas already exists
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.LogWarning("Canvas already exists in the scene. Skipping Canvas creation.");
        }
        else
        {
            // Create Canvas for UI
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Set Canvas Scaler settings for responsive UI
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }
        
        Canvas canvasToUse = existingCanvas != null ? existingCanvas : FindObjectOfType<Canvas>();
        
        // Create EventSystem if it doesn't exist
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");
        }
        
        // Check if HUD already exists
        Transform hudTransform = canvasToUse.transform.Find("HUD");
        if (hudTransform != null)
        {
            Debug.LogWarning("HUD already exists in the scene. Removing old HUD.");
            Undo.DestroyObjectImmediate(hudTransform.gameObject);
        }
        
        // Create HUD Panel (bottom of screen)
        GameObject hudPanel = new GameObject("HUD");
        hudPanel.transform.SetParent(canvasToUse.transform, false);
        RectTransform hudRect = hudPanel.AddComponent<RectTransform>();
        
        // Position at bottom of screen
        hudRect.anchorMin = new Vector2(0, 0);
        hudRect.anchorMax = new Vector2(1, 0);
        hudRect.pivot = new Vector2(0.5f, 0);
        hudRect.anchoredPosition = new Vector2(0, 20);
        hudRect.sizeDelta = new Vector2(0, 80);
        
        // Add background panel (optional, for better visibility)
        GameObject backgroundPanel = new GameObject("Background");
        backgroundPanel.transform.SetParent(hudPanel.transform, false);
        RectTransform bgRect = backgroundPanel.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = backgroundPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black background
        
        // Create Lives Text (left side)
        GameObject livesObj = new GameObject("LivesText");
        livesObj.transform.SetParent(hudPanel.transform, false);
        RectTransform livesRect = livesObj.AddComponent<RectTransform>();
        livesRect.anchorMin = new Vector2(0, 0);
        livesRect.anchorMax = new Vector2(0.5f, 1);
        livesRect.pivot = new Vector2(0, 0.5f);
        livesRect.sizeDelta = Vector2.zero;
        livesRect.anchoredPosition = new Vector2(20, 0);
        
        TextMeshProUGUI livesText = livesObj.AddComponent<TextMeshProUGUI>();
        livesText.text = "Lives: 3";
        livesText.fontSize = 36;
        livesText.color = Color.white;
        livesText.alignment = TextAlignmentOptions.Left;
        
        // Create Level Text (right side)
        GameObject levelObj = new GameObject("LevelText");
        levelObj.transform.SetParent(hudPanel.transform, false);
        RectTransform levelRect = levelObj.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.5f, 0);
        levelRect.anchorMax = new Vector2(1, 1);
        levelRect.pivot = new Vector2(1, 0.5f);
        levelRect.sizeDelta = Vector2.zero;
        levelRect.anchoredPosition = new Vector2(-20, 0);
        
        TextMeshProUGUI levelText = levelObj.AddComponent<TextMeshProUGUI>();
        levelText.text = "Level: 1";
        levelText.fontSize = 36;
        levelText.color = Color.white;
        levelText.alignment = TextAlignmentOptions.Right;
        
        // Add HUDController component to HUD panel
        HUDController hudController = hudPanel.AddComponent<HUDController>();
        hudController.livesText = livesText;
        hudController.levelText = levelText;
        
        // Check if GameManager exists, if not create it
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManager = gameManagerObj.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gameManagerObj, "Create GameManager");
        }
        
        // Check if LevelLoader exists and sync level with GameManager
        LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
        if (levelLoader != null)
        {
            gameManager.SetLevel(levelLoader.levelIndex);
        }
        
        Undo.RegisterCreatedObjectUndo(hudPanel, "Create HUD");
        
        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        Debug.Log("HUD setup complete! The HUD is now positioned at the bottom of the screen.");
    }
}
#endif
