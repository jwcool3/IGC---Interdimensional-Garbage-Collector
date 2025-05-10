using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Location", menuName = "Locations/Location Data")]
public class LocationData : ScriptableObject
{
    [Header("Basic Properties")]
    public string locationID;
    public string displayName;
    [TextArea(3, 5)]
    public string description;
    public Sprite locationIcon;
    public Sprite backgroundImage;  // Optional larger background for location screen

    [Header("Unlock Requirements")]
    public int requiredWasteCollected = 0;
    public string[] requiredItems;  // Item IDs needed to unlock
    public LocationData prerequisiteLocation;  // Previous location required

    [Header("Gameplay Properties")]
    [Range(0f, 1f)]
    public float dangerLevel = 0f;
    [Range(0.5f, 2f)]
    public float averageValueMultiplier = 1f;
    [Range(0.5f, 2f)]
    public float discoveryRateMultiplier = 1f;

    [Header("Waste Configuration")]
    public List<string> wasteTypes = new List<string>();  // Dimensional types allowed

    [Header("Rarity Configuration")]
    [Range(0f, 1f)] public float commonChance = 0.7f;
    [Range(0f, 1f)] public float uncommonChance = 0.2f;
    [Range(0f, 1f)] public float rareChance = 0.07f;
    [Range(0f, 1f)] public float epicChance = 0.025f;
    [Range(0f, 1f)] public float legendaryChance = 0.005f;

    [Header("Special Properties")]
    public bool isStartingLocation = false;
    public string[] specialDiscoveryItems;  // Special items that can be found here

    // Discovery properties
    [Header("Discovery Settings")]
    public bool isDiscovered = false;
    public float discoveryChance = 0f;
    public float baseDiscoveryChance = 0.1f; // Base chance before modifiers
}