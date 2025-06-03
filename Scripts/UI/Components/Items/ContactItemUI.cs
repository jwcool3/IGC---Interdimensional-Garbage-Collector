using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Controller for individual contact items in the contacts list
/// </summary>
public class ContactItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image shipIcon;
    [SerializeField] private TextMeshProUGUI shipNameText;
    [SerializeField] private TextMeshProUGUI shipTypeText;
    [SerializeField] private Image rarityBadge;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Button selectButton;
    
    [Header("Health Bar")]
    [SerializeField] private Image healthBarBackground;
    [SerializeField] private Image healthBarFill;
    
    [Header("Status Icons")]
    [SerializeField] private Image canFightIcon;
    [SerializeField] private Image canTradeIcon;
    [SerializeField] private Image defeatedIcon;
    
    [Header("Colors")]
    [SerializeField] private Color selectedColor = new Color(0.3f, 0.6f, 1f, 0.5f);
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
    [SerializeField] private Color destroyedColor = new Color(0.5f, 0.2f, 0.2f, 0.8f);
    
    // Private variables
    private DiscoveredShip assignedShip;
    private ContactsUI parentUI;
    private bool isSelected = false;
    
    private void Awake()
    {
        // Auto-find components if not assigned
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
            
        if (selectButton == null)
            selectButton = GetComponentInChildren<Button>();
    }
    
    private void Start()
    {
        // Set up button listener
        if (selectButton != null)
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        else
            Debug.LogError("ContactItemUI: Select button not found!");
    }
    
    /// <summary>
    /// Initialize the contact item with ship data
    /// </summary>
    public void Initialize(DiscoveredShip ship, ContactsUI contactsUI)
    {
        assignedShip = ship;
        parentUI = contactsUI;
        
        UpdateDisplay();
        
        Debug.Log($"ContactItemUI: Initialized for {ship.ShipName}");
    }
    
    /// <summary>
    /// Update the visual display of this contact item
    /// </summary>
    public void UpdateDisplay()
    {
        if (assignedShip == null) return;
        
        // Update ship name
        if (shipNameText != null)
            shipNameText.text = assignedShip.ShipName;
        
        // Update ship type
        if (shipTypeText != null)
            shipTypeText.text = assignedShip.ShipType;
        
        // Update rarity display
        UpdateRarityDisplay();
        
        // Update health bar
        UpdateHealthBar();
        
        // Update status icons
        UpdateStatusIcons();
        
        // Update background color based on status
        UpdateBackgroundColor();
        
        // Update ship icon (placeholder for now)
        UpdateShipIcon();
    }
    
    /// <summary>
    /// Update rarity badge and text
    /// </summary>
    private void UpdateRarityDisplay()
    {
        Color rarityColor = assignedShip.GetRarityColor();
        
        if (rarityBadge != null)
        {
            rarityBadge.color = rarityColor;
        }
        
        if (rarityText != null)
        {
            rarityText.text = assignedShip.GetRarityDisplayText();
            rarityText.color = rarityColor;
        }
    }
    
    /// <summary>
    /// Update health bar display
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float healthPercent = assignedShip.Health > 0 ? 
                assignedShip.CurrentHealth / assignedShip.Health : 0f;
            
            healthBarFill.fillAmount = healthPercent;
            
            // Color the health bar
            if (healthPercent > 0.7f)
                healthBarFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }
        
        if (healthBarBackground != null)
        {
            healthBarBackground.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }
    }
    
    /// <summary>
    /// Update status icons (fight/trade/defeated)
    /// </summary>
    private void UpdateStatusIcons()
    {
        // Can Fight Icon
        if (canFightIcon != null)
        {
            canFightIcon.gameObject.SetActive(assignedShip.CanFight);
            if (assignedShip.CanFight)
                canFightIcon.color = Color.red;
        }
        
        // Can Trade Icon
        if (canTradeIcon != null)
        {
            canTradeIcon.gameObject.SetActive(assignedShip.CanTrade);
            if (assignedShip.CanTrade)
                canTradeIcon.color = Color.green;
        }
        
        // Defeated Icon
        if (defeatedIcon != null)
        {
            defeatedIcon.gameObject.SetActive(assignedShip.IsDestroyed);
            if (assignedShip.IsDestroyed)
                defeatedIcon.color = Color.gray;
        }
    }
    
    /// <summary>
    /// Update background color based on ship status
    /// </summary>
    private void UpdateBackgroundColor()
    {
        if (backgroundImage == null) return;
        
        Color targetColor;
        
        if (isSelected)
            targetColor = selectedColor;
        else if (assignedShip.IsDestroyed)
            targetColor = destroyedColor;
        else
            targetColor = normalColor;
        
        backgroundImage.color = targetColor;
    }
    
    /// <summary>
    /// Update ship icon (placeholder - will be expanded with ship database)
    /// </summary>
    private void UpdateShipIcon()
    {
        if (shipIcon != null)
        {
            // For now, use a colored square based on rarity
            // This will be replaced with actual ship sprites later
            shipIcon.color = assignedShip.GetRarityColor();
            
            // If the ship has an icon assigned, use it
            if (assignedShip.ShipIcon != null)
            {
                shipIcon.sprite = assignedShip.ShipIcon;
                shipIcon.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// Set selection state
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBackgroundColor();
    }
    
    /// <summary>
    /// Handle select button click
    /// </summary>
    private void OnSelectButtonClicked()
    {
        if (parentUI != null && assignedShip != null)
        {
            Debug.Log($"ContactItemUI: Selected {assignedShip.ShipName}");
            parentUI.SelectShip(assignedShip);
            
            // Update selection visuals for all items
            UpdateSelectionVisuals();
        }
    }
    
    /// <summary>
    /// Update selection visuals across all contact items
    /// </summary>
    private void UpdateSelectionVisuals()
    {
        // Find all ContactItemUI components and update their selection state
        ContactItemUI[] allContactItems = FindObjectsOfType<ContactItemUI>();
        
        foreach (var item in allContactItems)
        {
            if (item == this)
                item.SetSelected(true);
            else
                item.SetSelected(false);
        }
    }
    
    /// <summary>
    /// Get the assigned ship (for external access)
    /// </summary>
    public DiscoveredShip GetAssignedShip()
    {
        return assignedShip;
    }
    
    private void OnDestroy()
    {
        // Clean up button listener
        if (selectButton != null)
            selectButton.onClick.RemoveListener(OnSelectButtonClicked);
    }
}