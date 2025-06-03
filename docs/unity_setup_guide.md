# 🚀 Unity Resource System Implementation Guide

## 📁 Project Structure Setup

### **Step 1: Create Folder Hierarchy**
```
Assets/
├── Scripts/
│   ├── Resources/
│   │   ├── Core/
│   │   │   ├── ResourceType.cs
│   │   │   ├── NewResourceManager.cs
│   │   │   └── ResourceConfig.cs
│   │   ├── Processing/
│   │   │   ├── UpdatedWasteProcessor.cs
│   │   │   └── ProcessingFacilityUI.cs
│   │   ├── UI/
│   │   │   ├── ResourceDisplayUI.cs
│   │   │   └── ResourceItemDisplay.cs
│   │   └── Items/
│   │       └── UpdatedWasteItem.cs
├── ScriptableObjects/
│   └── Resources/
├── Prefabs/
│   ├── UI/
│   │   ├── ResourceDisplay/
│   │   └── Processing/
│   └── GameObjects/
├── Sprites/
│   └── Resources/
└── Audio/
    └── Processing/
```

### **Step 2: Create Resource ScriptableObjects**

#### **2.1: Create Resource Configurations**
1. **In Unity Editor**: Right-click in `Assets/ScriptableObjects/Resources/`
2. **Select**: `Create → Resources → Resource Configuration`
3. **Create these configs** (name exactly as shown):

**PlasticConfig.asset:**
- Resource Type: `Plastic`
- Display Name: `"Plastic"`
- Description: `"Versatile polymer for fuel and basic components"`
- Base Value: `1`
- Resource Color: Light Blue `#87CEEB`

**MetalScrapsConfig.asset:**
- Resource Type: `MetalScraps`
- Display Name: `"Metal Scraps"`
- Description: `"Salvaged metal for ship parts and upgrades"`
- Base Value: `2`
- Resource Color: Silver `#C0C0C0`

**OrganicMatterConfig.asset:**
- Resource Type: `OrganicMatter`
- Display Name: `"Organic Matter"`
- Description: `"Biological material for food and fuel"`
- Base Value: `1`
- Resource Color: Green `#90EE90`

**CrystalFragmentsConfig.asset:**
- Resource Type: `CrystalFragments`
- Display Name: `"Crystal Fragments"`
- Description: `"Rare crystals for advanced technology"`
- Base Value: `4`
- Resource Color: Purple `#9370DB`

**NeuralResidueConfig.asset:**
- Resource Type: `NeuralResidue`
- Display Name: `"Neural Residue"`
- Description: `"Psychic traces for knowledge and scanning"`
- Base Value: `3`
- Resource Color: Pink `#FF69B4`

**ToxicSludgeConfig.asset:**
- Resource Type: `ToxicSludge`
- Display Name: `"Toxic Sludge"`
- Description: `"Dangerous but potent energy source"`
- Base Value: `2`
- Resource Color: Dark Green `#556B2F`

#### **2.2: Create Processing Recipes**
1. **In Unity Editor**: Right-click in `Assets/ScriptableObjects/Resources/`
2. **Select**: `Create → Resources → Processing Recipe` (you'll need to create this)

**Example Recipe - Plastic to Fuel:**
```
Recipe Name: "Plastic to Fuel"
Required Facility: "Fuel Synthesizer"
Processing Time: 2.0f
Inputs: 
  - Type: Plastic, Amount: 3
Outputs:
  - Type: Fuel, Amount: 2
```

## 🎮 GameObject Setup

### **Step 3: Create Core Manager GameObjects**

#### **3.1: Resource Manager GameObject**
1. **Create Empty GameObject**: `"ResourceManager"`
2. **Add Component**: `NewResourceManager`
3. **In Inspector**:
   - **Default Storage Limit**: `1000`
   - **Resource Configs**: Drag all your resource configs here
4. **Tag as**: `"GameManager"` (create this tag if needed)
5. **Position**: `(0, 0, 0)` in a `_Managers` scene or make it a prefab

#### **3.2: Waste Processor GameObject**
1. **Create Empty GameObject**: `"WasteProcessor"`
2. **Add Component**: `UpdatedWasteProcessor`
3. **Add Components for Effects**:
   - **Audio Source** (for processing sounds)
   - **Particle System** (for visual effects)
4. **In Inspector**:
   - **Base Processing Time**: `1.0f`
   - **Show Processing Effects**: `✓ true`
   - **Processing Effect**: Assign the Particle System
   - **Processing Audio**: Assign the Audio Source

### **Step 4: Create UI Prefabs**

#### **4.1: Resource Display Item Prefab**
1. **Create UI → Panel**: Name it `"ResourceDisplayItem"`
2. **Structure it like this**:
```
ResourceDisplayItem (Panel)
├── Background (Image)
├── IconContainer (Panel)
│   └── ResourceIcon (Image)
├── InfoContainer (Vertical Layout Group)
│   ├── ResourceName (TextMeshPro)
│   ├── AmountText (TextMeshPro)
│   └── StorageInfo (Horizontal Layout Group)
│       ├── StorageText (TextMeshPro)
│       └── StorageSlider (Slider)
└── EffectsContainer (Panel)
    ├── ChangeEffect (Particle System)
    └── Animator (Animator)
```

**Component Setup**:
- **On ResourceDisplayItem**: Add `ResourceItemDisplay` component
- **Background**: Set color to semi-transparent
- **Resource Icon**: Set to placeholder sprite, aspect ratio preserved
- **Layout Groups**: Set spacing and padding appropriately
- **Storage Slider**: Set min=0, max=1, value=0

#### **4.2: Resource Container UI Prefab**
1. **Create UI → Panel**: Name it `"ResourceDisplayPanel"`
2. **Structure**:
```
ResourceDisplayPanel (Panel)
├── Header (Panel)
│   ├── TitleText (TextMeshPro) "Resources"
│   └── ControlsContainer (Horizontal Layout Group)
│       ├── RawResourcesToggle (Toggle) "Raw"
│       ├── ProcessedResourcesToggle (Toggle) "Processed"
│       ├── ToggleAllButton (Button) "Toggle All"
│       └── SortDropdown (TMP_Dropdown)
└── ContentContainer (Panel)
    ├── ScrollView (Scroll Rect)
    │   └── ResourcesGrid (Grid Layout Group)
    └── EmptyMessage (TextMeshPro) "No resources"
```

**Component Setup**:
- **On ResourceDisplayPanel**: Add `ResourceDisplayUI` component
- **In ResourceDisplayUI Inspector**:
  - **Resource Container**: Assign ResourcesGrid
  - **Resource Item Prefab**: Assign ResourceDisplayItem prefab
  - **Show Raw/Processed Toggles**: Assign toggles
  - **Sort Dropdown**: Assign dropdown

#### **4.3: Processing Facility UI Prefab**
1. **Create UI → Panel**: Name it `"ProcessingFacilityPanel"`
2. **Structure**:
```
ProcessingFacilityPanel (Panel)
├── HeaderPanel (Panel)
│   └── FacilityNameText (TextMeshPro)
├── RecipesPanel (Panel)
│   ├── RecipesTitle (TextMeshPro) "Available Recipes"
│   └── RecipesScrollView (Scroll Rect)
│       └── RecipesContainer (Vertical Layout Group)
├── SelectedRecipePanel (Panel) [Initially disabled]
│   ├── RecipeTitle (TextMeshPro)
│   ├── InputsPanel (Panel)
│   │   ├── InputsTitle (TextMeshPro) "Required:"
│   │   └── InputsContainer (Vertical Layout Group)
│   ├── OutputsPanel (Panel)
│   │   ├── OutputsTitle (TextMeshPro) "Produces:"
│   │   └── OutputsContainer (Vertical Layout Group)
│   └── ProcessButton (Button)
│       └── ProcessButtonText (TextMeshPro) "Process"
└── ProcessingProgressPanel (Panel) [Initially disabled]
    ├── ProgressText (TextMeshPro) "Processing..."
    ├── ProgressSlider (Slider)
    └── EffectsContainer (Panel)
```

## 🔧 Component Configuration

### **Step 5: Configure Components**

#### **5.1: NewResourceManager Configuration**
```csharp
// In Inspector or via code:
- Default Storage Limit: 1000
- Resource Configs: [Array of all 6 resource configs]
```

#### **5.2: Processing Facility Configuration**
```csharp
// Example configuration for a Basic Processor:
- Facility Name: "Basic Recycling Facility"
- Available Recipes: [Array of ProcessingRecipe ScriptableObjects]
- Processing Duration: 2.0f
```

### **Step 6: Create Example Processing Recipes**

#### **6.1: Basic Fuel Recipe**
```csharp
// Create as ScriptableObject
Recipe Name: "Make Fuel"
Required Facility: "Basic Processor"
Processing Time: 1.5f
Inputs:
  - Plastic: 2 units
Outputs:
  - Fuel: 1 unit
```

#### **6.2: Advanced Component Recipe**
```csharp
Recipe Name: "Craft Advanced Parts"
Required Facility: "Advanced Fabricator"
Processing Time: 3.0f
Inputs:
  - Metal Scraps: 3 units
  - Crystal Fragments: 1 unit
Outputs:
  - Parts: 2 units
```

## 🎨 Visual Setup

### **Step 7: Create Resource Icons**

**Create or find 64x64 pixel sprites for each resource:**
- 🟩 **Plastic**: Stylized polymer chunks
- ⬛ **Metal Scraps**: Rusty metal pieces  
- 🟫 **Organic Matter**: Leafy/biological material
- 💠 **Crystal Fragments**: Glowing purple crystals
- 🧠 **Neural Residue**: Pink/magenta energy wisps
- 💉 **Toxic Sludge**: Green bubbling liquid

**Icon Setup:**
1. **Import sprites** with `Sprite (2D and UI)` texture type
2. **Set filtering** to `Point (no filter)` for pixel art style
3. **Assign icons** to each ResourceConfig's icon field

### **Step 8: Color Scheme Setup**

**Apply consistent colors throughout:**
- **UI Background**: Dark gray `#2D2D30`
- **Panel Backgrounds**: Medium gray `#3F3F46` with slight transparency
- **Text**: White `#FFFFFF` for main text, light gray `#CCCCCC` for secondary
- **Resource Colors**: Use the colors specified in Step 2.1

## ⚡ Integration Steps

### **Step 9: Replace Old System Calls**

**Find and replace in your existing code:**

```csharp
// OLD: Abstract currency system
RecyclingManager.AddRP(50);
RecyclingManager.AddDP(25);

// NEW: Specific resources
NewResourceManager.Instance.AddResource(ResourceType.Plastic, 3);
NewResourceManager.Instance.AddResource(ResourceType.MetalScraps, 2);
```

```csharp
// OLD: Generic waste processing
WasteProcessor.ProcessWaste(wasteItem);

// NEW: Resource-based processing  
UpdatedWasteProcessor.Instance.ProcessWasteItem(updatedWasteItem);
```

### **Step 10: Update Waste Item Creation**

**Modify your waste generation code:**

```csharp
// OLD: Basic waste items
var wasteItem = new WasteItem("Rusted Can", WasteRarity.Common);

// NEW: Resource-aware waste items
var wasteItem = new UpdatedWasteItem("Rusted Can", "Earth", WasteRarity.Common);
wasteItem.InitializeProperties(); // This generates resource yields
```

## 🧪 Testing Setup

### **Step 11: Create Test Scene**

1. **Create new scene**: `"ResourceSystemTest"`
2. **Add GameObjects**:
   - ResourceManager (with NewResourceManager)
   - WasteProcessor (with UpdatedWasteProcessor)  
   - Canvas with ResourceDisplayUI
   - Canvas with ProcessingFacilityUI
3. **Create test script**:

```csharp
public class ResourceSystemTester : MonoBehaviour
{
    public Button addPlasticButton;
    public Button addMetalButton;
    public Button processWasteButton;
    
    void Start()
    {
        addPlasticButton.onClick.AddListener(() => 
        {
            NewResourceManager.Instance.AddResource(ResourceType.Plastic, 5);
        });
        
        addMetalButton.onClick.AddListener(() => 
        {
            NewResourceManager.Instance.AddResource(ResourceType.MetalScraps, 3);
        });
        
        processWasteButton.onClick.AddListener(() => 
        {
            var testWaste = new UpdatedWasteItem("Test Waste", "Earth", WasteRarity.Common);
            UpdatedWasteProcessor.Instance.ProcessWasteItem(testWaste);
        });
    }
}
```

## 🔍 Debugging Tips

### **Common Issues & Solutions:**

**Issue**: Resources not appearing in UI
- **Solution**: Check that ResourceDisplayUI has correct prefab assigned
- **Solution**: Verify NewResourceManager.Instance is not null

**Issue**: Processing not working
- **Solution**: Ensure UpdatedWasteProcessor has NewResourceManager reference
- **Solution**: Check that waste items have initialized properties

**Issue**: Storage limits not working  
- **Solution**: Verify storage limits are set in ResourceManager
- **Solution**: Check AddResource return value for storage full

**Issue**: UI not updating
- **Solution**: Ensure UI components subscribe to ResourceManager events
- **Solution**: Check that events are being fired correctly

### **Debug Console Commands:**

Add this to your test script for easy debugging:

```csharp
[ContextMenu("Add Test Resources")]
void AddTestResources()
{
    NewResourceManager.Instance.AddResource(ResourceType.Plastic, 10);
    NewResourceManager.Instance.AddResource(ResourceType.MetalScraps, 5);
    NewResourceManager.Instance.AddResource(ResourceType.OrganicMatter, 8);
}

[ContextMenu("Process Test Waste")]
void ProcessTestWaste()
{
    var waste = new UpdatedWasteItem("Debug Waste", "Earth", WasteRarity.Rare);
    UpdatedWasteProcessor.Instance.ProcessWasteItem(waste);
}
```

## 🚀 Next Steps

After basic setup:

1. **Create ship compartments** that consume specific resources
2. **Add crafting recipes** for ship upgrades using multiple resource types
3. **Implement trading system** using resource values
4. **Add resource generation** from different dimensional sources
5. **Create storage expansion** mechanics using processed resources

Your resource-based economy is now ready to replace the abstract RP/DP system! 🎉