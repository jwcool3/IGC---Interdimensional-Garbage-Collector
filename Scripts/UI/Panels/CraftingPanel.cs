using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main crafting panel that integrates with the TabSystem
/// This panel contains the CraftingUI component and handles panel-level functionality
/// </summary>
public class CraftingPanel : MonoBehaviour
{
    [Header("Panel Components")]
    [SerializeField] private CraftingUI craftingUI;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject loadingIndicator;
    
    [Header("Panel Settings")]
    [SerializeField] private bool autoHideOnStart = false;
    
    private void Start()
    {
        InitializePanel();
    }
    
    private void InitializePanel()
    {
        // Setup back button if available
        if (backButton != null)
        {
            backButton.onClick.AddListener(ClosePanel);
        }
        
        // Get CraftingUI component if not assigned
        if (craftingUI == null)
        {
            craftingUI = GetComponentInChildren<CraftingUI>();
        }
        
        // Hide panel on start if requested
        if (autoHideOnStart)
        {
            gameObject.SetActive(false);
        }
        
        Debug.Log("CraftingPanel initialized");
    }
    
    private void OnEnable()
    {
        // Panel activated - ensure crafting UI is ready
        if (craftingUI != null)
        {
            // Refresh crafting UI when panel becomes active
            Debug.Log("CraftingPanel activated");
        }
        
        // Hide loading indicator when panel opens
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
    }
    
    /// <summary>
    /// Close the crafting panel and return to previous tab
    /// </summary>
    public void ClosePanel()
    {
        // Try to return to resources tab
        TabSystem tabSystem = FindObjectOfType<TabSystem>();
        if (tabSystem != null)
        {
            tabSystem.ShowResourcesTab();
            Debug.Log("Closed crafting panel, returned to resources tab");
        }
        else
        {
            // Fallback - just hide this panel
            gameObject.SetActive(false);
            Debug.Log("Closed crafting panel (no TabSystem found)");
        }
    }
    
    /// <summary>
    /// Show loading indicator (useful when loading recipes)
    /// </summary>
    public void ShowLoading(bool show)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(show);
        }
    }
    
    /// <summary>
    /// Get the CraftingUI component
    /// </summary>
    public CraftingUI GetCraftingUI()
    {
        return craftingUI;
    }
    
    /// <summary>
    /// Public method to open this panel (can be called from other scripts)
    /// </summary>
    public static void OpenCraftingPanel()
    {
        TabSystem tabSystem = FindObjectOfType<TabSystem>();
        if (tabSystem != null)
        {
            tabSystem.ShowCraftingTab();
        }
        else
        {
            Debug.LogWarning("Cannot open crafting panel - TabSystem not found");
        }
    }
} 