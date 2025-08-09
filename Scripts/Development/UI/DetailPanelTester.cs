using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailPanelTester : MonoBehaviour
{
    // Reference to the detail panel
    public GameObject detailPanel;
    
    // Reference to text elements
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    
    // Reference to a test button
    public Button testButton;
    
    private bool isPanelVisible = false;
    
    void Start()
    {
        // Add listener to button
        if (testButton != null)
        {
            testButton.onClick.AddListener(TogglePanel);
        }
        else
        {
            Debug.LogError("Test button reference is missing!");
        }
        
        // Initially hide panel
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Detail panel reference is missing!");
        }
    }
    
    public void TogglePanel()
    {
        // Simple toggle functionality
        isPanelVisible = !isPanelVisible;
        
        if (detailPanel != null)
        {
            detailPanel.SetActive(isPanelVisible);
            
            // Update text when showing
            if (isPanelVisible)
            {
                if (nameText != null)
                    nameText.text = "TEST COMPARTMENT";
                
                if (descriptionText != null)
                    descriptionText.text = "This is a test description to verify the panel is working.";
            }
        }
    }
} 