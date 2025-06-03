using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ProcessingFacilityUI : MonoBehaviour
{
    [Header("Facility Settings")]
    [SerializeField] private string facilityName = "Basic Processor";
    [SerializeField] private ProcessingRecipe[] availableRecipes;
    
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI facilityNameText;
    [SerializeField] private Transform recipeContainer;
    [SerializeField] private GameObject recipeUIPrefab;
    
    [Header("Selected Recipe Display")]
    [SerializeField] private GameObject selectedRecipePanel;
    [SerializeField] private TextMeshProUGUI selectedRecipeNameText;
    [SerializeField] private Transform inputsContainer;
    [SerializeField] private Transform outputsContainer;
    [SerializeField] private GameObject resourceDisplayPrefab;
    [SerializeField] private Button processButton;
    [SerializeField] private TextMeshProUGUI processButtonText;
    
    [Header("Processing Animation")]
    [SerializeField] private Slider processingProgressSlider;
    [SerializeField] private GameObject processingEffects;
    [SerializeField] private float processingDuration = 2f;
    
    private ProcessingRecipe selectedRecipe;
    private List<RecipeUIElement> recipeUIElements = new List<RecipeUIElement>();
    private bool isProcessing = false;
    private float processingTimer = 0f;
    
    private void Start()
    {
        InitializeUI();
        SetupRecipes();
        UpdateUI();
    }
    
    private void Update()
    {
        if (isProcessing)
        {
            UpdateProcessingProgress();
        }
    }
    
    private void InitializeUI()
    {
        if (facilityNameText != null)
        {
            facilityNameText.text = facilityName;
        }
        
        if (processButton != null)
        {
            processButton.onClick.AddListener(OnProcessButtonClicked);
        }
        
        if (selectedRecipePanel != null)
        {
            selectedRecipePanel.SetActive(false);
        }
        
        if (processingProgressSlider != null)
        {
            processingProgressSlider.gameObject.SetActive(false);
        }
        
        if (processingEffects != null)
        {
            processingEffects.SetActive(false);
        }
    }
    
    private void SetupRecipes()
    {
        // Clear existing recipe UI elements
        foreach (var element in recipeUIElements)
        {
            if (element != null && element.gameObject != null)
            {
                Destroy(element.gameObject);
            }
        }
        recipeUIElements.Clear();
        
        // Create UI elements for each available recipe
        if (availableRecipes == null) return;
        
        foreach (var recipe in availableRecipes)
        {
            CreateRecipeUIElement(recipe);
        }
    }
    
    private void CreateRecipeUIElement(ProcessingRecipe recipe)
    {
        if (recipeUIPrefab == null || recipeContainer == null) return;
        
        GameObject recipeObj = Instantiate(recipeUIPrefab, recipeContainer);
        RecipeUIElement recipeUI = recipeObj.GetComponent<RecipeUIElement>();
        
        if (recipeUI != null)
        {
            recipeUI.Initialize(recipe, this);
            recipeUIElements.Add(recipeUI);
        }
        else
        {
            Debug.LogError("RecipeUIElement component not found on recipe prefab!");
            Destroy(recipeObj);
        }
    }
    
    public void SelectRecipe(ProcessingRecipe recipe)
    {
        selectedRecipe = recipe;
        UpdateSelectedRecipeDisplay();
        UpdateProcessButton();
    }
    
    private void UpdateSelectedRecipeDisplay()
    {
        if (selectedRecipe == null)
        {
            if (selectedRecipePanel != null)
                selectedRecipePanel.SetActive(false);
            return;
        }
        
        if (selectedRecipePanel != null)
            selectedRecipePanel.SetActive(true);
        
        if (selectedRecipeNameText != null)
            selectedRecipeNameText.text = selectedRecipe.recipeName;
        
        // Display inputs
        UpdateResourceDisplayContainer(inputsContainer, selectedRecipe.inputs, "Input:");
        
        // Display outputs
        UpdateResourceDisplayContainer(outputsContainer, selectedRecipe.outputs, "Output:");
    }
    
    private void UpdateResourceDisplayContainer(Transform container, ResourceAmount[] resources, string prefix)
    {
        if (container == null || resourceDisplayPrefab == null) return;
        
        // Clear existing displays
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        
        // Create new displays
        foreach (var resource in resources)
        {
            GameObject displayObj = Instantiate(resourceDisplayPrefab, container);
            ResourceRequirementDisplay display = displayObj.GetComponent<ResourceRequirementDisplay>();
            
            if (display != null)
            {
                bool hasEnough = NewResourceManager.Instance?.GetResourceAmount(resource.type) >= resource.amount;
                display.Initialize(resource, hasEnough, prefix);
            }
            else
            {
                // Fallback to simple text display
                TextMeshProUGUI text = displayObj.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{prefix} {resource.amount} {resource.type}";
                }
            }
        }
    }
    
    private void UpdateProcessButton()
    {
        if (processButton == null || selectedRecipe == null) return;
        
        bool canAfford = NewResourceManager.Instance?.CanAffordRecipe(selectedRecipe.inputs) ?? false;
        
        processButton.interactable = canAfford && !isProcessing;
        
        if (processButtonText != null)
        {
            if (isProcessing)
            {
                processButtonText.text = "Processing...";
            }
            else if (canAfford)
            {
                processButtonText.text = "Process";
            }
            else
            {
                processButtonText.text = "Insufficient Resources";
            }
        }
    }
    
    private void UpdateUI()
    {
        // Update all recipe UI elements
        foreach (var recipeUI in recipeUIElements)
        {
            if (recipeUI != null)
            {
                recipeUI.UpdateAffordability();
            }
        }
        
        // Update selected recipe display
        if (selectedRecipe != null)
        {
            UpdateSelectedRecipeDisplay();
        }
        
        UpdateProcessButton();
    }
    
    private void OnProcessButtonClicked()
    {
        if (selectedRecipe == null || isProcessing) return;
        
        if (NewResourceManager.Instance == null)
        {
            Debug.LogError("NewResourceManager.Instance is null!");
            return;
        }
        
        StartProcessing();
    }
    
    private void StartProcessing()
    {
        isProcessing = true;
        processingTimer = 0f;
        
        if (processingProgressSlider != null)
        {
            processingProgressSlider.gameObject.SetActive(true);
            processingProgressSlider.value = 0f;
        }
        
        if (processingEffects != null)
        {
            processingEffects.SetActive(true);
        }
        
        UpdateProcessButton();
    }
    
    private void UpdateProcessingProgress()
    {
        processingTimer += Time.deltaTime;
        float progress = processingTimer / processingDuration;
        
        if (processingProgressSlider != null)
        {
            processingProgressSlider.value = progress;
        }
        
        if (progress >= 1f)
        {
            CompleteProcessing();
        }
    }
    
    private void CompleteProcessing()
    {
        isProcessing = false;
        processingTimer = 0f;
        
        // Actually process the resources
        bool success = NewResourceManager.Instance.ProcessResources(selectedRecipe);
        
        if (processingProgressSlider != null)
        {
            processingProgressSlider.gameObject.SetActive(false);
        }
        
        if (processingEffects != null)
        {
            processingEffects.SetActive(false);
        }
        
        if (success)
        {
            Debug.Log($"Successfully processed {selectedRecipe.recipeName}!");
            // Could add success effects here
        }
        else
        {
            Debug.LogWarning($"Failed to process {selectedRecipe.recipeName}!");
            // Could add failure effects here
        }
        
        // Refresh UI
        UpdateUI();
    }
    
    // Subscribe to resource changes to update UI
    private void OnEnable()
    {
        if (NewResourceManager.Instance != null)
        {
            NewResourceManager.Instance.OnResourceInventoryChanged += UpdateUI;
        }
    }
    
    private void OnDisable()
    {
        if (NewResourceManager.Instance != null)
        {
            NewResourceManager.Instance.OnResourceInventoryChanged -= UpdateUI;
        }
    }
    
    // Public method to add recipes at runtime
    public void AddRecipe(ProcessingRecipe recipe)
    {
        var recipeList = availableRecipes?.ToList() ?? new List<ProcessingRecipe>();
        recipeList.Add(recipe);
        availableRecipes = recipeList.ToArray();
        
        CreateRecipeUIElement(recipe);
        UpdateUI();
    }
    
    // Public method to remove recipes
    public void RemoveRecipe(ProcessingRecipe recipe)
    {
        if (availableRecipes == null) return;
        
        var recipeList = availableRecipes.ToList();
        recipeList.Remove(recipe);
        availableRecipes = recipeList.ToArray();
        
        // Remove UI element
        var uiElement = recipeUIElements.Find(ui => ui.Recipe == recipe);
        if (uiElement != null)
        {
            recipeUIElements.Remove(uiElement);
            Destroy(uiElement.gameObject);
        }
        
        // Clear selection if this was the selected recipe
        if (selectedRecipe == recipe)
        {
            selectedRecipe = null;
            UpdateSelectedRecipeDisplay();
            UpdateProcessButton();
        }
    }
}

/// <summary>
/// UI element for individual processing recipes
/// </summary>
public class RecipeUIElement : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private TextMeshProUGUI inputsText;
    [SerializeField] private TextMeshProUGUI outputsText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image backgroundImage;
    
    [Header("Visual States")]
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = Color.gray;
    [SerializeField] private Color selectedColor = Color.yellow;
    
    public ProcessingRecipe Recipe { get; private set; }
    private ProcessingFacilityUI facilityUI;
    private bool isAffordable = false;
    private bool isSelected = false;
    
    public void Initialize(ProcessingRecipe recipe, ProcessingFacilityUI facility)
    {
        Recipe = recipe;
        facilityUI = facility;
        
        if (recipeNameText != null)
            recipeNameText.text = recipe.recipeName;
        
        if (inputsText != null)
            inputsText.text = FormatResourceList(recipe.inputs, "Needs:");
        
        if (outputsText != null)
            outputsText.text = FormatResourceList(recipe.outputs, "Makes:");
        
        if (selectButton != null)
            selectButton.onClick.AddListener(OnSelectClicked);
        
        UpdateAffordability();
    }
    
    private string FormatResourceList(ResourceAmount[] resources, string prefix)
    {
        if (resources == null || resources.Length == 0)
            return prefix + " Nothing";
        
        var formatted = resources.Select(r => $"{r.amount} {r.type}");
        return prefix + " " + string.Join(", ", formatted);
    }
    
    public void UpdateAffordability()
    {
        if (Recipe == null || NewResourceManager.Instance == null) return;
        
        isAffordable = NewResourceManager.Instance.CanAffordRecipe(Recipe.inputs);
        UpdateVisuals();
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (backgroundImage != null)
        {
            if (isSelected)
                backgroundImage.color = selectedColor;
            else if (isAffordable)
                backgroundImage.color = affordableColor;
            else
                backgroundImage.color = unaffordableColor;
        }
        
        if (selectButton != null)
            selectButton.interactable = isAffordable;
    }
    
    private void OnSelectClicked()
    {
        facilityUI?.SelectRecipe(Recipe);
        
        // Update selection state for all recipes in the facility
        var allRecipeElements = facilityUI.GetComponentsInChildren<RecipeUIElement>();
        foreach (var element in allRecipeElements)
        {
            element.SetSelected(element == this);
        }
    }
}

/// <summary>
/// Display component for resource requirements in recipes
/// </summary>
public class ResourceRequirementDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image resourceIcon;
    [SerializeField] private TextMeshProUGUI resourceNameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Header("Visual States")]
    [SerializeField] private Color sufficientColor = Color.green;
    [SerializeField] private Color insufficientColor = Color.red;
    
    public void Initialize(ResourceAmount resource, bool hasEnough, string prefix = "")
    {
        // Update icon
        if (resourceIcon != null)
        {
            var config = NewResourceManager.Instance?.GetResourceConfig(resource.type);
            if (config?.icon != null)
            {
                resourceIcon.sprite = config.icon;
                resourceIcon.color = Color.white;
            }
            else
            {
                resourceIcon.color = Color.gray;
            }
        }
        
        // Update name
        if (resourceNameText != null)
        {
            var config = NewResourceManager.Instance?.GetResourceConfig(resource.type);
            string displayName = config?.displayName ?? resource.type.ToString();
            resourceNameText.text = prefix + displayName;
        }
        
        // Update amount
        if (amountText != null)
        {
            int currentAmount = NewResourceManager.Instance?.GetResourceAmount(resource.type) ?? 0;
            amountText.text = $"{currentAmount}/{resource.amount}";
            amountText.color = hasEnough ? sufficientColor : insufficientColor;
        }
        
        // Update status
        if (statusText != null)
        {
            statusText.text = hasEnough ? "✓" : "✗";
            statusText.color = hasEnough ? sufficientColor : insufficientColor;
        }
    }
}