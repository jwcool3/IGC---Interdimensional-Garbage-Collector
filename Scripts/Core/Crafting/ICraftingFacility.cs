using UnityEngine;

/// <summary>
/// Interface for facilities that can be used for crafting
/// </summary>
public interface ICraftingFacility
{
    ProcessingType FacilityType { get; }
    int Level { get; }
    float Efficiency { get; }
    string FacilityName { get; }
    bool IsAvailable { get; }
}

/// <summary>
/// Extension methods to make ProcessingFacilityBase compatible with crafting
/// </summary>
public static class ProcessingFacilityExtensions
{
    public static bool IsAvailable(this ProcessingFacilityBase facility)
    {
        return facility.IsOperational && facility.AvailableSlots > 0;
    }
    
    public static ICraftingFacility AsCraftingFacility(this ProcessingFacilityBase facility)
    {
        return new ProcessingFacilityAdapter(facility);
    }
}

/// <summary>
/// Adapter to make ProcessingFacilityBase work with ICraftingFacility
/// </summary>
public class ProcessingFacilityAdapter : ICraftingFacility
{
    private readonly ProcessingFacilityBase facility;
    
    public ProcessingFacilityAdapter(ProcessingFacilityBase facility)
    {
        this.facility = facility;
    }
    
    public ProcessingType FacilityType => facility.FacilityType;
    public int Level => facility.Level;
    public float Efficiency => facility.Efficiency;
    public string FacilityName => facility.FacilityName;
    public bool IsAvailable => facility.IsOperational && facility.AvailableSlots > 0;
    
    public ProcessingFacilityBase GetFacility() => facility;
} 