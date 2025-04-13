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
    [SerializeField] private Image backgroundImage;

    public WasteItem CurrentItem { get; private set; }

    public void Initialize(WasteItem item)
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

        // Update background color based on rarity
        if (backgroundImage != null)
        {
            Color rarityColor = CurrentItem.GetRarityColor();
            rarityColor.a = 0.3f; // Make it semi-transparent
            backgroundImage.color = rarityColor;
        }
    }
}