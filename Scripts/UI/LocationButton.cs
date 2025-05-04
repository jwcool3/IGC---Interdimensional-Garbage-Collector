using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LocationButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI locationNameText;
    [SerializeField] private Image locationIcon;
    [SerializeField] private Image lockIcon;
    [SerializeField] private GameObject selectedIndicator;

    private LocationData locationData;
    private bool isUnlocked;
    private LocationUI locationUI;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        
        button.onClick.AddListener(OnButtonClick);
    }

    public void Initialize(LocationData data, bool unlocked, LocationUI ui)
    {
        locationData = data;
        isUnlocked = unlocked;
        locationUI = ui;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        locationNameText.text = locationData.displayName;
        
        if (locationIcon != null && locationData.locationIcon != null)
            locationIcon.sprite = locationData.locationIcon;

        if (lockIcon != null)
            lockIcon.gameObject.SetActive(!isUnlocked);

        button.interactable = isUnlocked;
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(selected);
    }

    private void OnButtonClick()
    {
        if (isUnlocked && locationUI != null)
        {
            locationUI.SelectLocation(locationData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (locationUI != null)
        {
            locationUI.ShowLocationInfo(locationData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (locationUI != null)
        {
            locationUI.HideLocationInfo();
        }
    }

    // Add this method to fix the error
    public LocationData GetLocationData()
    {
        return locationData;
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnButtonClick);
    }
}