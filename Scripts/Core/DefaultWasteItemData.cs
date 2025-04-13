using UnityEngine;

[CreateAssetMenu(fileName = "DefaultWasteItem", menuName = "Inventory/Default Waste Item")]
public class DefaultWasteItemData : WasteItemData
{
    private void OnEnable()
    {
        // Set default values when this scriptable object is created
        if (string.IsNullOrEmpty(uniqueIdentifier))
        {
            uniqueIdentifier = System.Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(itemName))
        {
            itemName = "Unknown Waste";
        }

        if (string.IsNullOrEmpty(description))
        {
            description = "An unidentified piece of interdimensional waste.";
        }

        if (string.IsNullOrEmpty(dimensionalOrigin))
        {
            dimensionalOrigin = "Unknown";
        }
    }
}