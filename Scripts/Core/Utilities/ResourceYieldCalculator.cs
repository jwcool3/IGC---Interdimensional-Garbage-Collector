using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Utility class for calculating resource yields from waste processing
/// Handles complex yield calculations with various modifiers and bonuses
/// </summary>
public static class ResourceYieldCalculator
{
    /// <summary>
    /// Calculate resource yield from processing a waste item
    /// </summary>
    /// <param name="wasteItem">The waste item being processed</param>
    /// <param name="facilityEfficiency">Processing facility efficiency multiplier</param>
    /// <param name="locationMultiplier">Location-based yield multiplier</param>
    /// <param name="playerSkillBonus">Player skill bonus multiplier</param>
    /// <returns>Dictionary of resource types and amounts</returns>
    public static Dictionary<ResourceType, int> CalculateYield(
        UpdatedWasteItem wasteItem, 
        float facilityEfficiency = 1f, 
        float locationMultiplier = 1f, 
        float playerSkillBonus = 0f)
    {
        if (wasteItem?.resourceYield == null)
        {
            return new Dictionary<ResourceType, int>();
        }
        
        var finalYield = new Dictionary<ResourceType, int>();
        
        // Calculate primary resource yields
        if (wasteItem.resourceYield.primaryResources != null)
        {
            foreach (var primaryResource in wasteItem.resourceYield.primaryResources)
            {
                int amount = CalculateResourceAmount(
                    primaryResource.amount,
                    wasteItem,
                    facilityEfficiency,
                    locationMultiplier,
                    playerSkillBonus
                );
                
                if (amount > 0)
                {
                    if (finalYield.ContainsKey(primaryResource.type))
                    {
                        finalYield[primaryResource.type] += amount;
                    }
                    else
                    {
                        finalYield[primaryResource.type] = amount;
                    }
                }
            }
        }
        
        // Calculate secondary resource yields (chance-based)
        if (wasteItem.resourceYield.secondaryResources != null)
        {
            foreach (var secondaryResource in wasteItem.resourceYield.secondaryResources)
            {
                // Apply chance calculation
                float adjustedChance = CalculateAdjustedChance(
                    secondaryResource.chance,
                    wasteItem,
                    facilityEfficiency,
                    playerSkillBonus
                );
                
                if (Random.value <= adjustedChance)
                {
                    int amount = CalculateResourceAmount(
                        secondaryResource.amount,
                        wasteItem,
                        facilityEfficiency,
                        locationMultiplier,
                        playerSkillBonus
                    );
                    
                    if (amount > 0)
                    {
                        if (finalYield.ContainsKey(secondaryResource.type))
                        {
                            finalYield[secondaryResource.type] += amount;
                        }
                        else
                        {
                            finalYield[secondaryResource.type] = amount;
                        }
                    }
                }
            }
        }
        
        // Apply contamination effects
        ApplyContaminationEffects(finalYield, wasteItem);
        
        // Apply rarity bonuses
        ApplyRarityBonuses(finalYield, wasteItem);
        
        // Apply quantity multiplier
        if (wasteItem.Quantity > 1)
        {
            var keys = finalYield.Keys.ToList();
            foreach (var key in keys)
            {
                finalYield[key] *= wasteItem.Quantity;
            }
        }
        
        return finalYield;
    }
    
    /// <summary>
    /// Calculate the final amount for a specific resource
    /// </summary>
    private static int CalculateResourceAmount(
        int baseAmount,
        UpdatedWasteItem wasteItem,
        float facilityEfficiency,
        float locationMultiplier,
        float playerSkillBonus)
    {
        float amount = baseAmount;
        
        // Apply facility efficiency
        amount *= facilityEfficiency;
        
        // Apply location multiplier
        amount *= locationMultiplier;
        
        // Apply player skill bonus
        amount *= (1f + playerSkillBonus);
        
        // Apply condition modifier
        amount *= GetConditionMultiplier(wasteItem.Condition);
        
        // Apply dimensional stability modifier
        amount *= wasteItem.DimensionalStability;
        
        // Add some randomness (±10%)
        float randomVariation = Random.Range(0.9f, 1.1f);
        amount *= randomVariation;
        
        return Mathf.Max(0, Mathf.RoundToInt(amount));
    }
    
    /// <summary>
    /// Calculate adjusted chance for secondary resources
    /// </summary>
    private static float CalculateAdjustedChance(
        float baseChance,
        UpdatedWasteItem wasteItem,
        float facilityEfficiency,
        float playerSkillBonus)
    {
        float chance = baseChance;
        
        // Facility efficiency can improve chances
        chance *= Mathf.Lerp(1f, 1.2f, (facilityEfficiency - 1f));
        
        // Player skill bonus
        chance += playerSkillBonus * 0.1f;
        
        // Rarity bonus for secondary resources
        switch (wasteItem.Rarity)
        {
            case WasteRarity.Uncommon: chance *= 1.1f; break;
            case WasteRarity.Rare: chance *= 1.25f; break;
            case WasteRarity.Epic: chance *= 1.5f; break;
            case WasteRarity.Legendary: chance *= 2f; break;
        }
        
        // Condition affects chance
        chance *= GetConditionMultiplier(wasteItem.Condition);
        
        return Mathf.Clamp01(chance);
    }
    
    /// <summary>
    /// Get condition-based multiplier
    /// </summary>
    private static float GetConditionMultiplier(WasteCondition condition)
    {
        return condition switch
        {
            WasteCondition.Pristine => 1.3f,
            WasteCondition.Good => 1f,
            WasteCondition.Damaged => 0.8f,
            WasteCondition.Deteriorated => 0.6f,
            WasteCondition.Corrupted => 0.4f,
            _ => 1f
        };
    }
    
    /// <summary>
    /// Apply contamination effects to the yield
    /// </summary>
    private static void ApplyContaminationEffects(Dictionary<ResourceType, int> yield, UpdatedWasteItem wasteItem)
    {
        if (wasteItem.ContaminationLevel > 0f)
        {
            // High contamination can reduce yields
            float contaminationPenalty = 1f - (wasteItem.ContaminationLevel * 0.3f);
            
            var keys = yield.Keys.ToList();
            foreach (var key in keys)
            {
                yield[key] = Mathf.RoundToInt(yield[key] * contaminationPenalty);
            }
            
            // But might produce toxic byproducts
            if (wasteItem.ContaminationLevel > 0.7f && Random.value < 0.3f)
            {
                if (yield.ContainsKey(ResourceType.ToxicSludge))
                {
                    yield[ResourceType.ToxicSludge] += 1;
                }
                else
                {
                    yield[ResourceType.ToxicSludge] = 1;
                }
            }
        }
    }
    
    /// <summary>
    /// Apply rarity-based bonuses
    /// </summary>
    private static void ApplyRarityBonuses(Dictionary<ResourceType, int> yield, UpdatedWasteItem wasteItem)
    {
        // Higher rarity items have a chance for bonus resources
        float bonusChance = wasteItem.Rarity switch
        {
            WasteRarity.Uncommon => 0.1f,
            WasteRarity.Rare => 0.2f,
            WasteRarity.Epic => 0.35f,
            WasteRarity.Legendary => 0.5f,
            _ => 0f
        };
        
        if (bonusChance > 0f && Random.value < bonusChance)
        {
            // Add bonus recycling points
            if (yield.ContainsKey(ResourceType.RecyclingPoints))
            {
                yield[ResourceType.RecyclingPoints] += (int)wasteItem.Rarity;
            }
            else
            {
                yield[ResourceType.RecyclingPoints] = (int)wasteItem.Rarity;
            }
        }
        
        // Legendary items have a chance for dimensional potential
        if (wasteItem.Rarity == WasteRarity.Legendary && Random.value < 0.25f)
        {
            if (yield.ContainsKey(ResourceType.DimensionalPotential))
            {
                yield[ResourceType.DimensionalPotential] += 1;
            }
            else
            {
                yield[ResourceType.DimensionalPotential] = 1;
            }
        }
    }
    
    /// <summary>
    /// Calculate expected yield without randomness (for UI display)
    /// </summary>
    public static Dictionary<ResourceType, float> CalculateExpectedYield(
        UpdatedWasteItem wasteItem,
        float facilityEfficiency = 1f,
        float locationMultiplier = 1f,
        float playerSkillBonus = 0f)
    {
        if (wasteItem?.resourceYield == null)
        {
            return new Dictionary<ResourceType, float>();
        }
        
        var expectedYield = new Dictionary<ResourceType, float>();
        
        // Calculate primary resource expected yields
        if (wasteItem.resourceYield.primaryResources != null)
        {
            foreach (var primaryResource in wasteItem.resourceYield.primaryResources)
            {
                float amount = CalculateExpectedResourceAmount(
                    primaryResource.amount,
                    wasteItem,
                    facilityEfficiency,
                    locationMultiplier,
                    playerSkillBonus
                );
                
                if (expectedYield.ContainsKey(primaryResource.type))
                {
                    expectedYield[primaryResource.type] += amount;
                }
                else
                {
                    expectedYield[primaryResource.type] = amount;
                }
            }
        }
        
        // Calculate secondary resource expected yields
        if (wasteItem.resourceYield.secondaryResources != null)
        {
            foreach (var secondaryResource in wasteItem.resourceYield.secondaryResources)
            {
                float adjustedChance = CalculateAdjustedChance(
                    secondaryResource.chance,
                    wasteItem,
                    facilityEfficiency,
                    playerSkillBonus
                );
                
                float amount = CalculateExpectedResourceAmount(
                    secondaryResource.amount,
                    wasteItem,
                    facilityEfficiency,
                    locationMultiplier,
                    playerSkillBonus
                ) * adjustedChance;
                
                if (expectedYield.ContainsKey(secondaryResource.type))
                {
                    expectedYield[secondaryResource.type] += amount;
                }
                else
                {
                    expectedYield[secondaryResource.type] = amount;
                }
            }
        }
        
        // Apply quantity multiplier
        if (wasteItem.Quantity > 1)
        {
            var keys = expectedYield.Keys.ToList();
            foreach (var key in keys)
            {
                expectedYield[key] *= wasteItem.Quantity;
            }
        }
        
        return expectedYield;
    }
    
    /// <summary>
    /// Calculate expected amount without randomness
    /// </summary>
    private static float CalculateExpectedResourceAmount(
        int baseAmount,
        UpdatedWasteItem wasteItem,
        float facilityEfficiency,
        float locationMultiplier,
        float playerSkillBonus)
    {
        float amount = baseAmount;
        
        // Apply all modifiers except randomness
        amount *= facilityEfficiency;
        amount *= locationMultiplier;
        amount *= (1f + playerSkillBonus);
        amount *= GetConditionMultiplier(wasteItem.Condition);
        amount *= wasteItem.DimensionalStability;
        
        // Apply contamination penalty
        if (wasteItem.ContaminationLevel > 0f)
        {
            float contaminationPenalty = 1f - (wasteItem.ContaminationLevel * 0.3f);
            amount *= contaminationPenalty;
        }
        
        return Mathf.Max(0f, amount);
    }
    
    /// <summary>
    /// Get a summary string of expected yields
    /// </summary>
    public static string GetYieldSummary(UpdatedWasteItem wasteItem, float facilityEfficiency = 1f)
    {
        var expectedYield = CalculateExpectedYield(wasteItem, facilityEfficiency);
        
        if (expectedYield.Count == 0)
        {
            return "No resources";
        }
        
        var summary = expectedYield
            .Where(kvp => kvp.Value > 0.01f)
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => $"{kvp.Value:F1} {kvp.Key}")
            .ToArray();
        
        return string.Join(", ", summary);
    }
} 