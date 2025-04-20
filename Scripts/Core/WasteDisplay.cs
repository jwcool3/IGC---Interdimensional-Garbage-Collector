using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WasteDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI nameText;        // Make these public for debugging
    public TextMeshProUGUI originText;
    public TextMeshProUGUI stabilityText;
    public Image backgroundImage;
    public Image iconImage;
    public Button recycleButton;

    private WasteItem currentWaste;

    private void Awake()
    {
        // Make sure we have all required components
        if (nameText == null) Debug.LogError("NameText is missing on WasteDisplay prefab!");
        if (originText == null) Debug.LogError("OriginText is missing on WasteDisplay prefab!");
        if (stabilityText == null) Debug.LogError("StabilityText is missing on WasteDisplay prefab!");
        if (iconImage == null) Debug.LogError("IconImage is missing on WasteDisplay prefab!");
        if (backgroundImage == null) Debug.LogError("BackgroundImage is missing on WasteDisplay prefab!");
        if (recycleButton == null) Debug.LogError("RecycleButton is missing on WasteDisplay prefab!");
    }

    private void Start()
    {
        if (recycleButton != null)
        {
            recycleButton.onClick.AddListener(RecycleWaste);
        }
    }

    public void Initialize(WasteItem waste)
    {
        if (waste == null)
        {
            Debug.LogError("Cannot initialize WasteDisplay with null waste item");
            return;
        }

        Debug.Log($"Initializing waste display for item: {waste.Name}, Origin: {waste.DimensionalOrigin}");
        currentWaste = waste;

        // Set name text
        if (nameText != null)
        {
            nameText.text = waste.Name;
            Debug.Log($"Set name text to: {waste.Name}");
        }
        else
        {
            Debug.LogError("nameText is null in WasteDisplay");
        }

        // Set origin text
        if (originText != null)
        {
            originText.text = waste.DimensionalOrigin;
            Debug.Log($"Set origin text to: {waste.DimensionalOrigin}");
        }
        else
        {
            Debug.LogError("originText is null in WasteDisplay");
        }

        // Set stability text
        if (stabilityText != null)
        {
            stabilityText.text = $"Stability: {waste.WasteStability:P2}";
            Debug.Log($"Set stability text to: {waste.WasteStability:P2}");
        }
        else
        {
            Debug.LogError("stabilityText is null in WasteDisplay");
        }

        // Set icon if available
        if (iconImage != null)
        {
            if (waste.Icon != null)
            {
                iconImage.sprite = waste.Icon;
                iconImage.enabled = true;
                Debug.Log($"Set icon image to sprite: {waste.Icon.name}");
            }
            else
            {
                iconImage.enabled = false;
                Debug.Log("No icon available for waste item");
            }
        }
        else
        {
            Debug.LogError("iconImage is null in WasteDisplay");
        }

        // Set background color based on dimension type
        if (backgroundImage != null)
        {
            Color color = GetColorForDimension(waste.DimensionalOrigin);
            backgroundImage.color = color;
            Debug.Log($"Set background color to: {color} for dimension: {waste.DimensionalOrigin}");
        }
        else
        {
            Debug.LogError("backgroundImage is null in WasteDisplay");
        }
    }

    private void RecycleWaste()
    {
        if (currentWaste == null)
        {
            Debug.LogError("Cannot recycle: currentWaste is null!");
            return;
        }

        if (ResourceManager.Instance == null)
        {
            Debug.LogError("Cannot recycle: ResourceManager.Instance is null!");
            return;
        }

        Debug.Log($"Starting recycling process for: {currentWaste.Name}");
        Debug.Log($"Before recycling - RP: {ResourceManager.Instance.RecyclingPoints}, " +
                 $"DP: {ResourceManager.Instance.DimensionalPotential}, " +
                 $"Contamination: {ResourceManager.Instance.ContaminationLevel}");
            
        // Calculate resources based on waste properties
        float recyclingValue = currentWaste.RecyclingPotential * 100f;
        float dimensionalValue = currentWaste.WasteStability * 10f;
        float contaminationEffect = currentWaste.ContaminationLevel * 0.05f;
        
        Debug.Log($"Calculated values - Recycling: {recyclingValue:F1}, " +
                 $"Dimensional: {dimensionalValue:F1}, " +
                 $"Contamination: {contaminationEffect:F1}");
        
        // Add resources
        ResourceManager.Instance.AddRecyclingPoints(recyclingValue);
        ResourceManager.Instance.AddDimensionalPotential(dimensionalValue);
        ResourceManager.Instance.IncreaseContamination(contaminationEffect);
        
        Debug.Log($"After recycling - RP: {ResourceManager.Instance.RecyclingPoints}, " +
                 $"DP: {ResourceManager.Instance.DimensionalPotential}, " +
                 $"Contamination: {ResourceManager.Instance.ContaminationLevel}");
        
        // Remove item from inventory
        if (WasteInventoryManager.Instance != null)
        {
            WasteInventoryManager.Instance.RemoveWasteItem(currentWaste);
            Debug.Log($"Removed {currentWaste.Name} from inventory");
        }
        else
        {
            Debug.LogError("WasteInventoryManager.Instance is null when trying to remove item!");
        }
        
        // Destroy the waste item display
        Debug.Log($"Destroying display for {currentWaste.Name}");
        Destroy(gameObject);
    }

    private Color GetColorForDimension(string dimensionType)
    {
        // Return different colors based on dimension type
        if (string.IsNullOrEmpty(dimensionType))
            return Color.gray;
            
        dimensionType = dimensionType.ToLower();
        
        if (dimensionType.Contains("technological"))
            return new Color(0.2f, 0.4f, 0.8f); // Blue
        else if (dimensionType.Contains("biological"))
            return new Color(0.2f, 0.8f, 0.4f); // Green
        else if (dimensionType.Contains("quantum"))
            return new Color(0.8f, 0.3f, 0.8f); // Purple
        else if (dimensionType.Contains("temporal"))
            return new Color(0.8f, 0.6f, 0.2f); // Orange
        else if (dimensionType.Contains("cosmic"))
            return new Color(0.1f, 0.1f, 0.3f); // Dark blue
        else if (dimensionType.Contains("ethereal"))
            return new Color(0.9f, 0.9f, 1.0f); // Light blue/white
        else if (dimensionType.Contains("philosophical"))
            return new Color(0.5f, 0.3f, 0.7f); // Purple/blue

        return new Color(0.7f, 0.7f, 0.7f); // Default gray
    }

    private void OnDestroy()
    {
        if (recycleButton != null)
        {
            recycleButton.onClick.RemoveListener(RecycleWaste);
        }
    }
    
    public void SetIcon(Sprite icon)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }
        else
        {
            Debug.LogError("iconImage is null in WasteDisplay when trying to set icon");
        }
    }
}