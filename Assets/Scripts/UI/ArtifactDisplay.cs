using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArtifactDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI originText;
    [SerializeField] private TextMeshProUGUI stabilityText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Image artifactIcon;
    [SerializeField] private Button researchButton;
    
    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color uncommonColor = Color.green;
    [SerializeField] private Color rareColor = Color.blue;
    [SerializeField] private Color legendaryColor = Color.yellow;

    private ArtifactBase currentArtifact;

    public void Initialize(ArtifactBase artifact)
    {
        currentArtifact = artifact;
        UpdateDisplay();
        
        if (researchButton != null)
            researchButton.onClick.AddListener(OnResearchButtonClicked);
    }

    private void UpdateDisplay()
    {
        if (currentArtifact == null) return;

        // Update text elements
        if (nameText != null)
            nameText.text = currentArtifact.Name;
        
        if (originText != null)
            originText.text = $"Origin: {currentArtifact.DimensionalOrigin}";
        
        if (stabilityText != null)
            stabilityText.text = $"Stability: {currentArtifact.QuantumStability:P0}";
        
        if (rarityText != null)
        {
            rarityText.text = $"Rarity: {GetRarityText()}";
            rarityText.color = GetRarityColor();
        }

        // Update icon (placeholder for now)
        if (artifactIcon != null)
            artifactIcon.color = GetRarityColor();
    }

    private string GetRarityText()
    {
        if (currentArtifact.RarityScore >= 1.5f) return "Legendary";
        if (currentArtifact.RarityScore >= 1.0f) return "Rare";
        if (currentArtifact.RarityScore >= 0.5f) return "Uncommon";
        return "Common";
    }

    private Color GetRarityColor()
    {
        if (currentArtifact.RarityScore >= 1.5f) return legendaryColor;
        if (currentArtifact.RarityScore >= 1.0f) return rareColor;
        if (currentArtifact.RarityScore >= 0.5f) return uncommonColor;
        return commonColor;
    }

    private void OnResearchButtonClicked()
    {
        // Add research functionality here
        Debug.Log($"Researching artifact: {currentArtifact.Name}");
        
        // Example: Add research points based on artifact value
        FacilityManager.Instance.AddDRP(currentArtifact.ResearchValue);
        
        // Example: Add quantum potential based on stability
        float quantumGain = currentArtifact.QuantumStability * 10f;
        FacilityManager.Instance.AddQuantumPotential(quantumGain);
    }

    private void OnDestroy()
    {
        if (researchButton != null)
            researchButton.onClick.RemoveListener(OnResearchButtonClicked);
    }
} 