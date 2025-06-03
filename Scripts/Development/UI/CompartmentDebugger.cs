using UnityEngine;

public class CompartmentDebugger : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log("DIRECT CLICK DETECTED on " + gameObject.name);
        
        // Try to get the ShipCompartment component
        ShipCompartment compartment = GetComponent<ShipCompartment>();
        if (compartment != null)
        {
            Debug.Log("Found ShipCompartment: " + compartment.DisplayName);
            
            // Try to notify the ShipManager
            if (ShipManager.Instance != null)
            {
                Debug.Log("ShipManager found, selecting compartment");
                ShipManager.Instance.SelectCompartment(compartment);
            }
            else
            {
                Debug.LogError("ShipManager.Instance is null!");
            }
        }
        else
        {
            Debug.LogError("No ShipCompartment component found!");
        }
    }
} 