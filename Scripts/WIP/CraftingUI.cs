using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Main crafting interface
/// </summary>
public class CraftingUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private GameObject recipeButtonPrefab;
    [SerializeField] private ScrollRect recipeScrollView;
    
    [Header("Recipe Details Panel")]
    [SerializeField] private GameObject recipeDetailsPanel;
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private TextMeshProUGUI recipeDescriptionText;
    [SerializeField] private Transform ingredientsContainer;
    [SerializeField] private GameObject ingredientDisplayPrefab;
    [SerializeField] private Image outputItemIcon;
    [SerializeField] private TextMeshProUGUI outputAmountText;
    
    [Header("Crafting Controls")]
    [SerializeField] private Slider batchSizeSlider;
    [SerializeField] private TextMeshProUGUI batchSizeText;
    [SerializeField] private Button craftButton;
    [SerializeField] private Dropdown facilityDropdown;
    [SerializeField] private TextMeshProUGUI processingTimeText;
    [SerializeField] private TextMeshProUGUI successChanceText;
    
    [Header("Active Jobs Panel")]
    [SerializeField] private Transform activeJobsContainer;
    [SerializeField] private GameObject craftingJobPrefab;
    
    private CraftingRecipeData selectedRecipe;
    private List<ProcessingFacilityBase> availableFacilities = new List<ProcessingFacilityBase>();
    
    private void Start()
    {
        InitializeUI();
        LoadRecipes();
        
        // Subscribe to crafting events
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingStarted += OnCraftingJobStarted;
            CraftingManager.Instance.OnCraftingCompleted += OnCraftingJobCompleted;
        }
    }
    
    private void InitializeUI()
    {
        // Setup batch size slider
        if (batchSizeSlider != null)
        {
            batchSizeSlider.minValue = 1;
            batchSizeSlider.maxValue = 10;
            batchSizeSlider.value = 1;
            batchSizeSlider.onValueChanged.AddListener(OnBatchSizeChanged);
        }
        
        // Setup craft button
        if (craftButton != null)
        {
            craftButton.onClick.AddListener(StartCrafting);
        }
        
        // Setup facility dropdown
        if (facilityDropdown != null)
        {
            facilityDropdown.onValueChanged.AddListener(OnFacilitySelectionChanged);
        }
        
        // Hide details panel initially
        if (recipeDetailsPanel != null)
            recipeDetailsPanel.SetActive(false);
    }
    
    private void LoadRecipes()
    {
        if (CraftingManager.Instance == null) return;
        
        var recipes = CraftingManager.Instance.GetAvailableRecipes();
        
        // Clear existing recipe buttons
        foreach (Transform child in recipeListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create buttons for each recipe
        foreach (var recipe in recipes)
        {
            var buttonObj = Instantiate(recipeButtonPrefab, recipeListContainer);
            var button = buttonObj.GetComponent<Button>();
            var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (text != null)
                text.text = recipe.recipeName;
                
            if (button != null)
                button.onClick.AddListener(() => SelectRecipe(recipe));
                
            // Add visual indicators for craftability
            UpdateRecipeButtonState(buttonObj, recipe);
        }
    }
    
    private void UpdateRecipeButtonState(GameObject buttonObj, CraftingRecipeData recipe)
    {
        var button = buttonObj.GetComponent<Button>();
        var canCraft = recipe.CanCraft();
        
        if (button != null)
        {
            button.interactable = canCraft;
            
            // Change color based on craftability
            var colors = button.colors;
            colors.normalColor = canCraft ? Color.white : Color.gray;
            button.colors = colors;
        }
    }
    
    private void SelectRecipe(CraftingRecipeData recipe)
    {
        selectedRecipe = recipe;
        UpdateRecipeDetails();
        UpdateFacilityDropdown();
        
        if (recipeDetailsPanel != null)
            recipeDetailsPanel.SetActive(true);
    }
    
    private void UpdateRecipeDetails()
    {
        if (selectedRecipe == null) return;
        
        // Update basic info
        if (recipeNameText != null)
            recipeNameText.text = selectedRecipe.recipeName;
            
        if (recipeDescriptionText != null)
            recipeDescriptionText.text = selectedRecipe.description;
        
        // Update output info
        if (outputItemIcon != null)
        {
            var config = ResourceConfigManager.Instance?.GetResourceConfig(selectedRecipe.outputItemType);
            if (config?.icon != null)
                outputItemIcon.sprite = config.icon;
        }
        
        if (outputAmountText != null)
            outputAmountText.text = selectedRecipe.outputAmount.ToString();
        
        // Update ingredients display
        UpdateIngredientsDisplay();
        
        // Update batch size limits
        if (batchSizeSlider != null)
        {
            batchSizeSlider.maxValue = selectedRecipe.maxBatchSize;
            batchSizeSlider.value = 1;
        }
        
        UpdateCraftingInfo();
    }
    
    private void UpdateIngredientsDisplay()
    {
        if (selectedRecipe == null || ingredientsContainer == null) return;
        
        // Clear existing ingredients
        foreach (Transform child in ingredientsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create displays for each ingredient
        foreach (var ingredient in selectedRecipe.requiredIngredients)
        {
            var displayObj = Instantiate(ingredientDisplayPrefab, ingredientsContainer);
            var display = displayObj.GetComponent<IngredientDisplay>();
            
            if (display != null)
            {
                int batchSize = Mathf.RoundToInt(batchSizeSlider?.value ?? 1);
                display.UpdateDisplay(ingredient, batchSize);
            }
        }
    }
    
    private void UpdateFacilityDropdown()
    {
        if (selectedRecipe == null || facilityDropdown == null) return;
        
        availableFacilities.Clear();
        facilityDropdown.ClearOptions();
        
        // Find facilities that can handle this recipe
        var allFacilities = FacilityManager.Instance?.GetProcessingFacilities();
        if (allFacilities != null)
        {
            foreach (var facility in allFacilities)
            {
                if (CanFacilityHandleRecipe(facility, selectedRecipe))
                {
                    availableFacilities.Add(facility);
                }
            }
        }
        
        // Populate dropdown
        var options = new List<string>();
        foreach (var facility in availableFacilities)
        {
            string status = facility.IsOperational ? "Ready" : "Offline";
            options.Add($"{facility.FacilityName} ({status})");
        }
        
        facilityDropdown.AddOptions(options);
        
        if (availableFacilities.Count > 0)
        {
            facilityDropdown.value = 0;
            UpdateCraftingInfo();
        }
    }
    
    private bool CanFacilityHandleRecipe(ProcessingFacilityBase facility, CraftingRecipeData recipe)
    {
        if (!facility.IsOperational) return false;
        if (facility.FacilityType != recipe.requiredFacilityType) return false;
        if (facility.Level < recipe.minimumFacilityLevel) return false;
        
        return true;
    }
    
    private void OnBatchSizeChanged(float value)
    {
        int batchSize = Mathf.RoundToInt(value);
        
        if (batchSizeText != null)
            batchSizeText.text = batchSize.ToString();
        
        UpdateIngredientsDisplay();
        UpdateCraftingInfo();
    }
    
    private void OnFacilitySelectionChanged(int index)
    {
        UpdateCraftingInfo();
    }
    
    private void UpdateCraftingInfo()
    {
        if (selectedRecipe == null) return;
        
        int batchSize = Mathf.RoundToInt(batchSizeSlider?.value ?? 1);
        var selectedFacility = GetSelectedFacility();
        
        if (selectedFacility != null)
        {
            // Update processing time
            float processingTime = selectedRecipe.GetProcessingTime(selectedFacility, batchSize);
            if (processingTimeText != null)
                processingTimeText.text = $"{processingTime:F1}s";
            
            // Update success chance
            float successChance = selectedRecipe.GetSuccessChance(selectedFacility);
            if (successChanceText != null)
                successChanceText.text = $"{successChance:P0}";
        }
        
        // Update craft button state
        bool canCraft = selectedRecipe.CanCraft(batchSize) && selectedFacility != null && selectedFacility.IsOperational;
        if (craftButton != null)
            craftButton.interactable = canCraft;
    }
    
    private ProcessingFacilityBase GetSelectedFacility()
    {
        if (facilityDropdown == null || availableFacilities.Count == 0) return null;
        
        int index = facilityDropdown.value;
        if (index >= 0 && index < availableFacilities.Count)
            return availableFacilities[index];
            
        return null;
    }
    
    private void StartCrafting()
    {
        if (selectedRecipe == null) return;
        
        var facility = GetSelectedFacility();
        int batchSize = Mathf.RoundToInt(batchSizeSlider?.value ?? 1);
        
        if (CraftingManager.Instance?.StartCrafting(selectedRecipe, facility, batchSize) == true)
        {
            Debug.Log($"Started crafting {batchSize}x {selectedRecipe.recipeName}");
            LoadRecipes(); // Refresh to update craftability
        }
    }
    
    private void OnCraftingJobStarted(CraftingJob job)
    {
        // Add job to active jobs display
        if (activeJobsContainer != null && craftingJobPrefab != null)
        {
            var jobObj = Instantiate(craftingJobPrefab, activeJobsContainer);
            var jobDisplay = jobObj.GetComponent<CraftingJobDisplay>();
            
            if (jobDisplay != null)
                jobDisplay.Initialize(job);
        }
    }
    
    private void OnCraftingJobCompleted(CraftingJob job, bool success)
    {
        // Remove job from active jobs display
        // (CraftingJobDisplay handles this automatically)
        
        // Refresh recipe list to update craftability
        LoadRecipes();
    }
} 