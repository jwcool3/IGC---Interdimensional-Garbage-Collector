using UnityEngine;
using TMPro;

public class ResourceIconTooltip : MonoBehaviour
{
    public void Setup(ResourceType type, ResourceYield yield, ResourceConfig config)
    {
        // Simple setup - you can expand this
        gameObject.name = $"Icon_{type}";
        
        var image = GetComponent<UnityEngine.UI.Image>();
        if (image != null && config != null)
        {
            if (config.icon != null) image.sprite = config.icon;
            image.color = config.resourceColor;
        }
    }
}