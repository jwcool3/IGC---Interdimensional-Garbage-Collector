using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WasteDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI originText;
    [SerializeField] private TextMeshProUGUI stabilityText;
    [SerializeField] private Image backgroundImage;

    private WasteItem currentWaste;

    public void Initialize(WasteItem waste)
    {
        currentWaste = waste;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (currentWaste == null) return;

        if (nameText != null)
            nameText.text = currentWaste.Name;
            
        if (originText != null)
            originText.text = currentWaste.DimensionalOrigin;
            
        if (stabilityText != null)
            stabilityText.text = $"Stability: {currentWaste.WasteStability:P0}";
            
        if (backgroundImage != null)
            backgroundImage.color = GetColorForDimension(currentWaste.DimensionalOrigin);
    }
    
    private Color GetColorForDimension(string dimensionType)
    {
        if (dimensionType.Contains("Technological"))
            return new Color(0.2f, 0.4f, 0.8f); // Blue
        else if (dimensionType.Contains("Biological"))
            return new Color(0.2f, 0.8f, 0.4f); // Green
        else if (dimensionType.Contains("Quantum"))
            return new Color(0.8f, 0.3f, 0.8f); // Purple
        else if (dimensionType.Contains("Temporal"))
            return new Color(0.8f, 0.6f, 0.2f); // Orange
        else if (dimensionType.Contains("Philosophical"))
            return new Color(0.7f, 0.7f, 0.7f); // Gray
            
        return Color.gray; // Default
    }
} 