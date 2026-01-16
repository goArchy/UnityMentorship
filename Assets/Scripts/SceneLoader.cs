using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene transitions, particularly from the landing screen to the game scene.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The name of the scene to load when Start is clicked")]
    public string targetSceneName = "SampleScene";
    
    [Header("Loading Settings")]
    [Tooltip("Delay before loading the scene (in seconds)")]
    public float loadDelay = 0.1f;
    
    /// <summary>
    /// Loads the target scene. This method can be called from a UI button.
    /// </summary>
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target scene name is not set!");
            return;
        }
        
        // Check if the scene exists in the build settings
        if (!SceneExists(targetSceneName))
        {
            Debug.LogError($"Scene '{targetSceneName}' not found in build settings!");
            return;
        }
        
        // Load the scene with a small delay for better UX
        if (loadDelay > 0)
        {
            Invoke(nameof(LoadScene), loadDelay);
        }
        else
        {
            LoadScene();
        }
    }
    
    /// <summary>
    /// Loads the scene immediately.
    /// </summary>
    private void LoadScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
    
    /// <summary>
    /// Checks if a scene exists in the build settings.
    /// </summary>
    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Quits the application. Can be used for a Quit button.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
