using UnityEngine;

public class CompartmentClickTester : MonoBehaviour
{
    // Reference to the ShipCompartment script
    private ShipCompartment shipCompartment;
    
    private void Start()
    {
        // Get the ShipCompartment component
        shipCompartment = GetComponent<ShipCompartment>();
        
        if (shipCompartment == null)
        {
            Debug.LogError("No ShipCompartment found on " + gameObject.name);
        }
    }
    
    private void OnMouseDown()
    {
        Debug.Log("CLICKED ON: " + gameObject.name);
        
        // Try to get the DetailPanel directly
        GameObject detailPanel = GameObject.Find("DetailPanel");
        if (detailPanel != null)
        {
            Debug.Log("Found DetailPanel, activating");
            detailPanel.SetActive(true);
            
            // Try to update text if possible
            Transform nameText = detailPanel.transform.Find("CompartmentNameText");
            if (nameText != null)
            {
                TMPro.TextMeshProUGUI tmp = nameText.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = gameObject.name;
                }
            }
        }
        else
        {
            Debug.LogError("DetailPanel not found!");
        }
    }
} 