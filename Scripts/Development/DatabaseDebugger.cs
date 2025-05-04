using UnityEngine;
using System.Linq;

public class DatabaseDebugger : MonoBehaviour
{
    void Start()
    {
        var database = WasteItemDatabase.Instance;
        if (database != null)
        {
            var allOrigins = database.GetAllDimensionalOrigins();
            Debug.Log($"All dimensional origins in database: [{string.Join(", ", allOrigins)}]");

            // Check for Earth specifically
            var earthItems = database.GetAllItemsByOrigin("Earth");
            Debug.Log($"Items with 'Earth' origin: {earthItems.Count}");

            // Show first few items for each origin
            foreach (var origin in allOrigins)
            {
                var items = database.GetAllItemsByOrigin(origin);
                Debug.Log($"Origin '{origin}' has {items.Count} items");
                if (items.Count > 0)
                {
                    Debug.Log($"  First item: {items[0].itemName}");
                }
            }
        }
    }
}