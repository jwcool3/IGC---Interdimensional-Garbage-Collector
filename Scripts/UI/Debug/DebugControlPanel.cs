using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime UI panel for controlling debug output
/// </summary>
public class DebugControlPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private Button togglePanelButton;
    [SerializeField] private Button closeButton;
    
    [Header("Global Controls")]
    [SerializeField] private Toggle enableAllToggle;
    [SerializeField] private Toggle errorsOnlyToggle;
    [SerializeField] private Toggle errorsAndWarningsToggle;
    
    [Header("Category Toggles")]
    [SerializeField] private Toggle wasteGenerationToggle;
    [SerializeField] private Toggle wasteProcessingToggle;
    [SerializeField] private Toggle locationSystemToggle;
    [SerializeField] private Toggle inventoryManagementToggle;
    [SerializeField] private Toggle resourceSystemToggle;
    [SerializeField] private Toggle uiDebugToggle;
    [SerializeField] private Toggle probeSystemToggle;
    [SerializeField] private Toggle facilitySystemToggle;
    [SerializeField] private Toggle tradeSystemToggle;
    [SerializeField] private Toggle generalGameplayToggle;
    
    [Header("Info")]
    [SerializeField] private TextMeshProUGUI statusText;
    
    private bool isPanelVisible = false;
    
    private void Start()
    {
        SetupUI();
        UpdateUI();
    }
    
    private void Update()
    {
        // Toggle panel with F12 key
        if (Input.GetKeyDown(KeyCode.F12))
        {
            TogglePanel();
        }
        
        // Quick disable all logs with F11
        if (Input.GetKeyDown(KeyCode.F11))
        {
            QuickDisableAllLogs();
        }
        
        // Quick enable errors only with F10
        if (Input.GetKeyDown(KeyCode.F10))
        {
            QuickErrorsOnly();
        }
    }
    
    private void SetupUI()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
        
        // Setup button listeners
        if (togglePanelButton != null)
        {
            togglePanelButton.onClick.AddListener(TogglePanel);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
        
        // Setup toggle listeners
        if (wasteGenerationToggle != null)
            wasteGenerationToggle.onValueChanged.AddListener(_ => DebugManager.Instance?.ToggleCategory(DebugCategory.WasteGeneration));
        
        if (wasteProcessingToggle != null)
            wasteProcessingToggle.onValueChanged.AddListener(_ => DebugManager.Instance?.ToggleCategory(DebugCategory.WasteProcessing));
        
        if (locationSystemToggle != null)
            locationSystemToggle.onValueChanged.AddListener(_ => DebugManager.Instance?.ToggleCategory(DebugCategory.LocationSystem));
        
        if (inventoryManagementToggle != null)
            inventoryManagementToggle.onValueChanged.AddListener(_ => DebugManager.Instance?.ToggleCategory(DebugCategory.InventoryManagement));
        
        if (resourceSystemToggle != null)
            resourceSystemToggle.onValueChanged.AddListener(_ => DebugManager.Instance?.ToggleCategory(DebugCategory.ResourceSystem));
        
        if (uiDebugToggle != null)
            uiDebugToggle.onValueChanged.AddListener(_ => DebugManager.Instance?.ToggleCategory(DebugCategory.UIDebug));
        
        if (probeSystemToggle != null)
            probeSystemToggle.onValueChanged.AddListener(_ => DebugManager.Instance?.ToggleCategory(DebugCategory.ProbeSystem));
        
        if (facilitySystemToggle != null)
            facilitySystemToggle.onValueChanged.AddListener(_ => DebugManager.Instance?.ToggleCategory(DebugCategory.FacilitySystem));
        
        if (tradeSystemToggle != null)
            tradeSystemToggle.onValueChanged.AddListener(_ => DebugManager.Instance?.ToggleCategory(DebugCategory.TradeSystem));
        
        if (generalGameplayToggle != null)
            generalGameplayToggle.onValueChanged.AddListener(_ => DebugManager.Instance?.ToggleCategory(DebugCategory.GeneralGameplay));
        
        // Setup global control listeners
        if (enableAllToggle != null)
            enableAllToggle.onValueChanged.AddListener(OnEnableAllChanged);
        
        if (errorsOnlyToggle != null)
            errorsOnlyToggle.onValueChanged.AddListener(OnErrorsOnlyChanged);
        
        if (errorsAndWarningsToggle != null)
            errorsAndWarningsToggle.onValueChanged.AddListener(OnErrorsAndWarningsChanged);
    }
    
    private void UpdateUI()
    {
        if (DebugManager.Instance == null) return;
        
        // Update toggle states based on DebugManager
        if (wasteGenerationToggle != null)
            wasteGenerationToggle.SetIsOnWithoutNotify(DebugManager.WasteGeneration);
        
        if (wasteProcessingToggle != null)
            wasteProcessingToggle.SetIsOnWithoutNotify(DebugManager.WasteProcessing);
        
        if (locationSystemToggle != null)
            locationSystemToggle.SetIsOnWithoutNotify(DebugManager.LocationSystem);
        
        if (inventoryManagementToggle != null)
            inventoryManagementToggle.SetIsOnWithoutNotify(DebugManager.InventoryManagement);
        
        if (resourceSystemToggle != null)
            resourceSystemToggle.SetIsOnWithoutNotify(DebugManager.ResourceSystem);
        
        if (uiDebugToggle != null)
            uiDebugToggle.SetIsOnWithoutNotify(DebugManager.UIDebug);
        
        if (probeSystemToggle != null)
            probeSystemToggle.SetIsOnWithoutNotify(DebugManager.ProbeSystem);
        
        if (facilitySystemToggle != null)
            facilitySystemToggle.SetIsOnWithoutNotify(DebugManager.FacilitySystem);
        
        if (tradeSystemToggle != null)
            tradeSystemToggle.SetIsOnWithoutNotify(DebugManager.TradeSystem);
        
        if (generalGameplayToggle != null)
            generalGameplayToggle.SetIsOnWithoutNotify(DebugManager.GeneralGameplay);
        
        // Update status text
        UpdateStatusText();
    }
    
    private void UpdateStatusText()
    {
        if (statusText == null) return;
        
        int enabledCategories = 0;
        if (DebugManager.WasteGeneration) enabledCategories++;
        if (DebugManager.WasteProcessing) enabledCategories++;
        if (DebugManager.LocationSystem) enabledCategories++;
        if (DebugManager.InventoryManagement) enabledCategories++;
        if (DebugManager.ResourceSystem) enabledCategories++;
        if (DebugManager.UIDebug) enabledCategories++;
        if (DebugManager.ProbeSystem) enabledCategories++;
        if (DebugManager.FacilitySystem) enabledCategories++;
        if (DebugManager.TradeSystem) enabledCategories++;
        if (DebugManager.GeneralGameplay) enabledCategories++;
        
        statusText.text = $"Debug Status: {enabledCategories}/10 categories enabled\nF10: Errors Only | F11: Disable All | F12: Toggle Panel";
    }
    
    public void TogglePanel()
    {
        isPanelVisible = !isPanelVisible;
        if (debugPanel != null)
        {
            debugPanel.SetActive(isPanelVisible);
        }
        
        if (isPanelVisible)
        {
            UpdateUI();
        }
    }
    
    public void ClosePanel()
    {
        isPanelVisible = false;
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }
    
    public void QuickDisableAllLogs()
    {
        DebugManager.Instance?.SetAllCategories(false);
        UpdateUI();
        Debug.Log("DebugControlPanel: All debug logging disabled");
    }
    
    public void QuickErrorsOnly()
    {
        if (DebugManager.Instance != null)
        {
            DebugManager.Instance.SetAllCategories(false);
            // Note: This would need additional DebugManager properties to fully implement
        }
        UpdateUI();
        Debug.Log("DebugControlPanel: Showing errors only");
    }
    
    private void OnEnableAllChanged(bool value)
    {
        if (value && DebugManager.Instance != null)
        {
            DebugManager.Instance.SetAllCategories(true);
            UpdateUI();
        }
    }
    
    private void OnErrorsOnlyChanged(bool value)
    {
        if (value)
        {
            QuickErrorsOnly();
        }
    }
    
    private void OnErrorsAndWarningsChanged(bool value)
    {
        if (value && DebugManager.Instance != null)
        {
            DebugManager.Instance.SetAllCategories(false);
            // This would need additional implementation in DebugManager
        }
        UpdateUI();
    }
    
    #region Context Menu Controls (for testing in editor)
    
    [ContextMenu("Test: Toggle Panel")]
    private void TestTogglePanel()
    {
        TogglePanel();
    }
    
    [ContextMenu("Test: Disable All Logs")]
    private void TestDisableAllLogs()
    {
        QuickDisableAllLogs();
    }
    
    #endregion
} 