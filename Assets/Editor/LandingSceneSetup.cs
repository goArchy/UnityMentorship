#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to create and set up the Landing Scene with UI elements.
/// Run this from the menu: Tools > Create Landing Scene
/// </summary>
public class LandingSceneSetup : EditorWindow
{
    [MenuItem("Tools/Create Landing Scene")]
    public static void CreateLandingScene()
    {
        // Create a new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Remove the default Main Camera (we'll create a UI-only scene)
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            DestroyImmediate(mainCamera);
        }
        
        // Remove the default Directional Light (optional, but cleaner for a menu screen)
        GameObject directionalLight = GameObject.Find("Directional Light");
        if (directionalLight != null)
        {
            DestroyImmediate(directionalLight);
        }
        
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
        
        // Create EventSystem if it doesn't exist
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // Create a background panel (optional, for visual appeal)
        GameObject backgroundPanel = new GameObject("BackgroundPanel");
        backgroundPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = backgroundPanel.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = backgroundPanel.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark blue-gray background
        
        // Create Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvasObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(800, 150);
        titleRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Mentorship 3D";
        titleText.fontSize = 72;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        // Create Start Button
        GameObject buttonObj = new GameObject("StartButton");
        buttonObj.transform.SetParent(canvasObj.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.sizeDelta = new Vector2(300, 80);
        buttonRect.anchoredPosition = Vector2.zero;
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.9f, 1f); // Blue button
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Create Button Text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "START";
        buttonText.fontSize = 36;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        
        // Add SceneLoader component to a manager object
        GameObject sceneManagerObj = new GameObject("SceneManager");
        SceneLoader sceneLoader = sceneManagerObj.AddComponent<SceneLoader>();
        sceneLoader.targetSceneName = "SampleScene";
        sceneLoader.loadDelay = 0.1f;
        
        // Connect button to SceneLoader
        button.onClick.AddListener(sceneLoader.LoadTargetScene);
        
        // Save the scene
        string scenePath = "Assets/Scenes/LandingScene.unity";
        bool success = EditorSceneManager.SaveScene(newScene, scenePath);
        
        if (success)
        {
            Debug.Log($"Landing Scene created successfully at {scenePath}");
            Debug.Log("Don't forget to add LandingScene to Build Settings!");
            
            // Add scene to build settings
            AddSceneToBuildSettings(scenePath);
        }
        else
        {
            Debug.LogError("Failed to save Landing Scene!");
        }
    }
    
    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        
        // Check if scene is already in build settings
        bool alreadyAdded = scenes.Any(s => s.path == scenePath);
        
        if (!alreadyAdded)
        {
            var newScene = new EditorBuildSettingsScene(scenePath, true);
            scenes.Add(newScene);
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("LandingScene added to Build Settings!");
        }
        else
        {
            Debug.Log("LandingScene already in Build Settings.");
        }
    }
}

#endif
