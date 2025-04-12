using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradesUI : MonoBehaviour
{
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private GameObject upgradeItemPrefab;
    
    private List<GameObject> upgradeItems = new List<GameObject>();

    private void Start()
    {
        // Subscribe to upgrade events
        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.OnUpgradeCompleted += RefreshUpgradeUI;
            
            // Initial UI population
            PopulateUpgradeUI();
        }
    }
    
    public void PopulateUpgradeUI()
    {
        // Clear existing items
        ClearUpgradeItems();
        
        // Get all available upgrades
        var upgrades = FacilityManager.Instance.GetUpgrades();
        
        // Create UI for each upgrade
        foreach (var upgrade in upgrades)
        {
            GameObject upgradeItemObject = Instantiate(upgradeItemPrefab, upgradeContainer);
            UpgradeItemUI itemUI = upgradeItemObject.GetComponent<UpgradeItemUI>();
            
            if (itemUI != null)
            {
                itemUI.SetUpgrade(upgrade.Value);
                itemUI.SetUpgradeType(upgrade.Key);
            }
            
            upgradeItems.Add(upgradeItemObject);
        }
    }
    
    private void ClearUpgradeItems()
    {
        foreach (var item in upgradeItems)
        {
            Destroy(item);
        }
        
        upgradeItems.Clear();
    }
    
    private void RefreshUpgradeUI(string upgradeName)
    {
        // Refresh all upgrades when one is completed
        PopulateUpgradeUI();
    }
    
    private void OnDestroy()
    {
        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.OnUpgradeCompleted -= RefreshUpgradeUI;
        }
    }
}