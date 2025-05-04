using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OpenLocationsButton : MonoBehaviour
{
    private Button button;
    private LocationUI locationUI;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void Start()
    {
        // Use the newer Unity method to find objects
        locationUI = Object.FindFirstObjectByType<LocationUI>();

        if (locationUI == null)
        {
            Debug.LogError("LocationUI not found in scene!");
        }
    }

    private void OnButtonClick()
    {
        if (locationUI != null)
        {
            locationUI.ToggleLocationPanel();
        }
        else
        {
            Debug.LogError("Cannot open locations panel - LocationUI is missing!");
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}