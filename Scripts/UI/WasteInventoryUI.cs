using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// UI Controller for displaying inventory
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
        inventoryManager = WasteInventoryManager.Instance;
        if (inventoryManager != null)
        {
            // Subscribe to inventory events
            inventoryManager.OnWasteAdded += HandleWasteAdded;
            inventoryManager.OnWasteRemoved += HandleWasteRemoved;
            inventoryManager.OnInventoryChanged += RefreshInventoryDisplay;
            inventoryManager.OnItemQuantityChanged += UpdateItemQuantity;
            
            // Initial refresh
            RefreshInventoryDisplay(inventoryManager.GetAllItems());
        }
        else
        {
            Debug.LogError("WasteInventoryManager not found!");
        }
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            // Unsubscribe from events
            inventoryManager.OnWasteAdded -= HandleWasteAdded;
            inventoryManager.OnWasteRemoved -= HandleWasteRemoved;
            inventoryManager.OnInventoryChanged -= RefreshInventoryDisplay;
            inventoryManager.OnItemQuantityChanged -= UpdateItemQuantity;
        }
    }

    private void HandleWasteAdded(WasteItem item)
    {
        // Optional: Add specific handling for newly added items
        Debug.Log($"New item added to inventory: {item.Name}");
    }

    private void HandleWasteRemoved(WasteItem item)
    {
        // Optional: Add specific handling for removed items
        Debug.Log($"Item removed from inventory: {item.Name}");
    }

    public void RefreshInventoryDisplay(List<WasteItem> items)
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

    private void CreateItemDisplay(WasteItem item)
    {
        if (itemPrefab == null || itemContainer == null) return;

        GameObject display = Instantiate(itemPrefab, itemContainer);
        WasteItemDisplay itemDisplay = display.GetComponent<WasteItemDisplay>();
        
        if (itemDisplay != null)
        {
            itemDisplay.Initialize(item);
            activeItemDisplays.Add(display);
        }
    }

    private void UpdateItemQuantity(WasteItem item)
    {
        // Find and update the specific item display
        foreach (var display in activeItemDisplays)
        {
            WasteItemDisplay itemDisplay = display.GetComponent<WasteItemDisplay>();
            if (itemDisplay != null && itemDisplay.CurrentItem.Id == item.Id)
            {
                itemDisplay.UpdateQuantity(item.Quantity);
                break;
            }
        }
    }

    private void UpdateInventoryCount(int count)
    {
        if (inventoryCountText != null)
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
