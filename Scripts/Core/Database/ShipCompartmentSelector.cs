using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ShipCompartmentSelector : MonoBehaviour
{
    [Header("Required References")]
    public Camera mainCamera;
    public GameObject detailPanel;

    [Header("Optional References")]
    public List<SpriteRenderer> compartmentRenderers = new List<SpriteRenderer>();

    [Header("Text Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI nextLevelPreviewText;
    public TextMeshProUGUI costText;

    private ShipCompartment selectedCompartment;

    void Start()
    {
        // Auto-assign camera if not set
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Auto-find detail panel if not set
        if (detailPanel == null)
            detailPanel = GameObject.Find("DetailPanel");

        // Auto-find text elements if not set
        if (nameText == null && detailPanel != null)
            nameText = detailPanel.transform.Find("CompartmentNameText")?.GetComponent<TextMeshProUGUI>();

        if (descriptionText == null && detailPanel != null)
            descriptionText = detailPanel.transform.Find("CompartmentDescriptionText")?.GetComponent<TextMeshProUGUI>();

        if (levelText == null && detailPanel != null)
            levelText = detailPanel.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();

        if (effectText == null && detailPanel != null)
            effectText = detailPanel.transform.Find("EffectText")?.GetComponent<TextMeshProUGUI>();

        if (nextLevelPreviewText == null && detailPanel != null)
            nextLevelPreviewText = detailPanel.transform.Find("NextLevelPreviewText")?.GetComponent<TextMeshProUGUI>();

        if (costText == null && detailPanel != null)
            costText = detailPanel.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();

        // Auto-populate compartment renderers if list is empty
        if (compartmentRenderers.Count == 0)
        {
            // Find all ShipCompartment components
            ShipCompartment[] allCompartments = FindObjectsOfType<ShipCompartment>();

            foreach (var compartment in allCompartments)
            {
                SpriteRenderer renderer = compartment.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    compartmentRenderers.Add(renderer);
                }
            }

            Debug.Log($"Auto-found {compartmentRenderers.Count} compartment renderers");
        }

        // Initially hide detail panel
        if (detailPanel != null)
            detailPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

            bool clickedOnCompartment = false;

            foreach (var renderer in compartmentRenderers)
            {
                if (renderer == null) continue;

                Bounds bounds = renderer.bounds;
                if (bounds.Contains(worldPosition))
                {
                    clickedOnCompartment = true;
                    ShipCompartment compartment = renderer.GetComponent<ShipCompartment>();

                    if (compartment != null)
                    {
                        SelectCompartment(compartment);
                    }
                    else
                    {
                        Debug.LogWarning("SpriteRenderer doesn't have a ShipCompartment component: " + renderer.gameObject.name);
                    }

                    break;
                }
            }

            // Optionally hide panel when clicking outside compartments
            // if (!clickedOnCompartment && detailPanel != null)
            //    detailPanel.SetActive(false);
        }
    }

    void SelectCompartment(ShipCompartment compartment)
    {
        Debug.Log("Selected compartment: " + compartment.DisplayName);

        // Store the selection
        selectedCompartment = compartment;

        // Show the detail panel
        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
        }

        // Update selection indicators
        foreach (var renderer in compartmentRenderers)
        {
            ShipCompartment comp = renderer?.GetComponent<ShipCompartment>();
            if (comp != null)
            {
                // Get the selection indicator child
                Transform indicator = comp.transform.Find("SelectionIndicator");
                if (indicator != null)
                {
                    // Show only for the selected compartment
                    indicator.gameObject.SetActive(comp == compartment);
                }
            }
        }

        // Update panel content
        UpdateDetailPanel();

        // Also notify the ShipManager if it exists
        if (ShipManager.Instance != null)
        {
            ShipManager.Instance.SelectCompartment(compartment);
        }
    }

    void UpdateDetailPanel()
    {
        if (selectedCompartment == null) return;

        // Update text elements
        if (nameText != null)
            nameText.text = selectedCompartment.DisplayName;

        if (descriptionText != null)
            descriptionText.text = selectedCompartment.Description;

        if (levelText != null)
            levelText.text = $"Level: {selectedCompartment.CurrentLevel}/{selectedCompartment.MaxLevel}";

        if (effectText != null)
        {
            string effectDescription = GetEffectDescription(selectedCompartment);
            effectText.text = $"Current Effect: {effectDescription}";
        }

        if (nextLevelPreviewText != null)
        {
            if (selectedCompartment.CurrentLevel < selectedCompartment.MaxLevel)
            {
                string nextLevelPreview = GetNextLevelPreview(selectedCompartment);
                nextLevelPreviewText.text = $"Next Level: {nextLevelPreview}";
                nextLevelPreviewText.gameObject.SetActive(true);
            }
            else
            {
                nextLevelPreviewText.text = "MAX LEVEL REACHED";
                nextLevelPreviewText.gameObject.SetActive(true);
            }
        }

        if (costText != null)
        {
            if (selectedCompartment.CurrentLevel < selectedCompartment.MaxLevel)
            {
                float rpCost = GetUpgradeCost(selectedCompartment, false);
                float dpCost = GetUpgradeCost(selectedCompartment, true);
                costText.text = $"Upgrade Cost:\nRP: {rpCost:F0}\nDP: {dpCost:F0}";
            }
            else
            {
                costText.text = "MAX LEVEL";
            }
        }

        // Find and update the upgrade button state
        Button upgradeButton = detailPanel.transform.Find("UpgradeButton")?.GetComponent<Button>();
        if (upgradeButton != null)
        {
            if (selectedCompartment.CurrentLevel >= selectedCompartment.MaxLevel)
            {
                // Max level reached
                upgradeButton.interactable = false;
            }
            else
            {
                // Check if player can afford the upgrade
                float rpCost = GetUpgradeCost(selectedCompartment, false);
                float dpCost = GetUpgradeCost(selectedCompartment, true);

                bool canAfford = (ResourceManager.Instance != null) &&
                                (ResourceManager.Instance.RecyclingPoints >= rpCost) &&
                                (ResourceManager.Instance.DimensionalPotential >= dpCost);

                upgradeButton.interactable = canAfford;

                // Optionally update button color based on affordability
                ColorBlock colors = upgradeButton.colors;
                colors.normalColor = canAfford ? new Color(0.2f, 1f, 0.2f) : new Color(1f, 0.2f, 0.2f);
                upgradeButton.colors = colors;
            }
        }
    }

    // Helper methods to calculate effects and costs
    string GetEffectDescription(ShipCompartment compartment)
    {
        switch (compartment.Type)
        {
            case CompartmentType.Engine:
                return $"Increases probe collection rate by {GetEffectValue(compartment):P0}";
            case CompartmentType.Lab:
                return $"Improves recycling efficiency by {GetEffectValue(compartment):P0}";
            case CompartmentType.Storage:
                return $"Provides {GetEffectValue(compartment):F0} units of storage capacity";
            case CompartmentType.Bridge:
                return $"Enhances command efficiency by {GetEffectValue(compartment):P0}";
            case CompartmentType.Recycling:
                return $"Reduces contamination by {GetEffectValue(compartment):P0}";
            case CompartmentType.Stabilizer:
                return $"Improves waste stability by {GetEffectValue(compartment):P0}";
            case CompartmentType.Communications:
                return $"Increases rare waste chance by {GetEffectValue(compartment):P0}";
            case CompartmentType.Scanner:
                return $"Enhances scanning efficiency by {GetEffectValue(compartment):P0}";
            default:
                return "Unknown effect";
        }
    }

    string GetNextLevelPreview(ShipCompartment compartment)
    {
        if (compartment.CurrentLevel >= compartment.MaxLevel)
            return "MAX LEVEL";

        float currentEffect = GetEffectValue(compartment);
        float nextEffect = GetEffectValue(compartment, true);
        float improvement = nextEffect - currentEffect;

        switch (compartment.Type)
        {
            case CompartmentType.Engine:
                return $"Collection rate: +{improvement:P0}";
            case CompartmentType.Lab:
                return $"Recycling efficiency: +{improvement:P0}";
            case CompartmentType.Storage:
                return $"Storage capacity: +{improvement:F0} units";
            case CompartmentType.Bridge:
                return $"Command efficiency: +{improvement:P0}";
            case CompartmentType.Recycling:
                return $"Contamination reduction: +{improvement:P0}";
            case CompartmentType.Stabilizer:
                return $"Waste stability: +{improvement:P0}";
            case CompartmentType.Communications:
                return $"Rare waste chance: +{improvement:P0}";
            case CompartmentType.Scanner:
                return $"Scanning efficiency: +{improvement:P0}";
            default:
                return "Improved performance";
        }
    }

    float GetEffectValue(ShipCompartment compartment, bool nextLevel = false)
    {
        float baseValue = 0f;
        int level = nextLevel ? compartment.CurrentLevel + 1 : compartment.CurrentLevel;

        // Don't go above max level
        if (level > compartment.MaxLevel)
            level = compartment.MaxLevel;

        // Base values per compartment type
        switch (compartment.Type)
        {
            case CompartmentType.Engine:
                baseValue = 0.1f; // 10% base
                break;
            case CompartmentType.Lab:
                baseValue = 0.15f; // 15% base
                break;
            case CompartmentType.Storage:
                baseValue = 50f; // 50 units base
                break;
            case CompartmentType.Bridge:
                baseValue = 0.2f; // 20% base
                break;
            case CompartmentType.Recycling:
                baseValue = 0.25f; // 25% base
                break;
            case CompartmentType.Stabilizer:
                baseValue = 0.1f; // 10% base
                break;
            case CompartmentType.Communications:
                baseValue = 0.15f; // 15% base
                break;
            case CompartmentType.Scanner:
                baseValue = 0.2f; // 20% base
                break;
        }

        // Calculate multiplier based on level (linear scaling)
        float multiplier = 1f + (0.25f * (level - 1));

        return baseValue * multiplier;
    }

    float GetUpgradeCost(ShipCompartment compartment, bool isDimensionalPotential)
    {
        if (compartment.CurrentLevel >= compartment.MaxLevel)
            return 0f;

        float baseCost = isDimensionalPotential ?
            compartment.Type.GetBaseDPCost() :
            compartment.Type.GetBaseRPCost();

        float costMultiplier = 1.5f; // Default multiplier

        return Mathf.Round(baseCost * Mathf.Pow(costMultiplier, compartment.CurrentLevel - 1));
    }

    // Call this after a successful upgrade
    void PlayUpgradeEffect(ShipCompartment compartment)
    {
        // Find the compartment's renderer
        SpriteRenderer renderer = compartment.GetComponent<SpriteRenderer>();
        if (renderer == null) return;

        // Store original color
        Color originalColor = renderer.color;

        // Start coroutine to flash the compartment
        StartCoroutine(FlashCompartment(renderer, originalColor));
    }

    System.Collections.IEnumerator FlashCompartment(SpriteRenderer renderer, Color originalColor)
    {
        // Flash green
        renderer.color = Color.green;

        yield return new WaitForSeconds(0.1f);

        // Back to original
        renderer.color = originalColor;

        yield return new WaitForSeconds(0.1f);

        // Flash again
        renderer.color = Color.green;

        yield return new WaitForSeconds(0.1f);

        // Finally return to original
        renderer.color = originalColor;
    }

    // Public method that can be called by UI buttons
    public void UpgradeSelectedCompartment()
    {
        if (selectedCompartment == null) return;

        if (ShipManager.Instance != null)
        {
            bool success = ShipManager.Instance.UpgradeCompartment(selectedCompartment);

            if (success)
            {
                Debug.Log("Successfully upgraded: " + selectedCompartment.DisplayName);
                UpdateDetailPanel();
                PlayUpgradeEffect(selectedCompartment);
            }
            else
            {
                Debug.Log("Could not upgrade: " + selectedCompartment.DisplayName);
            }
        }
        else
        {
            Debug.LogError("ShipManager.Instance is null!");
        }
    }
}