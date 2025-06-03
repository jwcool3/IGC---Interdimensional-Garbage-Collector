using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class WasteItemDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI dimensionText;
    [SerializeField] private Image backgroundImage;

    public UpdatedWasteItem CurrentItem { get; private set; }

    public void Initialize(UpdatedWasteItem item)
    {
        CurrentItem = item;
        UpdateDisplay();
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (quantityText != null)
        {
            quantityText.text = newQuantity > 1 ? $"x{newQuantity}" : "";
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

    private void UpdateDisplay()
    {
        if (CurrentItem == null) return;

        // Update icon
        if (iconImage != null)
        {
            iconImage.sprite = CurrentItem.Icon;
            iconImage.gameObject.SetActive(CurrentItem.Icon != null);
        }

        // Update texts
        if (nameText != null)
            nameText.text = CurrentItem.Name;

        if (quantityText != null)
            UpdateQuantity(CurrentItem.Quantity);

        if (rarityText != null)
            rarityText.text = CurrentItem.Rarity.ToString();

        // Update dimensional origin
        if (dimensionText != null)
            dimensionText.text = CurrentItem.DimensionalOrigin;

        // Update background color based on rarity
        if (backgroundImage != null)
        {
            backgroundImage.color = GetRarityColor(CurrentItem.Rarity);
        }
    }

    private Color GetRarityColor(WasteRarity rarity)
    {
        switch (rarity)
        {
            case WasteRarity.Common:
                return new Color(0.8f, 0.8f, 0.8f, 0.5f); // Light gray
            case WasteRarity.Uncommon:
                return new Color(0.4f, 0.8f, 0.4f, 0.5f); // Green
            case WasteRarity.Rare:
                return new Color(0.4f, 0.4f, 0.8f, 0.5f); // Blue
            case WasteRarity.Epic:
                return new Color(0.8f, 0.4f, 0.8f, 0.5f); // Purple
            case WasteRarity.Legendary:
                return new Color(0.8f, 0.6f, 0.2f, 0.5f); // Orange/Gold
            default:
                return new Color(1f, 1f, 1f, 0.5f); // White
        }
    }
}