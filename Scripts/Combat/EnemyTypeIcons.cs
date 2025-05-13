using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds multiple icon variations for a specific enemy type
/// </summary>
[System.Serializable]
public class EnemyTypeIcons
{
    public EnemyType enemyType;
    public List<Sprite> iconVariations = new List<Sprite>();
} 