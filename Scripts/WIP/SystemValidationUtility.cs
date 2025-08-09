using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Utility class for validating the resource management system
/// Provides comprehensive checks for data integrity, performance, and migration verification
/// </summary>
public static class SystemValidationUtility
{
    /// <summary>
    /// Perform a comprehensive system validation
    /// </summary>
    /// <returns>Complete validation report</returns>
    public static SystemValidationReport ValidateCompleteSystem()
    {
        var report = new SystemValidationReport
        {
            timestamp = DateTime.Now,
            validationStartTime = Time.realtimeSinceStartup
        };
        
        // Validate individual components
        report.resourceManagerValidation = ValidateResourceManager();
        report.inventoryValidation = ValidateInventorySystem();
        report.processingValidation = ValidateProcessingSystem();
        report.bridgeValidation = ValidateBridgeSystem();
        report.configurationValidation = ValidateConfigurations();
        report.eventSystemValidation = ValidateEventSystem();
        
        // Calculate overall status
        report.overallStatus = CalculateOverallStatus(report);
        report.validationEndTime = Time.realtimeSinceStartup;
        report.validationDuration = report.validationEndTime - report.validationStartTime;
        
        return report;
    }
    
    /// <summary>
    /// Validate the resource manager system
    /// </summary>
    /// <returns>Resource manager validation result</returns>
    public static ValidationResult ValidateResourceManager()
    {
        var result = new ValidationResult { componentName = "ResourceManager" };
        var issues = new List<string>();
        
        try
        {
            var newManager = ResourceManager.Instance;
            if (newManager == null)
            {
                issues.Add("ResourceManager instance not found");
                result.status = ValidationStatus.Critical;
                result.issues = issues;
                return result;
            }
            
            // Validate resource storage
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                try
                {
                    int amount = newManager.GetResourceAmount(type);
                    if (amount < 0)
                    {
                        issues.Add($"Negative resource amount for {type}: {amount}");
                    }
                    
                    int capacity = newManager.GetStorageCapacity(type);
                    if (capacity <= 0)
                    {
                        issues.Add($"Invalid storage capacity for {type}: {capacity}");
                    }
                    
                    if (amount > capacity)
                    {
                        issues.Add($"Resource amount exceeds capacity for {type}: {amount}/{capacity}");
                    }
                }
                catch (Exception ex)
                {
                    issues.Add($"Error accessing {type}: {ex.Message}");
                }
            }
            
            // Check resource operations
            var testResources = new Dictionary<ResourceType, int>
            {
                { ResourceType.Plastic, 10 },
                { ResourceType.MetalScraps, 5 }
            };
            
            foreach (var kvp in testResources)
            {
                try
                {
                    bool canAdd = newManager.CanAddResource(kvp.Key, kvp.Value);
                    bool canSpend = newManager.CanSpendResource(kvp.Key, 1);
                    
                    if (!canAdd && newManager.GetResourceAmount(kvp.Key) + kvp.Value <= newManager.GetStorageCapacity(kvp.Key))
                    {
                        issues.Add($"CanAddResource failed unexpectedly for {kvp.Key}");
                    }
                }
                catch (Exception ex)
                {
                    issues.Add($"Error testing resource operations for {kvp.Key}: {ex.Message}");
                }
            }
            
            result.status = issues.Count == 0 ? ValidationStatus.Passed : 
                           issues.Any(i => i.Contains("Critical") || i.Contains("Error")) ? ValidationStatus.Critical : ValidationStatus.Warning;
        }
        catch (Exception ex)
        {
            issues.Add($"Critical error during resource manager validation: {ex.Message}");
            result.status = ValidationStatus.Critical;
        }
        
        result.issues = issues;
        return result;
    }
    
    /// <summary>
    /// Validate the inventory system
    /// </summary>
    /// <returns>Inventory validation result</returns>
    public static ValidationResult ValidateInventorySystem()
    {
        var result = new ValidationResult { componentName = "InventorySystem" };
        var issues = new List<string>();
        
        try
        {
            // Check legacy inventory manager
            var legacyInventory = WasteInventoryManager.Instance;
            if (legacyInventory != null)
            {
                var wasteItems = legacyInventory.GetAllWaste();
                
                foreach (var item in wasteItems)
                {
                    if (item == null)
                    {
                        issues.Add("Found null waste item in inventory");
                        continue;
                    }
                    
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        issues.Add($"Waste item has empty ID: {item.Name}");
                    }
                    
                    if (item.Quantity <= 0)
                    {
                        issues.Add($"Waste item has invalid quantity: {item.Name} ({item.Quantity})");
                    }
                    
                    if (item.RecyclingValue < 0)
                    {
                        issues.Add($"Waste item has negative recycling value: {item.Name} ({item.RecyclingValue})");
                    }
                }
                
                // Check inventory capacity
                int currentCount = wasteItems.Count;
                int maxCapacity = legacyInventory.GetMaxCapacity();
                
                if (currentCount > maxCapacity)
                {
                    issues.Add($"Inventory exceeds capacity: {currentCount}/{maxCapacity}");
                }
            }
            
            result.status = issues.Count == 0 ? ValidationStatus.Passed : ValidationStatus.Warning;
        }
        catch (Exception ex)
        {
            issues.Add($"Critical error during inventory validation: {ex.Message}");
            result.status = ValidationStatus.Critical;
        }
        
        result.issues = issues;
        return result;
    }
    
    /// <summary>
    /// Validate the processing system
    /// </summary>
    /// <returns>Processing validation result</returns>
    public static ValidationResult ValidateProcessingSystem()
    {
        var result = new ValidationResult { componentName = "ProcessingSystem" };
        var issues = new List<string>();
        
        try
        {
            var configManager = ResourceConfigManager.Instance;
            if (configManager != null)
            {
                var recipes = configManager.GetAllProcessingRecipes();
                
                foreach (var recipe in recipes)
                {
                    if (recipe == null)
                    {
                        issues.Add("Found null processing recipe");
                        continue;
                    }
                    
                    var validation = recipe.ValidateRecipe();
                    if (!validation.isValid)
                    {
                        issues.Add($"Recipe validation failed for {recipe.recipeName}: {validation.errorMessage}");
                    }
                    
                    // Check for circular dependencies
                    if (HasCircularDependency(recipe, recipes))
                    {
                        issues.Add($"Circular dependency detected in recipe: {recipe.recipeName}");
                    }
                }
                
                // Check for orphaned resources (resources that can't be produced)
                var orphanedResources = FindOrphanedResources(recipes);
                foreach (var orphan in orphanedResources)
                {
                    issues.Add($"Orphaned resource (no production recipe): {orphan}");
                }
            }
            
            result.status = issues.Count == 0 ? ValidationStatus.Passed : ValidationStatus.Warning;
        }
        catch (Exception ex)
        {
            issues.Add($"Critical error during processing validation: {ex.Message}");
            result.status = ValidationStatus.Critical;
        }
        
        result.issues = issues;
        return result;
    }
    
    /// <summary>
    /// Validate the bridge system
    /// </summary>
    /// <returns>Bridge validation result</returns>
    public static ValidationResult ValidateBridgeSystem()
    {
        var result = new ValidationResult { componentName = "BridgeSystem" };
        var issues = new List<string>();
        
        try
        {
            var bridge = ResourceManagerBridge.Instance;
            if (bridge == null)
            {
                issues.Add("ResourceManagerBridge instance not found");
                result.status = ValidationStatus.Critical;
                result.issues = issues;
                return result;
            }
            
            // Test bridge operations
            float initialRP = bridge.RecyclingPoints;
            float initialDP = bridge.DimensionalPotential;
            
            // Test adding resources
            bridge.AddRecyclingPoints(10f);
            if (bridge.RecyclingPoints != initialRP + 10f)
            {
                issues.Add("Bridge recycling points addition failed");
            }
            
            bridge.AddDimensionalPotential(5f);
            if (bridge.DimensionalPotential != initialDP + 5f)
            {
                issues.Add("Bridge dimensional potential addition failed");
            }
            
            // Test spending resources
            bool spendSuccess = bridge.SpendRecyclingPoints(5f);
            if (!spendSuccess && bridge.RecyclingPoints >= 5f)
            {
                issues.Add("Bridge recycling points spending failed unexpectedly");
            }
            
            // Restore original values
            bridge.SetRecyclingPoints(initialRP);
            bridge.SetDimensionalPotential(initialDP);
            
            result.status = issues.Count == 0 ? ValidationStatus.Passed : ValidationStatus.Warning;
        }
        catch (Exception ex)
        {
            issues.Add($"Critical error during bridge validation: {ex.Message}");
            result.status = ValidationStatus.Critical;
        }
        
        result.issues = issues;
        return result;
    }
    
    /// <summary>
    /// Validate system configurations
    /// </summary>
    /// <returns>Configuration validation result</returns>
    public static ValidationResult ValidateConfigurations()
    {
        var result = new ValidationResult { componentName = "Configurations" };
        var issues = new List<string>();
        
        try
        {
            var configManager = ResourceConfigManager.Instance;
            if (configManager == null)
            {
                issues.Add("ResourceConfigManager instance not found");
                result.status = ValidationStatus.Critical;
                result.issues = issues;
                return result;
            }
            
            var stats = configManager.GetConfigurationStats();
            if (!stats.isLoaded)
            {
                issues.Add("Configurations not loaded");
                result.status = ValidationStatus.Critical;
                result.issues = issues;
                return result;
            }
            
            // Check if all resource types have configurations
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                if (!configManager.HasResourceConfig(type))
                {
                    issues.Add($"Missing configuration for resource type: {type}");
                }
            }
            
            // Validate conversion rates
            var conversionIssues = ValidateConversionRates();
            issues.AddRange(conversionIssues);
            
            result.status = issues.Count == 0 ? ValidationStatus.Passed : ValidationStatus.Warning;
        }
        catch (Exception ex)
        {
            issues.Add($"Critical error during configuration validation: {ex.Message}");
            result.status = ValidationStatus.Critical;
        }
        
        result.issues = issues;
        return result;
    }
    
    /// <summary>
    /// Validate the event system
    /// </summary>
    /// <returns>Event system validation result</returns>
    public static ValidationResult ValidateEventSystem()
    {
        var result = new ValidationResult { componentName = "EventSystem" };
        var issues = new List<string>();
        
        try
        {
            var eventBridge = ResourceEventBridge.Instance;
            if (eventBridge == null)
            {
                issues.Add("ResourceEventBridge instance not found");
                result.status = ValidationStatus.Warning;
                result.issues = issues;
                return result;
            }
            
            var status = eventBridge.GetStatus();
            
            if (!status.legacyEventsEnabled && !status.newEventsEnabled)
            {
                issues.Add("Both legacy and new events are disabled");
            }
            
            if (status.pendingBatchedEvents > 100)
            {
                issues.Add($"High number of pending batched events: {status.pendingBatchedEvents}");
            }
            
            result.status = issues.Count == 0 ? ValidationStatus.Passed : ValidationStatus.Warning;
        }
        catch (Exception ex)
        {
            issues.Add($"Critical error during event system validation: {ex.Message}");
            result.status = ValidationStatus.Critical;
        }
        
        result.issues = issues;
        return result;
    }
    
    /// <summary>
    /// Validate migration data integrity
    /// </summary>
    /// <param name="originalRP">Original recycling points</param>
    /// <param name="originalDP">Original dimensional potential</param>
    /// <param name="migratedResources">Migrated resources</param>
    /// <returns>Migration validation result</returns>
    public static MigrationValidationResult ValidateMigration(
        float originalRP, 
        float originalDP, 
        Dictionary<ResourceType, int> migratedResources)
    {
        var result = new MigrationValidationResult
        {
            originalRecyclingPoints = originalRP,
            originalDimensionalPotential = originalDP,
            migratedResources = new Dictionary<ResourceType, int>(migratedResources)
        };
        
        try
        {
            // Calculate value preservation
            float originalValue = originalRP + (originalDP * 2f);
            float migratedValue = LegacyResourceConverter.CalculateResourceValue(migratedResources);
            
            result.originalTotalValue = originalValue;
            result.migratedTotalValue = migratedValue;
            result.valueDifference = migratedValue - originalValue;
            result.valueDifferencePercent = originalValue > 0 ? (result.valueDifference / originalValue) * 100f : 0f;
            
            // Check if migration is within acceptable tolerance
            result.isWithinTolerance = LegacyResourceConverter.ValidateConversion(originalRP, originalDP, migratedResources, 0.1f);
            
            // Validate individual resource amounts
            foreach (var kvp in migratedResources)
            {
                if (kvp.Value < 0)
                {
                    result.issues.Add($"Negative resource amount after migration: {kvp.Key} = {kvp.Value}");
                }
            }
            
            result.isValid = result.isWithinTolerance && result.issues.Count == 0;
        }
        catch (Exception ex)
        {
            result.issues.Add($"Migration validation error: {ex.Message}");
            result.isValid = false;
        }
        
        return result;
    }
    
    #region Helper Methods
    
    private static ValidationStatus CalculateOverallStatus(SystemValidationReport report)
    {
        var statuses = new[]
        {
            report.resourceManagerValidation.status,
            report.inventoryValidation.status,
            report.processingValidation.status,
            report.bridgeValidation.status,
            report.configurationValidation.status,
            report.eventSystemValidation.status
        };
        
        if (statuses.Any(s => s == ValidationStatus.Critical))
            return ValidationStatus.Critical;
        
        if (statuses.Any(s => s == ValidationStatus.Warning))
            return ValidationStatus.Warning;
        
        return ValidationStatus.Passed;
    }
    
    private static bool HasCircularDependency(ProcessingRecipeData recipe, List<ProcessingRecipeData> allRecipes)
    {
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        
        return HasCircularDependencyRecursive(recipe.recipeName, allRecipes, visited, recursionStack);
    }
    
    private static bool HasCircularDependencyRecursive(
        string recipeName, 
        List<ProcessingRecipeData> allRecipes, 
        HashSet<string> visited, 
        HashSet<string> recursionStack)
    {
        if (recursionStack.Contains(recipeName))
            return true;
        
        if (visited.Contains(recipeName))
            return false;
        
        visited.Add(recipeName);
        recursionStack.Add(recipeName);
        
        var recipe = allRecipes.FirstOrDefault(r => r.recipeName == recipeName);
        if (recipe != null)
        {
            // Check if any output of this recipe is used as input in other recipes
            // Handle guaranteed outputs (ResourceAmount[])
            if (recipe.guaranteedOutputs != null)
            {
                foreach (var output in recipe.guaranteedOutputs)
                {
                    var dependentRecipes = allRecipes.Where(r => 
                        r.requiredInputs.Any(input => input.type == output.type));
                    
                    foreach (var dependentRecipe in dependentRecipes)
                    {
                        if (HasCircularDependencyRecursive(dependentRecipe.recipeName, allRecipes, visited, recursionStack))
                            return true;
                    }
                }
            }
            
            // Handle possible outputs (ResourceChance[])
            if (recipe.possibleOutputs != null)
            {
                foreach (var output in recipe.possibleOutputs)
                {
                    var dependentRecipes = allRecipes.Where(r => 
                        r.requiredInputs.Any(input => input.type == output.type));
                    
                    foreach (var dependentRecipe in dependentRecipes)
                    {
                        if (HasCircularDependencyRecursive(dependentRecipe.recipeName, allRecipes, visited, recursionStack))
                            return true;
                    }
                }
            }
        }
        
        recursionStack.Remove(recipeName);
        return false;
    }
    
    private static List<ResourceType> FindOrphanedResources(List<ProcessingRecipeData> recipes)
    {
        var producedResources = new HashSet<ResourceType>();
        var consumedResources = new HashSet<ResourceType>();
        
        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;
            
            // Handle guaranteed outputs (ResourceAmount[])
            if (recipe.guaranteedOutputs != null)
            {
                foreach (var output in recipe.guaranteedOutputs)
                {
                    producedResources.Add(output.type);
                }
            }
            
            // Handle possible outputs (ResourceChance[])
            if (recipe.possibleOutputs != null)
            {
                foreach (var output in recipe.possibleOutputs)
                {
                    producedResources.Add(output.type);
                }
            }
            
            // Handle inputs
            if (recipe.requiredInputs != null)
            {
                foreach (var input in recipe.requiredInputs)
                {
                    consumedResources.Add(input.type);
                }
            }
        }
        
        // Find resources that are consumed but never produced
        var orphaned = new List<ResourceType>();
        foreach (var consumed in consumedResources)
        {
            if (!producedResources.Contains(consumed))
            {
                orphaned.Add(consumed);
            }
        }
        
        return orphaned;
    }
    
    private static List<string> ValidateConversionRates()
    {
        var issues = new List<string>();
        
        try
        {
            // Test conversion rate consistency
            var testRP = 100f;
            var testDP = 50f;
            
            var convertedResources = LegacyResourceConverter.GetOptimalConversion(testRP, testDP);
            var backConvertedRP = LegacyResourceConverter.ConvertToRecyclingPoints(convertedResources);
            var backConvertedDP = LegacyResourceConverter.ConvertToDimensionalPotential(convertedResources);
            
            float rpDifference = Mathf.Abs(testRP - backConvertedRP);
            float dpDifference = Mathf.Abs(testDP - backConvertedDP);
            
            if (rpDifference > testRP * 0.1f)
            {
                issues.Add($"RP conversion rate inconsistency: {rpDifference:F2} difference");
            }
            
            if (dpDifference > testDP * 0.1f)
            {
                issues.Add($"DP conversion rate inconsistency: {dpDifference:F2} difference");
            }
        }
        catch (Exception ex)
        {
            issues.Add($"Conversion rate validation error: {ex.Message}");
        }
        
        return issues;
    }
    
    #endregion
}

/// <summary>
/// Validation status levels
/// </summary>
public enum ValidationStatus
{
    Passed,
    Warning,
    Critical
}

/// <summary>
/// Individual component validation result
/// </summary>
[System.Serializable]
public class ValidationResult
{
    public string componentName;
    public ValidationStatus status;
    public List<string> issues = new List<string>();
    
    public override string ToString()
    {
        return $"{componentName}: {status} ({issues.Count} issues)";
    }
}

/// <summary>
/// Complete system validation report
/// </summary>
[System.Serializable]
public class SystemValidationReport
{
    public DateTime timestamp;
    public float validationStartTime;
    public float validationEndTime;
    public float validationDuration;
    
    public ValidationStatus overallStatus;
    public ValidationResult resourceManagerValidation;
    public ValidationResult inventoryValidation;
    public ValidationResult processingValidation;
    public ValidationResult bridgeValidation;
    public ValidationResult configurationValidation;
    public ValidationResult eventSystemValidation;
    
    public int TotalIssues => 
        resourceManagerValidation.issues.Count +
        inventoryValidation.issues.Count +
        processingValidation.issues.Count +
        bridgeValidation.issues.Count +
        configurationValidation.issues.Count +
        eventSystemValidation.issues.Count;
    
    public override string ToString()
    {
        return $"System Validation Report:\n" +
               $"Overall Status: {overallStatus}\n" +
               $"Total Issues: {TotalIssues}\n" +
               $"Validation Duration: {validationDuration:F2}s\n" +
               $"Timestamp: {timestamp}";
    }
}

/// <summary>
/// Migration-specific validation result
/// </summary>
[System.Serializable]
public class MigrationValidationResult
{
    public float originalRecyclingPoints;
    public float originalDimensionalPotential;
    public float originalTotalValue;
    public Dictionary<ResourceType, int> migratedResources;
    public float migratedTotalValue;
    public float valueDifference;
    public float valueDifferencePercent;
    public bool isWithinTolerance;
    public bool isValid;
    public List<string> issues = new List<string>();
    
    public override string ToString()
    {
        return $"Migration Validation:\n" +
               $"Original: {originalRecyclingPoints} RP, {originalDimensionalPotential} DP\n" +
               $"Value Difference: {valueDifferencePercent:F1}%\n" +
               $"Valid: {isValid}, Within Tolerance: {isWithinTolerance}\n" +
               $"Issues: {issues.Count}";
    }
} 