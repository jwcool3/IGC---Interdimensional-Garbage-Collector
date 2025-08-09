using UnityEngine;

[System.Serializable]
public class ShipModel
{
    public string modelName;      // Unique identifier 
    public EnemyType shipType;    // The enemy type (Scavenger, Rival, etc.)
    public int minSectorLevel;    // Minimum sector this ship appears in
    public Sprite shipIcon;       // The icon sprite
    [TextArea(1, 3)]
    public string description;    // Optional description
}