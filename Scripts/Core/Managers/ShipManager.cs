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
            InitializeShipManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeShipManager()
    {
        // Find all ShipCompartment components in the scene if list is empty
        if (compartments.Count == 0)
        {
            ShipCompartment[] foundCompartments = FindObjectsOfType<ShipCompartment>();
            foreach (var compartment in foundCompartments)
            {
                RegisterCompartment(compartment);
            }
            Debug.Log($"Found and registered {foundCompartments.Length} compartments");
        }

        // Initialize combat stats
        UpdateCombatStats();

        // Update visuals for all compartments
        UpdateAllCompartmentVisuals();
    }

    /// <summary>
    /// Update visuals for all registered compartments
    /// </summary>
    private void UpdateAllCompartmentVisuals()
    {
        foreach (var compartment in compartments)
        {
            if (compartment != null)
            {
                compartment.UpdateVisuals();
            }
        }
    }

    /// <summary>
    /// Try to upgrade a compartment by its ID
    /// </summary>
    public bool TryUpgradeCompartment(string compartmentId)
    {
        var compartment = compartments.Find(c => c.Id == compartmentId);
        if (compartment == null)
        {
            Debug.LogWarning($"No compartment found with ID: {compartmentId}");
            return false;
        }

        return UpgradeCompartment(compartment);
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
        Debug.Log("ShipManager: SelectCompartment called for " + compartment.DisplayName);

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
        Debug.Log("ShipManager: About to invoke OnCompartmentSelected event");
        OnCompartmentSelected?.Invoke(compartment);
    }

    /// <summary>
    /// Update all combat-related stats from compartments
    /// </summary>
    public void UpdateCombatStats()
    {
        if (CombatManager.Instance == null) return;

        // Reset combat stats to base values
        CombatManager.Instance.attackPower = 10f;
        CombatManager.Instance.defense = 5f;
        CombatManager.Instance.criticalChance = 0.05f;
        CombatManager.Instance.attackSpeed = 1f;

        // Apply effects from all compartments
        foreach (var compartment in compartments)
        {
            if (compartment != null)
            {
                compartment.ApplyEffects();
            }
        }

        Debug.Log("Updated combat stats from all compartments");
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

            // Update combat stats after upgrade
            UpdateCombatStats();
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