using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocationListItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI locationNameText;
    [SerializeField] private Image locationThumbnail;
    [SerializeField] private Image selectionIndicator;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Slider dangerIndicator;
    
    private LocationData locationData;
    private bool isUnlocked;
    private LocationSelectionManager selectionManager;
    
    public void Initialize(LocationData data, bool unlocked, LocationSelectionManager manager)
    {
        locationData = data;
        isUnlocked = unlocked;
        selectionManager = manager;
        
        // Setup visuals
        if (locationNameText) locationNameText.text = data.displayName;
        if (locationThumbnail) locationThumbnail.sprite = data.locationIcon;
        if (lockIcon) lockIcon.SetActive(!unlocked);
        if (dangerIndicator) dangerIndicator.value = data.dangerLevel;
        
        // Setup button
        button.onClick.AddListener(OnButtonClicked);
        button.interactable = isUnlocked;
    }
    
    public void SetSelected(bool selected)
    {
        if (selectionIndicator)
            selectionIndicator.gameObject.SetActive(selected);
    }
    
    private void OnButtonClicked()
    {
        if (selectionManager && isUnlocked)
            selectionManager.SelectLocation(locationData);
    }
    
    private void OnDestroy()
    {
        if (button) button.onClick.RemoveAllListeners();
    }
    
    public LocationData GetLocationData() => locationData;
}