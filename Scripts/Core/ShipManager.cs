using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Main controller for Ship Management system
/// </summary>
public class ShipManager : MonoBehaviour
{
    // Singleton pattern
    public static ShipManager Instance { get; private set; }
    
    // Events
    public event Action<ShipCompartment> OnCompartmentSelected;
    public event Action<ShipCompartment> OnCompartmentUpgraded;
    
    // Ship data
    [SerializeField] private List<ShipCompartment> compartments = new List<ShipCompartment>();
    [SerializeField] private ShipCompartment selectedCompartment;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Register a compartment with the ship manager
    /// </summary>
    public void RegisterCompartment(ShipCompartment compartment)
    {
        if (!compartments.Contains(compartment))
        {
            compartments.Add(compartment);
            Debug.Log($"Registered compartment: {compartment.DisplayName}");
        }
    }
    
    /// <summary>
    /// Get all registered compartments
    /// </summary>
    public List<ShipCompartment> GetAllCompartments()
    {
        return new List<ShipCompartment>(compartments);
    }
    
    /// <summary>
    /// Select a compartment and notify listeners
    /// </summary>
    public void SelectCompartment(ShipCompartment compartment)
    {
        // Deselect current selection
        if (selectedCompartment != null)
        {
            selectedCompartment.SetSelected(false);
        }
        
        // Set new selection
        selectedCompartment = compartment;
        
        // Update visual selection state
        if (selectedCompartment != null)
        {
            selectedCompartment.SetSelected(true);
        }
        
        // Notify listeners
        OnCompartmentSelected?.Invoke(compartment);
    }
    
    /// <summary>
    /// Attempt to upgrade a compartment using available resources
    /// </summary>
    public bool UpgradeCompartment(ShipCompartment compartment)
    {
        if (compartment == null || compartment.CurrentLevel >= compartment.MaxLevel)
            return false;
            
        // Check resource costs
        float rpCost = compartment.GetUpgradeCost(false);
        float dpCost = compartment.GetUpgradeCost(true);
        
        if (ResourceManager.Instance == null ||
            ResourceManager.Instance.RecyclingPoints < rpCost ||
            ResourceManager.Instance.DimensionalPotential < dpCost)
            return false;
            
        // Spend resources
        ResourceManager.Instance.SpendRecyclingPoints(rpCost);
        ResourceManager.Instance.SpendDimensionalPotential(dpCost);
            
        // Upgrade the compartment
        bool success = compartment.UpgradeLevel();
        
        // Notify listeners if successful
        if (success)
        {
            OnCompartmentUpgraded?.Invoke(compartment);
            Debug.Log($"Upgraded {compartment.DisplayName} to level {compartment.CurrentLevel}");
        }
        
        return success;
    }
    
    /// <summary>
    /// Apply effects from all compartments
    /// </summary>
    public void ApplyAllCompartmentEffects()
    {
        foreach (var compartment in compartments)
        {
            compartment.ApplyEffects();
        }
    }
}