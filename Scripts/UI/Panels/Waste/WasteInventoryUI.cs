using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI controller for the waste inventory display
/// </summary>
public class WasteInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private TextMeshProUGUI inventoryCountText;

    private List<GameObject> activeItemDisplays = new List<GameObject>();
    private WasteInventoryManager inventoryManager;

    private void Start()
    {
        // Get the inventory manager
        inventoryManager = WasteInventoryManager.Instance;

        if (inventoryManager != null)
        {
            // Subscribe to inventory events
            WasteInventoryManager.OnWasteAdded += HandleWasteAdded;
            WasteInventoryManager.OnWasteRemoved += HandleWasteRemoved;
            WasteInventoryManager.OnInventoryChanged += RefreshInventoryDisplay;
            WasteInventoryManager.OnItemQuantityChanged += UpdateItemQuantity;

            // Initial display refresh
            RefreshInventoryDisplay();
        }
        else
        {
            Debug.LogError("WasteInventoryManager.Instance is null!");
        }
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            // Unsubscribe from events
            WasteInventoryManager.OnWasteAdded -= HandleWasteAdded;
            WasteInventoryManager.OnWasteRemoved -= HandleWasteRemoved;
            WasteInventoryManager.OnInventoryChanged -= RefreshInventoryDisplay;
            WasteInventoryManager.OnItemQuantityChanged -= UpdateItemQuantity;
        }
    }

    private void HandleWasteAdded(UpdatedWasteItem item)
    {
        // Optional: Add specific handling for newly added items
        Debug.Log($"New item added to inventory: {item.Name}");
    }

    private void HandleWasteRemoved(UpdatedWasteItem item)
    {
        // Optional: Add specific handling for removed items
        Debug.Log($"Item removed from inventory: {item.Name}");
    }

    public void RefreshInventoryDisplay()
    {
        if (inventoryManager == null) return;
        
        // Get all waste items from inventory
        var items = inventoryManager.GetAllWaste();
        RefreshInventoryDisplay(items);
    }

    public void RefreshInventoryDisplay(List<UpdatedWasteItem> items)
    {
        // Clear existing displays
        ClearDisplays();

        // Create new displays
        foreach (var item in items)
        {
            CreateItemDisplay(item);
        }

        // Update inventory count
        UpdateInventoryCount(items.Count);
    }

    private void CreateItemDisplay(UpdatedWasteItem item)
    {
        if (itemPrefab == null || itemContainer == null)
        {
            Debug.LogError("Item prefab or container is null!");
            return;
        }

        GameObject display = Instantiate(itemPrefab, itemContainer);
        WasteDisplay itemDisplay = display.GetComponent<WasteDisplay>();

        if (itemDisplay != null)
        {
            Debug.Log($"Initializing display for waste item: {item.Name}");
            itemDisplay.Initialize(item);
            // Load and set the icon
            Sprite icon = LoadIconForItem(item);
            if (icon != null)
            {
                itemDisplay.SetIcon(icon);
            }
            else
            {
                Debug.LogWarning($"No icon found for item: {item.Name}, using default");
            }
            activeItemDisplays.Add(display);
        }
        else
        {
            Debug.LogError("WasteItemDisplay component not found on instantiated prefab!");
        }
    }

    private Sprite LoadIconForItem(UpdatedWasteItem item)
    {
        // If item has a specific icon, use it
        if (item.Icon != null)
            return item.Icon;

        // Fallback to resource loading
        string iconPath = $"WasteIcons/{item.Rarity}/{item.Name}";
        Sprite icon = Resources.Load<Sprite>(iconPath);

        return icon ?? Resources.Load<Sprite>("WasteIcons/DefaultIcon");
    }

    private void UpdateItemQuantity()
    {
        // Refresh the entire display when quantity changes
        RefreshInventoryDisplay();
    }

    private void UpdateInventoryCount(int count)
    {
        if (inventoryCountText != null && inventoryManager != null)
        {
            inventoryCountText.text = $"Items: {count}/{inventoryManager.GetRemainingCapacity()}";
        }
    }

    private void ClearDisplays()
    {
        foreach (var display in activeItemDisplays)
        {
            if (display != null)
            {
                Destroy(display);
            }
        }
        activeItemDisplays.Clear();
    }
}
