# **Waste Collection System - Game Design Document**

## **1. Overview**

The Waste Collection System forms the primary gameplay loop of the recycling facility game. Players collect, manage, and process interdimensional waste items with unique properties. Each waste item comes from different dimensional origins, possesses varying levels of stability, contamination, and recycling potential, and contributes both to the player's resources and the facility's contamination level.

## **2. Core Mechanics**

### **2.1 Waste Properties**

Each waste item has several key properties:

1. **Name**: The item's identifier
2. **Dimensional Origin**: The dimension the waste comes from (Earth, Technological Waste, Biological Remnants, etc.)
3. **Rarity**: Common, Uncommon, Rare, Epic, or Legendary
4. **Waste Stability**: How stable the item is (affects contamination)
5. **Contamination Level**: How much the item contaminates the facility
6. **Recycling Value**: How many Recycling Points the item yields
7. **Recycling Potential**: How much Dimensional Potential the item provides
8. **Quantity**: How many items are stacked (if stackable)

### **2.2 Collection Mechanics**

Players can collect waste through:

1. **Manual Collection**: Direct player action to collect a waste item
2. **Probe Collection**: Automated collection via deployed probes
3. **Expedition Returns**: Special collection events from expeditions

### **2.3 Waste Generation**

Waste items are procedurally generated based on:

1. **Current Location**: Different locations yield different types of waste
2. **Location Danger Level**: Higher danger means better rewards but more contamination
3. **Location Type**: Specialized dimensions produce unique waste types
4. **Player Upgrades**: Affects the quality and properties of generated waste
5. **Rarity Modifiers**: Ship compartments can influence rarity chances

## **3. Technical Implementation**

### **3.1 Key Classes**

1. **WasteItem**
   - Core data structure for waste items
   - Contains all properties and helper methods
   - Handles stacking logic and value calculations

2. **WasteGenerator**
   - Creates new waste items with appropriate properties
   - Applies modifiers based on location and upgrades
   - Handles procedural name and property generation

3. **WasteInventoryManager**
   - Manages the player's collection of waste items
   - Handles adding, removing, and stacking items
   - Enforces inventory capacity limits

4. **WasteItemDatabase**
   - Contains templates for all possible waste types
   - Organizes items by dimensional origin
   - Provides methods to retrieve items by various criteria

### **3.2 Hierarchy Structure**

```
GameManager
├── WasteGenerator
├── WasteInventoryManager (Singleton)
└── WasteItemDatabase (Singleton)
```

UI Hierarchy:
```
Canvas
└── InventoryPanel
    ├── WasteInventoryHeader
    │   ├── InventoryCountText
    │   └── SortOptions
    ├── WasteItemContainer (Transform)
    │   ├── WasteItemDisplay_1
    │   ├── WasteItemDisplay_2
    │   └── WasteItemDisplay_n
    └── ActionButtons
```

## **4. Waste Types and Origins**

### **4.1 Dimensional Origins**

1. **Earth**
   - Common, mundane waste
   - Low contamination, low value
   - High stability

2. **Technological Waste**
   - Electronic and advanced technological debris
   - Medium contamination, medium-high value
   - Medium stability

3. **Biological Remnants**
   - Organic materials from other dimensions
   - High contamination, medium value
   - Low stability

4. **Quantum Residue**
   - Reality-bending quantum particles
   - Variable contamination, high value
   - Very low stability

5. **Philosophical Byproducts**
   - Conceptual waste from thought dimensions
   - Low contamination, high value
   - High stability

6. **Cosmic Debris**
   - Materials from cosmic-scale events
   - High contamination, very high value
   - Medium stability

7. **Temporal Anomaly**
   - Time-based waste materials
   - Very high contamination, extremely high value
   - Extremely low stability

8. **Ethereal Plane**
   - Energy-based waste from non-material planes
   - Low contamination, high value
   - Medium stability

9. **Archaeological Waste**
   - Historical artifacts from other dimensions
   - Low contamination, very high value
   - Medium stability

### **4.2 Rarity Distribution**

| Rarity    | Probability | Value Multiplier | Visual Color |
|-----------|-------------|------------------|--------------|
| Common    | 70%         | 1x               | Gray         |
| Uncommon  | 20%         | 2x               | Green        |
| Rare      | 7%          | 4x               | Blue         |
| Epic      | 2.5%        | 8x               | Purple       |
| Legendary | 0.5%        | 16x              | Yellow/Gold  |

## **5. UI Implementation**

### **5.1 Waste Item Display**

Each waste item in the inventory is represented by:

- **Icon/Image**: Visual representation based on waste type
- **Name Text**: The name of the waste item
- **Origin Text**: The dimensional origin
- **Stability Text**: Visual indicator of stability level
- **Background Color**: Based on rarity
- **Quantity Badge**: For stacked items (if quantity > 1)
- **Recycle Button**: To process the waste for resources

### **5.2 Collection UI**

The waste collection interface includes:

- **"Collect Waste" Button**: Manually trigger waste collection
- **Total Waste Counter**: Shows current inventory fill level
- **Location Information**: Current location affecting collection
- **Collection Animation**: Visual feedback when collecting

### **5.3 Color Coding**

- **Dimensional Origin Colors**: Each dimension has a distinct color scheme
- **Rarity Colors**: Standard color progression from common to legendary
- **Contamination Level**: Red indicators for high contamination

## **6. Waste Processing**

### **6.1 Recycling Mechanics**

When recycling a waste item:

1. Calculate base Recycling Points from item's recycling value
2. Calculate Dimensional Potential from stability and recycling potential
3. Apply any bonuses from facility upgrades (Recycling Laboratory)
4. Add contamination to the facility based on item's contamination level
5. Remove the item from inventory
6. Update resource counters

### **6.2 Batch Processing**

- Ability to recycle multiple items of the same type
- Option to sort by various criteria (value, stability, origin)
- Quick-recycle options for common items

### **6.3 Contamination Effects**

- Each processed waste item increases facility contamination
- Higher contamination levels reduce recycling efficiency
- Dimensional Stabilization upgrades help reduce contamination
- Extreme contamination can trigger negative events

## **7. Integration with Other Systems**

### **7.1 Location System**

- Different locations yield different waste types
- Location-specific modifiers affect waste properties
- Unlocking new locations provides access to new waste varieties

### **7.2 Probe System**

- Probes automate waste collection
- Probe efficiency affects collected waste quality
- Multiple probes can collect from different locations

### **7.3 Ship System**

- Ship compartments provide bonuses to waste collection
- Communications compartment improves rare waste chance
- Scanner compartment reveals more waste properties
- Stabilizer compartment improves waste stability

### **7.4 Resource System**

- Processed waste provides Recycling Points and Dimensional Potential
- These resources fund facility upgrades and ship improvements
- Resource generation rate depends on waste collection efficiency

## **8. Implementation Guide**

### **8.1 Setting Up the Waste System**

1. Create a GameObject named "WasteManager" in your scene
2. Add WasteInventoryManager and WasteGenerator components
3. Create a separate GameObject for WasteItemDatabase
4. Populate the database with item templates from Resources folder

### **8.2 Creating Waste UI**

1. Design a prefab for WasteDisplay with all necessary components
2. Set up the WasteInventoryUI script to instantiate these prefabs
3. Create a waste collection button connected to GameManager.CollectWaste()
4. Ensure waste displays update properly when items change

### **8.3 Testing Considerations**

- Verify waste generation produces appropriate variety
- Test inventory management with stacking and capacity limits
- Ensure processing correctly awards resources
- Validate contamination effects on the facility

## **9. Future Expansions**

### **9.1 Advanced Waste Types**

- **Unstable Compounds**: Can explode if not processed quickly
- **Dimensional Echoes**: Change properties over time
- **Reactive Materials**: Interact with other waste in inventory

### **9.2 Collection Mechanics**

- **Minigames**: Simple puzzles to improve collection quality
- **Timed Events**: Special collection windows for rare materials
- **Dimensional Rifts**: Random events yielding unique waste

### **9.3 Processing Options**

- **Research**: Study waste instead of recycling for knowledge
- **Refinement**: Process waste in multiple stages for better yields
- **Synthesis**: Combine waste items to create new materials

## **10. Best Practices**

1. Maintain clear visual distinction between waste types
2. Provide adequate information about waste properties to players
3. Balance waste generation to maintain player engagement
4. Ensure waste processing feels rewarding and meaningful
