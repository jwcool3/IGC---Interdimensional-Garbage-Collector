using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages inventory for crafted components separately from raw resources
/// </summary>
public class CraftedItemInventory : MonoBehaviour
{
    public static CraftedItemInventory Instance { get; private set; }
    
    [Header("Inventory Settings")]
    [SerializeField] private int maxCraftedItemSlots = 200;
    [SerializeField] private bool enableStackingLimits = true;
    [SerializeField] private int defaultStackSize = 50;
    
    // Storage for crafted items
    private Dictionary<ResourceType, int> craftedItems = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, int> stackLimits = new Dictionary<ResourceType, int>();
    
    // Events
    public event Action<ResourceType, int, int> OnCraftedItemChanged; // type, oldAmount, newAmount
    public event Action<ResourceType, int> OnCraftedItemAdded;
    public event Action<ResourceType, int> OnCraftedItemRemoved;
    public event Action OnCraftedInventoryUpdated;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeInventory()
    {
        // Initialize stack limits for different component types
        InitializeStackLimits();
        DebugManager.Log("CraftedItemInventory initialized", DebugCategory.ResourceSystem);
    }
    
    private void InitializeStackLimits()
    {
        // Layer 1 components (smaller, more common)
        stackLimits[ResourceType.HullPiece] = 100;
        stackLimits[ResourceType.PowerCell] = 200;
        stackLimits[ResourceType.JointConnector] = 500;
        stackLimits[ResourceType.StructuralBeam] = 150;
        stackLimits[ResourceType.ArmorPlate] = 75;
        stackLimits[ResourceType.EnergyConduit] = 300;
        stackLimits[ResourceType.PowerRegulator] = 100;
        stackLimits[ResourceType.EnergyCrystal] = 50;
        stackLimits[ResourceType.DataProcessor] = 100;
        stackLimits[ResourceType.ServoMotor] = 150;
        stackLimits[ResourceType.SensorArray] = 100;
        stackLimits[ResourceType.LogicCore] = 50;
        
        // Layer 2 assemblies (larger, medium rarity)
        stackLimits[ResourceType.WallSection] = 50;
        stackLimits[ResourceType.ReinforcedBulkhead] = 30;
        stackLimits[ResourceType.FloorPanel] = 40;
        stackLimits[ResourceType.CeilingAssembly] = 40;
        stackLimits[ResourceType.BatteryBank] = 25;
        stackLimits[ResourceType.PowerDistributionHub] = 20;
        stackLimits[ResourceType.QuantumPowerCore] = 15;
        stackLimits[ResourceType.AssemblyRobot] = 10;
        stackLimits[ResourceType.QualityInspector] = 10;
        stackLimits[ResourceType.MaterialHandler] = 15;
        
        // Layer 3 systems (huge, rare)
        stackLimits[ResourceType.SmallShipCompartment] = 5;
        stackLimits[ResourceType.AirlockModule] = 3;
        stackLimits[ResourceType.EngineeringBay] = 2;
        stackLimits[ResourceType.PowerPlantModule] = 3;
        stackLimits[ResourceType.DimensionalGenerator] = 2;
        stackLimits[ResourceType.AutomatedFactory] = 1; // These are unique
        stackLimits[ResourceType.ResearchLaboratory] = 1;
    }
    
    /// <summary>
    /// Add crafted items to inventory
    /// </summary>
    public bool AddCraftedItem(ResourceType itemType, int amount)
    {
        if (!IsCraftedItem(itemType) || amount <= 0) return false;
        
        int currentAmount = GetCraftedItemAmount(itemType);
        int maxStack = GetStackLimit(itemType);
        int newAmount = Mathf.Min(currentAmount + amount, maxStack);
        int actualAdded = newAmount - currentAmount;
        
        if (actualAdded <= 0) return false; // Already at stack limit
        
        craftedItems[itemType] = newAmount;
        
        OnCraftedItemChanged?.Invoke(itemType, currentAmount, newAmount);
        OnCraftedItemAdded?.Invoke(itemType, actualAdded);
        OnCraftedInventoryUpdated?.Invoke();
        
        DebugManager.Log($"Added {actualAdded} {itemType} to crafted inventory", DebugCategory.ResourceSystem);
        return true;
    }
    
    /// <summary>
    /// Remove crafted items from inventory
    /// </summary>
    public bool RemoveCraftedItem(ResourceType itemType, int amount)
    {
        if (!IsCraftedItem(itemType) || amount <= 0) return false;
        
        int currentAmount = GetCraftedItemAmount(itemType);
        if (currentAmount < amount) return false;
        
        int newAmount = currentAmount - amount;
        
        if (newAmount == 0)
        {
            craftedItems.Remove(itemType);
        }
        else
        {
            craftedItems[itemType] = newAmount;
        }
        
        OnCraftedItemChanged?.Invoke(itemType, currentAmount, newAmount);
        OnCraftedItemRemoved?.Invoke(itemType, amount);
        OnCraftedInventoryUpdated?.Invoke();
        
        DebugManager.Log($"Removed {amount} {itemType} from crafted inventory", DebugCategory.ResourceSystem);
        return true;
    }
    
    /// <summary>
    /// Get current amount of a crafted item
    /// </summary>
    public int GetCraftedItemAmount(ResourceType itemType)
    {
        return craftedItems.TryGetValue(itemType, out int amount) ? amount : 0;
    }
    
    /// <summary>
    /// Check if player has enough of a crafted item
    /// </summary>
    public bool HasCraftedItem(ResourceType itemType, int requiredAmount)
    {
        return GetCraftedItemAmount(itemType) >= requiredAmount;
    }
    
    /// <summary>
    /// Get all crafted items in inventory
    /// </summary>
    public Dictionary<ResourceType, int> GetAllCraftedItems()
    {
        return new Dictionary<ResourceType, int>(craftedItems);
    }
    
    /// <summary>
    /// Check if a resource type is a crafted item
    /// </summary>
    public bool IsCraftedItem(ResourceType resourceType)
    {
        // Add logic to determine if this is a crafted component vs raw resource
        return resourceType switch
        {
            // Layer 1 components
            ResourceType.HullPiece or ResourceType.StructuralBeam or ResourceType.ArmorPlate or
            ResourceType.JointConnector or ResourceType.PowerCell or ResourceType.EnergyConduit or
            ResourceType.PowerRegulator or ResourceType.EnergyCrystal or ResourceType.DataProcessor or
            ResourceType.ServoMotor or ResourceType.SensorArray or ResourceType.LogicCore or
            
            // Layer 2 assemblies  
            ResourceType.WallSection or ResourceType.ReinforcedBulkhead or ResourceType.FloorPanel or
            ResourceType.CeilingAssembly or ResourceType.BatteryBank or ResourceType.PowerDistributionHub or
            ResourceType.QuantumPowerCore or ResourceType.AssemblyRobot or ResourceType.QualityInspector or
            ResourceType.MaterialHandler or
            
            // Layer 3 systems
            ResourceType.SmallShipCompartment or ResourceType.AirlockModule or ResourceType.EngineeringBay or
            ResourceType.PowerPlantModule or ResourceType.DimensionalGenerator or ResourceType.AutomatedFactory or
            ResourceType.ResearchLaboratory => true,
            
            _ => false
        };
    }
    
    private int GetStackLimit(ResourceType itemType)
    {
        return stackLimits.TryGetValue(itemType, out int limit) ? limit : defaultStackSize;
    }
    
    /// <summary>
    /// Get total number of different crafted item types in inventory
    /// </summary>
    public int GetCraftedItemTypeCount()
    {
        return craftedItems.Count;
    }
    
    /// <summary>
    /// Check if inventory has space for new crafted items
    /// </summary>
    public bool HasSpaceForCraftedItem(ResourceType itemType, int amount)
    {
        int currentAmount = GetCraftedItemAmount(itemType);
        int maxStack = GetStackLimit(itemType);
        return (currentAmount + amount) <= maxStack;
    }
} 