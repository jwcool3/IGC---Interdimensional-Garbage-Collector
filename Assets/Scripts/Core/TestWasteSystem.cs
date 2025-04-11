using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TestWasteSystem : MonoBehaviour
{
    [SerializeField] private Button generateButton;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private Transform wasteDisplayContainer;
    [SerializeField] private GameObject wasteDisplayPrefab;

    private WasteGenerator wasteGenerator;
    private List<WasteItem> generatedWaste = new List<WasteItem>();

    private void Start()
    {
        wasteGenerator = GetComponent<WasteGenerator>();
        if (wasteGenerator == null)
        {
            wasteGenerator = gameObject.AddComponent<WasteGenerator>();
        }

        if (generateButton != null)
        {
            generateButton.onClick.AddListener(GenerateWasteItem);
        }
    }

    public void GenerateWasteItem()
    {
        WasteItem newWaste = wasteGenerator.GenerateWasteItem();
        generatedWaste.Add(newWaste);

        DisplayWasteInfo(newWaste);
        CreateWasteDisplay(newWaste);
    }

    private void DisplayWasteInfo(WasteItem waste)
    {
        if (outputText != null)
        {
            outputText.text = $"Generated: {waste.Name}\n" +
                             $"Origin: {waste.DimensionalOrigin}\n" +
                             $"Stability: {waste.WasteStability:P2}\n" +
                             $"Recycling Potential: {waste.RecyclingPotential:P2}\n" +
                             $"Contamination: {waste.ContaminationLevel:P2}";
        }
    }

    private void CreateWasteDisplay(WasteItem waste)
    {
        if (wasteDisplayContainer == null || wasteDisplayPrefab == null)
            return;

        GameObject wasteDisplay = Instantiate(wasteDisplayPrefab, wasteDisplayContainer);
        // You'll create this component in a later step
        WasteDisplay displayComponent = wasteDisplay.GetComponent<WasteDisplay>();
        if (displayComponent != null)
        {
            displayComponent.Initialize(waste);
        }
    }

    private void OnDestroy()
    {
        if (generateButton != null)
        {
            generateButton.onClick.RemoveListener(GenerateWasteItem);
        }
    }
}