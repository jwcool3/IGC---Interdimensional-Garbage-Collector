# Crafting UI Setup Guide

## Overview
This guide explains how to set up the complete crafting UI system in Unity, integrating with your existing TabSystem.

## Files Updated/Created

### Updated Files:
1. **TabSystem.cs** - Added crafting tab support
2. **ResourceInventoryUI.cs** - Added crafting button for crafted items tab

### New Files:
1. **CraftingPanel.cs** - Main crafting panel component
2. **CraftingUI.cs** - Crafting interface logic
3. **CraftedItemDisplayItem.cs** - Individual crafted item display
4. **IngredientDisplay.cs** - Recipe ingredient display
5. **CraftingJobDisplay.cs** - Active crafting job display

## Unity Setup Instructions

### 1. TabSystem Setup

The TabSystem now includes a crafting tab. In the Unity Inspector:

1. **Find your TabSystem GameObject**
2. **Add these fields in the Inspector:**
   - **Crafting Tab Button**: Assign a new Button for the crafting tab
   - **Crafting Panel**: Assign the crafting panel GameObject (see step 2)

### 2. Creating the Crafting Panel Hierarchy

Create this hierarchy in your UI Canvas:

```
CraftingPanel (GameObject)
├── CraftingPanel (Script: CraftingPanel.cs)
├── Header
│   ├── Title (Text: "Crafting")
│   └── BackButton (Button: "← Back")
├── CraftingContent
│   ├── CraftingUI (Script: CraftingUI.cs)
│   ├── RecipeList (ScrollRect)
│   │   ├── Viewport
│   │   │   └── RecipeListContainer (Transform)
│   │   └── Scrollbar
│   ├── RecipeDetailsPanel
│   │   ├── RecipeInfo
│   │   │   ├── RecipeNameText (TextMeshProUGUI)
│   │   │   └── RecipeDescriptionText (TextMeshProUGUI)
│   │   ├── IngredientsContainer (Transform)
│   │   ├── OutputInfo
│   │   │   ├── OutputItemIcon (Image)
│   │   │   └── OutputAmountText (TextMeshProUGUI)
│   │   └── CraftingControls
│   │       ├── BatchSizeSlider (Slider)
│   │       ├── BatchSizeText (TextMeshProUGUI)
│   │       ├── FacilityDropdown (Dropdown)
│   │       ├── ProcessingTimeText (TextMeshProUGUI)
│   │       ├── SuccessChanceText (TextMeshProUGUI)
│   │       └── CraftButton (Button: "CRAFT")
│   └── ActiveJobsPanel
│       └── ActiveJobsContainer (Transform)
└── LoadingIndicator (GameObject, initially inactive)
```

### 3. ResourceInventoryUI Setup

In your ResourceInventoryUI (crafted items tab):

1. **Add a new Button**: "Open Crafting" or "⚒️ Craft Items"
2. **Assign the button** to the `openCraftingButton` field
3. **Position it** prominently on the crafted items tab

### 4. Required Prefabs

Create these prefabs in your project:

#### A. RecipeButtonPrefab
```
RecipeButton (Button)
├── Background (Image)
├── RecipeText (TextMeshProUGUI)
└── CraftableIndicator (Image) - changes color based on availability
```

#### B. IngredientDisplayPrefab
```
IngredientDisplay (GameObject + IngredientDisplay.cs)
├── IngredientIcon (Image)
├── IngredientNameText (TextMeshProUGUI)
├── RequiredAmountText (TextMeshProUGUI)
├── AvailableAmountText (TextMeshProUGUI)
└── AvailabilityIndicator (Image) - red/green
```

#### C. CraftingJobPrefab
```
CraftingJob (GameObject + CraftingJobDisplay.cs)
├── JobNameText (TextMeshProUGUI)
├── FacilityNameText (TextMeshProUGUI)
├── ProgressSlider (Slider)
├── TimeRemainingText (TextMeshProUGUI)
└── CancelButton (Button)
```

#### D. CraftedItemDisplayPrefab
```
CraftedItemDisplay (GameObject + CraftedItemDisplayItem.cs)
├── ItemIcon (Image)
├── ItemNameText (TextMeshProUGUI)
├── AmountText (TextMeshProUGUI)
├── StackInfoText (TextMeshProUGUI)
└── UseButton (Button) - only visible for usable items
```

### 5. Inspector Field Assignments

#### TabSystem.cs
- `craftingTabButton` → Your crafting tab button
- `craftingPanel` → Your CraftingPanel GameObject

#### CraftingPanel.cs
- `craftingUI` → CraftingUI component (child object)
- `backButton` → Back button (optional)
- `loadingIndicator` → Loading indicator GameObject

#### CraftingUI.cs
- `recipeListContainer` → Transform for recipe buttons
- `recipeButtonPrefab` → Your recipe button prefab
- `recipeDetailsPanel` → Panel showing recipe details
- `ingredientsContainer` → Transform for ingredient displays
- `ingredientDisplayPrefab` → Your ingredient display prefab
- `activeJobsContainer` → Transform for active job displays
- `craftingJobPrefab` → Your crafting job prefab
- All other UI element references

#### ResourceInventoryUI.cs
- `openCraftingButton` → Button to open crafting panel
- `craftedItemDisplayPrefab` → Your crafted item display prefab

## 6. Testing the Integration

### Test Sequence:
1. **Start the game**
2. **Navigate to Resources tab**
3. **Switch to Crafted Items sub-tab**
4. **Click "Open Crafting" button**
5. **Verify crafting panel opens**
6. **Test recipe selection and crafting**
7. **Check active jobs display**
8. **Verify crafted items appear in inventory**

### Debug Checklist:
- [ ] TabSystem finds all panel references
- [ ] Crafting button appears on crafted items tab
- [ ] Crafting panel opens when button clicked
- [ ] Recipe list populates with available recipes
- [ ] Ingredient requirements show correctly
- [ ] Facility dropdown shows available facilities
- [ ] Crafting jobs appear in active jobs panel
- [ ] Completed items go to crafted inventory

## 7. Common Issues and Solutions

### Issue: Crafting button doesn't appear
**Solution**: Check that `openCraftingButton` is assigned in ResourceInventoryUI Inspector

### Issue: Crafting panel doesn't open
**Solution**: Verify TabSystem has `craftingPanel` assigned and `ShowCraftingTab()` method exists

### Issue: No recipes show up
**Solution**: Check that CraftingManager.Instance exists and has recipes loaded

### Issue: Ingredients show as unavailable
**Solution**: Verify NewResourceManager and CraftedItemInventory have proper resource amounts

### Issue: Active jobs don't appear
**Solution**: Check that `craftingJobPrefab` is assigned and has CraftingJobDisplay component

## 8. Styling Recommendations

### Colors:
- **Crafting Button**: Gold/Orange (#FFA500)
- **Available Recipes**: Green text
- **Unavailable Recipes**: Gray text with reduced opacity
- **Available Ingredients**: Green indicators
- **Missing Ingredients**: Red indicators
- **Active Jobs**: Blue/Cyan progress bars

### Icons:
- **Crafting Tab**: ⚒️ or 🔧
- **Crafting Button**: ⚙️ or 🛠️
- **Available Recipe**: ✅
- **Unavailable Recipe**: ❌
- **Processing**: ⚡ or 🔄

## 9. Integration Points

The crafting system integrates with:
- **TabSystem**: For main navigation
- **ResourceInventoryUI**: For resource/crafted item display
- **CraftingManager**: For recipe management
- **NewResourceManager**: For raw resource tracking
- **CraftedItemInventory**: For crafted item storage
- **FacilityManager**: For processing facility management

This creates a seamless workflow from resource collection → crafting → item usage. 