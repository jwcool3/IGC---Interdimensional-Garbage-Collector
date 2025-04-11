using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WasteDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI originText;
    [SerializeField] private TextMeshProUGUI stabilityText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button recycleButton;

    private WasteItem currentWaste;

    private void Start()
    {
        if (recycleButton != null)
        {
            recycleButton.onClick.AddListener(RecycleWaste);
        }
    }

    public void Initialize(WasteItem waste)
    {
        currentWaste = waste;

        if (nameText != null)
            nameText.text = waste.Name;

        if (originText != null)
            originText.text = waste.DimensionalOrigin;

        if (stabilityText != null)
            stabilityText.text = $"Stability: {waste.WasteStability:P2}";

        if (backgroundImage != null)
        {
            // Assign color based on dimensional origin
            backgroundImage.color = GetColorForDimension(waste.DimensionalOrigin);
        }
    }

    private void RecycleWaste()
    {
        if (currentWaste != null && ResourceManager.Instance != null)
        {
            // Calculate resources based on waste properties
            float recyclingValue = currentWaste.RecyclingPotential * 100f;
            float dimensionalValue = currentWaste.WasteStability * 10f;
            float contaminationEffect = currentWaste.ContaminationLevel * 0.05f;
            
            // Add resources
            ResourceManager.Instance.AddRecyclingPoints(recyclingValue);
            ResourceManager.Instance.AddDimensionalPotential(dimensionalValue);
            ResourceManager.Instance.IncreaseContamination(contaminationEffect);
            
            // Destroy the waste item display
            Destroy(gameObject);
        }
    }

    private Color GetColorForDimension(string dimensionType)
    {
        // Return different colors based on dimension type
        if (dimensionType.Contains("Technological"))
            return new Color(0.2f, 0.4f, 0.8f); // Blue
        else if (dimensionType.Contains("Biological"))
            return new Color(0.2f, 0.8f, 0.4f); // Green
        else if (dimensionType.Contains("Quantum"))
            return new Color(0.8f, 0.3f, 0.8f); // Purple
        else if (dimensionType.Contains("Temporal"))
            return new Color(0.8f, 0.6f, 0.2f); // Orange

        return Color.gray; // Default
    }

    private void OnDestroy()
    {
        if (recycleButton != null)
        {
            recycleButton.onClick.RemoveListener(RecycleWaste);
        }
    }
}