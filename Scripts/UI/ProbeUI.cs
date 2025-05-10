using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProbeUI : MonoBehaviour
{
    [Header("Probe List")]
    [SerializeField] private Transform probeListContainer;
    [SerializeField] private GameObject probeItemPrefab;
    
    [Header("Probe Management")]
    [SerializeField] private Button dispatchProbeButton;
    [SerializeField] private TextMeshProUGUI probeCountText;
    
    [Header("Upgrade UI")]
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private GameObject upgradeItemPrefab;
    
    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = true;
    
    private List<GameObject> probeListItems = new List<GameObject>();
    private List<GameObject> upgradeItems = new List<GameObject>();
    
    private void Awake()
    {
        // Try to find components automatically if not assigned
        if (probeListContainer == null)
            probeListContainer = transform.Find("ProbeListContainer/Viewport/Content");
            
        if (upgradeContainer == null)
            upgradeContainer = transform.Find("UpgradesContainer/Viewport/Content");
    }
    
    private void Start()
    {
        if (logDebugMessages)
            Debug.Log("ProbeUI: Start method called");
            
        // Check for instance availability
        if (ProbeManager.Instance == null)
        {
            Debug.LogWarning("ProbeUI: ProbeManager.Instance is null. Attempting to find it...");
            ProbeManager manager = UnityEngine.Object.FindFirstObjectByType<ProbeManager>();
            if (manager != null)
            {
                Debug.Log("ProbeUI: Found ProbeManager in the scene!");
            }
            else
            {
                Debug.LogError("ProbeUI: No ProbeManager found in scene! Please add it to your Managers GameObject.");
                // Create it if needed
                GameObject obj = new GameObject("ProbeManager");
                obj.AddComponent<ProbeManager>();
                Debug.Log("ProbeUI: Created ProbeManager automatically");
            }
        }
        
        if (ProbeUpgradeManager.Instance == null)
        {
            Debug.LogWarning("ProbeUI: ProbeUpgradeManager.Instance is null. Attempting to find it...");
            ProbeUpgradeManager manager = UnityEngine.Object.FindFirstObjectByType<ProbeUpgradeManager>();
            if (manager != null)
            {
                Debug.Log("ProbeUI: Found ProbeUpgradeManager in the scene!");
            }
            else
            {
                Debug.LogError("ProbeUI: No ProbeUpgradeManager found in scene! Please add it to your Managers GameObject.");
                // Create it if needed
                GameObject obj = new GameObject("ProbeUpgradeManager");
                obj.AddComponent<ProbeUpgradeManager>();
                Debug.Log("ProbeUI: Created ProbeUpgradeManager automatically");
            }
        }
        
        // Set up button listeners
        if (dispatchProbeButton != null)
        {
            dispatchProbeButton.onClick.AddListener(OnDispatchProbeClicked);
            
            if (logDebugMessages)
                Debug.Log("ProbeUI: Dispatch button listener added");
        }
        
        // Subscribe to probe events
        if (ProbeManager.Instance != null)
        {
            if (logDebugMessages)
                Debug.Log("ProbeUI: Found ProbeManager instance, subscribing to events");
                
            ProbeManager.Instance.OnProbeDispatched += OnProbeDispatched;
            ProbeManager.Instance.OnProbeCountChanged += UpdateProbeCount;
        }
        
        // Add a slight delay to ensure everything is initialized
        Invoke("DelayedRefresh", 0.5f);
    }
    
    private void DelayedRefresh()
    {
        // Initial UI setup with a delay
        RefreshProbeList();
        RefreshUpgradeItems();
    }
    
    private void OnEnable()
    {
        if (logDebugMessages)
            Debug.Log("ProbeUI: OnEnable called");
            
        // Refresh whenever UI becomes visible
        Invoke("DelayedRefresh", 0.2f);
    }
    
    public void RefreshProbeList()
    {
        if (logDebugMessages)
            Debug.Log("ProbeUI: RefreshProbeList called");
            
        // Clear existing items
        ClearProbeList();
        
        // Check for required components
        if (probeListContainer == null)
        {
            Debug.LogError("ProbeUI: probeListContainer is null. Trying to find it automatically...");
            probeListContainer = transform.Find("ProbeListContainer/Viewport/Content");
            
            if (probeListContainer == null)
            {
                Debug.LogError("ProbeUI: Could not find probeListContainer automatically!");
                return;
            }
        }
        
        if (probeItemPrefab == null)
        {
            Debug.LogError("ProbeUI: probeItemPrefab is null. Please assign it in the Inspector!");
            return;
        }
        
        // Check if ProbeManager exists
        if (ProbeManager.Instance == null)
        {
            Debug.LogError("ProbeUI: ProbeManager.Instance is null. Cannot refresh probe list!");
            return;
        }
        
        // Get active probes from manager
        var probes = ProbeManager.Instance.GetActiveProbes();
        
        if (logDebugMessages)
            Debug.Log($"ProbeUI: Found {probes.Count} probes to display");
        
        // Create UI for each probe
        foreach (var probe in probes)
        {
            GameObject probeItem = Instantiate(probeItemPrefab, probeListContainer);
            ProbeItemUI itemUI = probeItem.GetComponent<ProbeItemUI>();
            
            if (itemUI == null)
            {
                Debug.LogError("ProbeUI: ProbeItemUI component not found on prefab! Adding it...");
                itemUI = probeItem.AddComponent<ProbeItemUI>();
            }
            
            itemUI.Initialize(probe);
            
            if (logDebugMessages)
                Debug.Log($"ProbeUI: Created UI for probe {probe.probeId}");
            
            probeListItems.Add(probeItem);
        }
        
        // Update the probe count display
        UpdateProbeCount(probes.Count);
    }
    
    private void ClearProbeList()
    {
        if (logDebugMessages)
            Debug.Log($"ProbeUI: Clearing probe list with {probeListItems.Count} items");
            
        foreach (var item in probeListItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        
        probeListItems.Clear();
    }
    
    private void RefreshUpgradeItems()
    {
        if (logDebugMessages)
            Debug.Log("ProbeUI: RefreshUpgradeItems called");
            
        // Clear existing upgrade items
        ClearUpgradeItems();
        
        // Check required components
        if (upgradeContainer == null)
        {
            Debug.LogError("ProbeUI: upgradeContainer is null. Trying to find it automatically...");
            upgradeContainer = transform.Find("UpgradesContainer/Viewport/Content");
            
            if (upgradeContainer == null)
            {
                Debug.LogError("ProbeUI: Could not find upgradeContainer automatically!");
                return;
            }
        }
        
        if (upgradeItemPrefab == null)
        {
            Debug.LogError("ProbeUI: upgradeItemPrefab is null. Please assign it in the Inspector!");
            return;
        }
        
        if (ProbeUpgradeManager.Instance == null)
        {
            Debug.LogError("ProbeUI: ProbeUpgradeManager.Instance is null!");
            return;
        }
        
        // Create upgrade UI for each upgrade type
        CreateUpgradeItem("ProbeCount", "More Probes", "Increase your maximum probe count");
        CreateUpgradeItem("Efficiency", "Better Efficiency", "Increase the value of collected waste");
        CreateUpgradeItem("Speed", "Faster Collection", "Decrease the collection interval");
            
        if (logDebugMessages)
            Debug.Log("ProbeUI: Created upgrade items");
    }
    
    private void CreateUpgradeItem(string upgradeType, string name, string description)
    {
        if (ProbeUpgradeManager.Instance == null)
        {
            Debug.LogError("ProbeUI: Cannot create upgrade item - ProbeUpgradeManager.Instance is null");
            return;
        }
        
        GameObject upgradeItem = Instantiate(upgradeItemPrefab, upgradeContainer);
        ProbeUpgradeItemUI itemUI = upgradeItem.GetComponent<ProbeUpgradeItemUI>();
        
        if (itemUI == null)
        {
            Debug.LogError("ProbeUI: ProbeUpgradeItemUI component not found on prefab! Adding it...");
            itemUI = upgradeItem.AddComponent<ProbeUpgradeItemUI>();
        }
        
        int currentLevel = ProbeUpgradeManager.Instance.GetUpgradeLevel(upgradeType);
        int maxLevel = ProbeUpgradeManager.Instance.GetMaxLevel(upgradeType);
        float rpCost = ProbeUpgradeManager.Instance.GetUpgradeCost(upgradeType, false);
        float dpCost = ProbeUpgradeManager.Instance.GetUpgradeCost(upgradeType, true);
        
        itemUI.Initialize(upgradeType, name, description, currentLevel, maxLevel, rpCost, dpCost);
        
        if (logDebugMessages)
            Debug.Log($"ProbeUI: Created upgrade item for {upgradeType}");
        
        upgradeItems.Add(upgradeItem);
    }
    
    private void ClearUpgradeItems()
    {
        if (logDebugMessages)
            Debug.Log($"ProbeUI: Clearing upgrade items with {upgradeItems.Count} items");
            
        foreach (var item in upgradeItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        
        upgradeItems.Clear();
    }
    
    private void OnDispatchProbeClicked()
    {
        if (logDebugMessages)
            Debug.Log("ProbeUI: Dispatch button clicked");
            
        if (ProbeManager.Instance == null)
        {
            Debug.LogError("ProbeUI: Cannot dispatch probe - ProbeManager.Instance is null");
            return;
        }
        
        if (LocationManager.Instance == null)
        {
            Debug.LogError("ProbeUI: Cannot dispatch probe - LocationManager.Instance is null");
            return;
        }
        
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        Probe newProbe = ProbeManager.Instance.DispatchProbe(currentLocation);
        
        if (newProbe != null)
        {
            if (logDebugMessages)
                Debug.Log($"ProbeUI: Probe dispatched successfully. ID: {newProbe.probeId}");
        }
        else
        {
            Debug.LogWarning("ProbeUI: Failed to dispatch probe");
        }
    }
    
    private void OnProbeDispatched(Probe probe)
    {
        if (logDebugMessages)
            Debug.Log($"ProbeUI: OnProbeDispatched event received for probe {probe.probeId}");
            
        // Refresh the probe list
        RefreshProbeList();
    }
    
    private void UpdateProbeCount(int count)
    {
        if (probeCountText != null)
        {
            int maxCount = ProbeManager.Instance != null ? ProbeManager.Instance.GetMaxProbeCount() : 10;
            probeCountText.text = $"Probes: {count}/{maxCount}";
            
            if (logDebugMessages)
                Debug.Log($"ProbeUI: Updated probe count display to {count}/{maxCount}");
            
            // Update dispatch button state
            if (dispatchProbeButton != null)
            {
                dispatchProbeButton.interactable = count < maxCount;
            }
        }
    }
    
    // Public method for manual refresh
    public void ForceRefresh()
    {
        if (logDebugMessages)
            Debug.Log("ProbeUI: Force refresh called");
            
        RefreshProbeList();
        RefreshUpgradeItems();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ProbeManager.Instance != null)
        {
            ProbeManager.Instance.OnProbeDispatched -= OnProbeDispatched;
            ProbeManager.Instance.OnProbeCountChanged -= UpdateProbeCount;
        }
        
        if (dispatchProbeButton != null)
        {
            dispatchProbeButton.onClick.RemoveListener(OnDispatchProbeClicked);
        }
    }
}