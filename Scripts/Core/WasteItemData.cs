using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Waste Item", menuName = "Inventory/Waste Item Data")]
public class WasteItemData : ScriptableObject
{
    public string itemName;
    public string uniqueIdentifier; // matches filename
    public string description;
    public WasteRarity defaultRarity = WasteRarity.Common;
    
    [Header("Base Properties")]
    public float baseStability = 0.5f;
    public float baseContamination = 0.2f;
    public float baseRecyclingPotential = 0.3f;
    
    [Header("Dimensional Properties")]
    public string dimensionalOrigin;
    
    [Header("Sprites")]
    public Sprite[] itemSprites; // Multiple sprites for stacked items

    private void OnValidate()
    {
        // Ensure uniqueIdentifier is set when the asset is created
        if (string.IsNullOrEmpty(uniqueIdentifier))
        {
            uniqueIdentifier = Guid.NewGuid().ToString();
        }
    }
} 