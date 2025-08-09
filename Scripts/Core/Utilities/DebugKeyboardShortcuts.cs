using UnityEngine;

/// <summary>
/// Simple keyboard shortcuts for debug control without requiring UI setup
/// </summary>
public class DebugKeyboardShortcuts : MonoBehaviour
{
    private void Update()
    {
        // Quick disable all logs
        if (Input.GetKeyDown(KeyCode.F11))
        {
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.SetAllCategories(false);
                Debug.Log("🔇 All debug logs DISABLED (F11)");
            }
            else
            {
                Debug.LogWarning("DebugManager not found!");
            }
        }
        
        // Quick disable spammy categories only (keep errors/warnings)
        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (DebugManager.Instance != null)
            {
                // Disable the most spammy categories
                DebugManager.Instance.ToggleCategory(DebugCategory.WasteGeneration);
                DebugManager.Instance.ToggleCategory(DebugCategory.UIDebug);
                DebugManager.Instance.ToggleCategory(DebugCategory.InventoryManagement);
                DebugManager.Instance.ToggleCategory(DebugCategory.LocationSystem);
                
                // Force them to be OFF
                if (DebugManager.WasteGeneration) DebugManager.Instance.ToggleCategory(DebugCategory.WasteGeneration);
                if (DebugManager.UIDebug) DebugManager.Instance.ToggleCategory(DebugCategory.UIDebug);
                if (DebugManager.InventoryManagement) DebugManager.Instance.ToggleCategory(DebugCategory.InventoryManagement);
                if (DebugManager.LocationSystem) DebugManager.Instance.ToggleCategory(DebugCategory.LocationSystem);
                
                Debug.Log("🤐 Disabled spammy logs (Waste Gen, UI, Inventory, Location) - F12");
            }
        }
        
        // Quick enable errors only
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.SetAllCategories(false);
                Debug.Log("⚠️ Showing ERRORS ONLY (F10)");
            }
            else
            {
                Debug.LogWarning("DebugManager not found!");
            }
        }
        
        // Toggle waste generation logs
        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.ToggleCategory(DebugCategory.WasteGeneration);
                Debug.Log($"🗑️ Waste Generation logs: {(DebugManager.WasteGeneration ? "ON" : "OFF")} (F9)");
            }
        }
        
        // Toggle location system logs
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.ToggleCategory(DebugCategory.LocationSystem);
                Debug.Log($"📍 Location System logs: {(DebugManager.LocationSystem ? "ON" : "OFF")} (F8)");
            }
        }
        
        // Enable all debug logs
        if (Input.GetKeyDown(KeyCode.F7))
        {
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.SetAllCategories(true);
                Debug.Log("🔊 All debug logs ENABLED (F7)");
            }
        }
        
        // Test debug system
        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.Log("🧪 Testing Debug System (F6)");
            DebugManager.Log("Test WasteGeneration log", DebugCategory.WasteGeneration);
            DebugManager.Log("Test LocationSystem log", DebugCategory.LocationSystem);
            DebugManager.LogWarning("Test warning", DebugCategory.GeneralGameplay);
            DebugManager.LogError("Test error", DebugCategory.GeneralGameplay);
            Debug.Log("Debug test complete!");
        }
        
        // Show help
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("🎮 DEBUG SHORTCUTS:\n" +
                     "F4 - Toggle spam filter (blocks old Debug.Log calls)\n" +
                     "F5 - Show this help\n" +
                     "F6 - Test debug system\n" +
                     "F7 - Enable ALL logs\n" +
                     "F8 - Toggle Location logs\n" +
                     "F9 - Toggle Waste Generation logs\n" +
                     "F10 - Errors only\n" +
                     "F11 - Disable ALL logs\n" +
                     "F12 - Disable SPAMMY logs (recommended!)");
        }
        
        // Toggle debug override spam filter
        if (Input.GetKeyDown(KeyCode.F4))
        {
            DebugOverride.ToggleOverride();
            Debug.Log($"🛡️ Debug spam filter: {(DebugOverride.IsActive ? "ON" : "OFF")} (F4)");
        }
    }
}

/// <summary>
/// Auto-adds debug shortcuts to any scene that doesn't have them
/// </summary>
[System.Serializable]
public class DebugShortcutsAutoSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSetupDebugShortcuts()
    {
        // Check if debug shortcuts already exist
        if (Object.FindFirstObjectByType<DebugKeyboardShortcuts>() == null)
        {
            // Create a GameObject with debug shortcuts
            GameObject debugObj = new GameObject("DebugShortcuts");
            debugObj.AddComponent<DebugKeyboardShortcuts>();
            
            // Make it persistent across scenes
            Object.DontDestroyOnLoad(debugObj);
            
            Debug.Log("🎮 Debug keyboard shortcuts auto-setup complete! Press F5 for help.");
        }
    }
} 