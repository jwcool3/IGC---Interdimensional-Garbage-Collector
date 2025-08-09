using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying individual crafted items
/// </summary>
public class CraftedItemDisplayItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI stackInfoText;
    [SerializeField] private Button useButton;
    
    private ResourceType currentItemType;
    private int currentAmount;
    
    public void UpdateDisplay(ResourceType itemType, int amount)
    {
        currentItemType = itemType;
        currentAmount = amount;
        
        // Update UI elements
        if (itemNameText != null)
            itemNameText.text = GetItemDisplayName(itemType);
            
        if (amountText != null)
            amountText.text = amount.ToString("N0");
            
        if (stackInfoText != null)
        {
            int maxStack = GetMaxStackSize(itemType);
            stackInfoText.text = $"{amount}/{maxStack}";
        }
        
        if (itemIcon != null)
        {
            var config = ResourceConfigManager.Instance?.GetResourceConfig(itemType);
            if (config?.icon != null)
                itemIcon.sprite = config.icon;
        }
        
        // Setup use button (for components that can be directly used)
        if (useButton != null)
        {
            useButton.gameObject.SetActive(CanUseDirectly(itemType));
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(() => UseItem(itemType));
        }
    }
    
    private string GetItemDisplayName(ResourceType itemType)
    {
        return itemType switch
        {
            ResourceType.HullPiece => "Hull Piece",
            ResourceType.PowerCell => "Power Cell",
            ResourceType.SmallShipCompartment => "Small Ship Compartment",
            _ => itemType.ToString()
        };
    }
    
    private int GetMaxStackSize(ResourceType itemType)
    {
        // This should match the stack limits in CraftedItemInventory
        return itemType switch
        {
            ResourceType.HullPiece => 100,
            ResourceType.PowerCell => 200,
            ResourceType.SmallShipCompartment => 5,
            ResourceType.AutomatedFactory => 1,
            _ => 50
        };
    }
    
    private bool CanUseDirectly(ResourceType itemType)
    {
        // Some items can be used directly (like ship compartments)
        return itemType switch
        {
            ResourceType.SmallShipCompartment => true,
            ResourceType.AutomatedFactory => true,
            _ => false
        };
    }
    
    private void UseItem(ResourceType itemType)
    {
        // Handle direct use of items
        switch (itemType)
        {
            case ResourceType.SmallShipCompartment:
                UseShipCompartment();
                break;
            case ResourceType.AutomatedFactory:
                InstallAutomatedFactory();
                break;
        }
    }
    
    private void UseShipCompartment()
    {
        if (CraftedItemInventory.Instance.HasCraftedItem(currentItemType, 1))
        {
            // Add new compartment to ship
            if (ShipManager.Instance?.AddNewCompartment() == true)
            {
                CraftedItemInventory.Instance.RemoveCraftedItem(currentItemType, 1);
                Debug.Log("Installed new ship compartment!");
            }
        }
    }
    
    private void InstallAutomatedFactory()
    {
        if (CraftedItemInventory.Instance.HasCraftedItem(currentItemType, 1))
        {
            // Install automated factory
            if (FacilityManager.Instance?.InstallAutomatedFactory() == true)
            {
                CraftedItemInventory.Instance.RemoveCraftedItem(currentItemType, 1);
                Debug.Log("Installed automated factory!");
            }
        }
    }
} 