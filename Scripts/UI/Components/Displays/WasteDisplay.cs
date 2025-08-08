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
    public UpdatedWasteItem currentWaste { get; private set; }

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

    public void Initialize(UpdatedWasteItem waste)
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
            stabilityText.text = $"Stability: {waste.DimensionalStability:P0}";
        }

        // Set quantity text
        if (quantityText != null)
        {
            quantityText.text = waste.Quantity > 1 ? $"x{waste.Quantity}" : "";
            
            // Show/hide quantity background based on whether we have multiple items
            if (quantityBackground != null)
            {
                quantityBackground.SetActive(waste.Quantity > 1);
            }
        }

        // Set background color based on rarity
        if (backgroundImage != null)
        {
            backgroundImage.color = GetColorForRarity(waste.Rarity);
        }

        // Set icon if available
        if (iconImage != null && waste.Icon != null)
        {
            iconImage.sprite = waste.Icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }
    }

    public void SetIcon(Sprite icon)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (currentWaste != null)
        {
            currentWaste.SetQuantity(newQuantity);
        }

        if (quantityText != null)
        {
            quantityText.text = newQuantity > 1 ? $"x{newQuantity}" : "";
        }

        if (quantityBackground != null)
        {
            quantityBackground.SetActive(newQuantity > 1);
        }
    }

    private Color GetColorForRarity(WasteRarity rarity)
    {
        switch (rarity)
        {
            case WasteRarity.Common:
                return new Color(0.8f, 0.8f, 0.8f); // Light gray
            case WasteRarity.Uncommon:
                return new Color(0.4f, 0.8f, 0.4f); // Green
            case WasteRarity.Rare:
                return new Color(0.4f, 0.4f, 0.8f); // Blue
            case WasteRarity.Epic:
                return new Color(0.8f, 0.4f, 0.8f); // Purple
            case WasteRarity.Legendary:
                return new Color(0.8f, 0.6f, 0.2f); // Orange/Gold
            default:
                return Color.white;
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

        // Convert to WasteItem for processing
        WasteItem wasteItem = currentWaste.ToWasteItem();
        ResourceManager.Instance.ProcessWasteItem(wasteItem);

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
}