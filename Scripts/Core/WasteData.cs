using System;
using UnityEngine;



[Serializable]
public class WasteData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public WasteRarity Rarity { get; set; }
    public string DimensionalOrigin { get; set; }
    public float WasteStability { get; set; }
    public float ContaminationLevel { get; set; }
    public float RecyclingValue { get; set; }
    public float RecyclingPotential { get; set; }
    public int Quantity { get; set; }
}