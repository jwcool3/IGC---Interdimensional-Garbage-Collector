using UnityEngine;

/// <summary>
/// Defines a specific ship model with its properties
/// </summary>
[System.Serializable]
public class ShipModel
{
    public string modelName;      // Unique identifier (e.g., "ScavengerDrone", "RaiderFrigate")
    public EnemyType shipType;    // The enemy type (Scavenger, Rival, etc.)
    public int minSectorLevel;    // Minimum sector this ship appears in
    public Sprite shipIcon;       // The icon sprite
    [TextArea(1, 3)]
    public string description;    // Optional description
} 