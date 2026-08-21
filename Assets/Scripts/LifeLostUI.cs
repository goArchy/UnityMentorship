using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Centered overlay shown when the player loses a life but the game is not over.
/// </summary>
public class LifeLostUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The life-lost panel GameObject (created at runtime if unset)")]
    public GameObject lifeLostPanel;
    
    [Tooltip("Text component displaying the life-lost message")]
    public TextMeshProUGUI lifeLostText;
    
    [Header("Display Settings")]
    [Tooltip("Message when the player hits a monster or falls in a hole")]
    public string youLostALifeMessage = "You lost a life!";
    
    [Tooltip("Message when a monster reaches the kitty")]
    public string poorKittyMessage = "Poor kitty!";
    
    [Tooltip("Subtitle under the main message")]
    public string subtitleText = "Press any key to retry...";
    
    private bool isInitialized;
    
    void Start()
    {
        Initialize();
    }
    
    /// <summary>
    /// Subscribes to game events. Safe to call when this component lives on an inactive panel.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;
        
        isInitialized = true;
        
        EnsurePanel();
        
        if (lifeLostPanel != null)
            lifeLostPanel.SetActive(false);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLifeLostPause += ShowLifeLost;
            GameManager.Instance.OnLifeLostResume += HideLifeLost;
            GameManager.Instance.OnGameOver += HideLifeLost;
        }
        else
        {
            Debug.LogWarning("LifeLostUI: GameManager instance not found. Make sure GameManager exists in the scene.");
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLifeLostPause -= ShowLifeLost;
            GameManager.Instance.OnLifeLostResume -= HideLifeLost;
            GameManager.Instance.OnGameOver -= HideLifeLost;
        }
    }
    
    private void ShowLifeLost(LifeLostReason reason)
    {
        EnsurePanel();
        
        if (lifeLostText != null)
        {
            string message = reason == LifeLostReason.KittyHit
                ? poorKittyMessage
                : youLostALifeMessage;
            
            lifeLostText.text = string.IsNullOrEmpty(subtitleText)
                ? message
                : message + "\n" + subtitleText;
        }
        
        if (lifeLostPanel != null)
            lifeLostPanel.SetActive(true);
    }
    
    private void HideLifeLost()
    {
        if (lifeLostPanel != null)
            lifeLostPanel.SetActive(false);
    }
    
    private void EnsurePanel()
    {
        if (lifeLostPanel != null && lifeLostText != null)
            return;
        
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("LifeLostUI: No Canvas found to host the life-lost panel.");
            return;
        }
        
        if (lifeLostPanel == null)
        {
            lifeLostPanel = new GameObject("LifeLostPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            lifeLostPanel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRect = lifeLostPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(600f, 400f);
            
            Image panelImage = lifeLostPanel.GetComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            panelImage.raycastTarget = true;
        }
        
        if (lifeLostText == null)
        {
            GameObject textObject = new GameObject("LifeLostText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(lifeLostPanel.transform, false);
            
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            lifeLostText = textObject.GetComponent<TextMeshProUGUI>();
            lifeLostText.fontSize = 48;
            lifeLostText.alignment = TextAlignmentOptions.Center;
            lifeLostText.color = Color.white;
            lifeLostText.enableWordWrapping = true;
            lifeLostText.raycastTarget = false;
        }
    }
}
