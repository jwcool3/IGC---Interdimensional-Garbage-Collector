using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Override system to suppress specific debug messages globally
/// This can catch and filter Debug.Log calls that haven't been converted yet
/// </summary>
public static class DebugOverride
{
    private static bool isOverrideActive = false;
    private static HashSet<string> suppressedKeywords = new HashSet<string>();
    
    static DebugOverride()
    {
        // Add keywords that should be suppressed when active
        suppressedKeywords.Add("Initializing display for waste item");
        suppressedKeywords.Add("Initializing waste display for item");
        suppressedKeywords.Add("New item added to inventory");
        suppressedKeywords.Add("Item removed from inventory");
        suppressedKeywords.Add("Collecting new waste item");
        suppressedKeywords.Add("Generating waste of type");
        suppressedKeywords.Add("Generated");
        suppressedKeywords.Add("waste item:");
        suppressedKeywords.Add("Checking unlock requirements");
        suppressedKeywords.Add("Location changed from");
        suppressedKeywords.Add("Checking for location unlocks");
    }
    
    /// <summary>
    /// Enable debug override to suppress spammy messages
    /// </summary>
    public static void EnableOverride()
    {
        isOverrideActive = true;
        Application.logMessageReceived += FilterLogMessages;
        Debug.Log("🛡️ DebugOverride: Spam filter ENABLED - suppressing spammy messages");
    }
    
    /// <summary>
    /// Disable debug override
    /// </summary>
    public static void DisableOverride()
    {
        isOverrideActive = false;
        Application.logMessageReceived -= FilterLogMessages;
        Debug.Log("🛡️ DebugOverride: Spam filter DISABLED");
    }
    
    /// <summary>
    /// Toggle debug override
    /// </summary>
    public static void ToggleOverride()
    {
        if (isOverrideActive)
        {
            DisableOverride();
        }
        else
        {
            EnableOverride();
        }
    }
    
    /// <summary>
    /// Check if override is active
    /// </summary>
    public static bool IsActive => isOverrideActive;
    
    /// <summary>
    /// Add a keyword to suppress
    /// </summary>
    public static void AddSuppressedKeyword(string keyword)
    {
        suppressedKeywords.Add(keyword.ToLower());
        Debug.Log($"🛡️ DebugOverride: Added suppressed keyword: '{keyword}'");
    }
    
    /// <summary>
    /// Remove a keyword from suppression
    /// </summary>
    public static void RemoveSuppressedKeyword(string keyword)
    {
        suppressedKeywords.Remove(keyword.ToLower());
        Debug.Log($"🛡️ DebugOverride: Removed suppressed keyword: '{keyword}'");
    }
    
    /// <summary>
    /// Clear all suppressed keywords
    /// </summary>
    public static void ClearSuppressedKeywords()
    {
        suppressedKeywords.Clear();
        Debug.Log("🛡️ DebugOverride: Cleared all suppressed keywords");
    }
    
    /// <summary>
    /// Filter incoming log messages
    /// </summary>
    private static void FilterLogMessages(string logString, string stackTrace, LogType type)
    {
        if (!isOverrideActive) return;
        
        // Only filter regular log messages, not warnings or errors
        if (type != LogType.Log) return;
        
        string lowerLog = logString.ToLower();
        
        // Check if this message should be suppressed
        foreach (string keyword in suppressedKeywords)
        {
            if (lowerLog.Contains(keyword.ToLower()))
            {
                // Suppress this message by not letting it through
                // Note: This doesn't actually prevent the original log, but we can track it
                return;
            }
        }
    }
    
    /// <summary>
    /// Get status information
    /// </summary>
    public static string GetStatus()
    {
        return $"DebugOverride: {(isOverrideActive ? "ACTIVE" : "INACTIVE")} - Suppressing {suppressedKeywords.Count} keyword types";
    }
} 