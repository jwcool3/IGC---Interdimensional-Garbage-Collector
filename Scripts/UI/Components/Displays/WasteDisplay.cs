using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WasteDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI originText;
    public TextMeshProUGUI stabilityText;
    public Image backgroundImage;
    public Image iconImage;
    public Button recycleButton;

    [Header("Stack Display")]
    public TextMeshProUGUI quantityText;
    public GameObject quantityBackground;

    // Make currentWaste accessible with a public getter
    public WasteItem currentWaste { get; private set; }

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
        }

        // Set origin text
        if (originText != null)
        {
            originText.text = waste.DimensionalOrigin;
        }

        // Set stability text
        if (stabilityText != null)
        {
            stabilityText.text = $"Stability: {waste.WasteStability:P2}";
        }

        // Set icon if available
        if (iconImage != null)
        {
            if (waste.Icon != null)
            {
                iconImage.sprite = waste.Icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        // Set background color based on dimension type
        if (backgroundImage != null)
        {
            Color color = GetColorForDimension(waste.DimensionalOrigin);
            backgroundImage.color = color;
        }

        // Update quantity display
        UpdateQuantity(waste.Quantity);
    }

    // Add the missing UpdateQuantity method
    public void UpdateQuantity(int quantity)
    {
        if (quantityText != null)
        {
            // Only show quantity text for stacks > 1
            if (quantity > 1)
            {
                quantityText.text = $"x{quantity}";
                quantityText.gameObject.SetActive(true);

                // Show background if it exists
                if (quantityBackground != null)
                    quantityBackground.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);

                // Hide background if it exists
                if (quantityBackground != null)
                    quantityBackground.SetActive(false);
            }
        }
    }

    private void RecycleWaste()
    {
        if (currentWaste == null || ResourceManager.Instance == null)
        {
            Debug.LogError("Cannot recycle: currentWaste or ResourceManager.Instance is null!");
            return;
        }

        Debug.Log($"Recycling {currentWaste.Name}");

        // Use the processing method
        ResourceManager.Instance.ProcessWasteItem(currentWaste);

        // Remove from inventory
        if (WasteInventoryManager.Instance != null)
        {
            WasteInventoryManager.Instance.RemoveWasteItem(currentWaste);
        }

        // Destroy the display
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
    }
}