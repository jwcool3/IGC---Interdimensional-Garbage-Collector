using UnityEngine;

public class WasteSystemDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("=== WASTE SYSTEM DEBUG ===");

            // Check current location
            var currentLocation = LocationManager.Instance.GetCurrentLocation();
            Debug.Log($"Current Location: {currentLocation?.displayName}");
            Debug.Log($"Allowed Waste Types: {string.Join(", ", currentLocation?.wasteTypes)}");

            // Check database
            var database = WasteItemDatabase.Instance;
            if (database != null)
            {
                var dimensions = database.GetAllDimensionalOrigins();
                Debug.Log($"Available Dimensions: {string.Join(", ", dimensions)}");

                // Check Earth items
                var earthItems = database.GetAllItemsByOrigin("Earth");
                Debug.Log($"Earth Items Count: {earthItems.Count}");
                if (earthItems.Count > 0)
                {
                    Debug.Log($"First Earth Item: {earthItems[0].itemName}");
                }
            }
        }
    }
}