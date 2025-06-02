# **Location System - Game Design Document**

## **1. Overview**

The Location System allows players to explore and interact with different dimensional areas in the game. Each location has unique properties affecting waste generation, dangers, and rewards. Players progress by unlocking new locations, deploying probes to them, and managing the specific challenges each presents.

## **2. Core Mechanics**

### **2.1 Location Properties**

Each location is defined by several key attributes:

1. **Display Name**: The name shown to the player
2. **Description**: Brief lore and functional description
3. **Location ID**: Unique identifier for the system
4. **Danger Level**: Affects contamination risk and reward quality (0.0-1.0)
5. **Value Multiplier**: Affects recycling value of items found (0.5-2.0)
6. **Discovery Rate Multiplier**: Affects chance to find rare items (0.5-2.0)
7. **Waste Types**: Specific dimensional waste origins allowed in this location
8. **Rarity Distribution**: Chances for finding different rarities of waste

### **2.2 Location Unlocking**

Locations are unlocked through progression:

1. **Starting Location**: Available from the beginning
2. **Waste Collection Requirements**: Unlock new locations by collecting X waste items
3. **Prerequisite Locations**: Some locations require previous locations to be unlocked
4. **Required Items**: Specific waste items may be needed to unlock certain locations
5. **Sector Progression**: Locations are organized into increasingly difficult sectors

### **2.3 Location Travel**

Players can travel between unlocked locations:

1. **Location Selection**: Choose from available locations in the UI
2. **Travel Button**: Confirm travel to the selected location
3. **Current Location Display**: Shows the active location and its properties
4. **Visual Transition**: Provides feedback when changing locations

## **3. Technical Implementation**

### **3.1 Key Classes**

1. **LocationManager** (Singleton)
   - Manages all available and unlocked locations
   - Handles location changes and unlocking logic
   - Tracks current location and provides location data

2. **LocationData** (ScriptableObject)
   - Defines a location's properties and attributes
   - Contains visual elements like icons and background images
   - Stores discovery and unlock requirements

3. **LocationUI**
   - Handles location selection and display
   - Shows location details and travel options
   - Visualizes location properties and status

4. **LocationImageDisplay**
   - Manages the visual representation of the current location
   - Handles transitions between locations
   - Shows location-specific UI elements

### **3.2 Hierarchy Structure**

```
GameManager
└── LocationManager (Singleton)
    ├── StartingLocation (LocationData)
    ├── Location_2 (LocationData)
    ├── Location_3 (LocationData)
    └── Location_n (LocationData)
```

UI Hierarchy:
```
Canvas
└── LocationsPanel
    ├── CurrentLocationDisplay
    │   ├── LocationImage
    │   ├── LocationNameText
    │   ├── DangerLevelText
    │   └── LocationDescriptionText
    ├── LocationButtonContainer (Transform)
    │   ├── LocationButton_1
    │   ├── LocationButton_2
    │   └── LocationButton_n
    ├── LocationInfoPanel
    │   ├── DetailedNameText
    │   ├── DetailedDescriptionText
    │   └── DangerLevelIndicator
    └── TravelButton
```

## **4. Location Types and Sectors**

### **4.1 Sectors Organization**

Locations are organized into sectors of increasing difficulty:

1. **Sector 1: Near Dimensions**
   - Low danger (0.0-0.3)
   - Common waste types (Earth, Technological)
   - Beginner-friendly locations

2. **Sector 2: Mid Dimensions**
   - Medium danger (0.3-0.6)
   - Uncommon waste types (Biological, Quantum)
   - Increased value but higher contamination

3. **Sector 3: Far Dimensions**
   - High danger (0.6-0.8)
   - Rare waste types (Philosophical, Cosmic)
   - High value and rare finds, significant contamination

4. **Sector 4: Extreme Dimensions**
   - Very high danger (0.8-1.0)
   - Legendary waste types (Temporal, Ethereal)
   - Highest value but extreme contamination

### **4.2 Example Locations**

| Name | Sector | Danger | Value Mult | Primary Waste Types | Unlock Requirement |
|------|--------|--------|------------|---------------------|-------------------|
| Recycling Center | 1 | 0.1 | 0.8 | Earth | Starting Location |
| Tech Graveyard | 1 | 0.2 | 1.0 | Technological | 10 waste collected |
| Biological Reserve | 2 | 0.4 | 1.2 | Biological | 30 waste collected |
| Quantum Nexus | 2 | 0.5 | 1.3 | Quantum | Tech Graveyard + 50 waste |
| Thought Labyrinth | 3 | 0.6 | 1.5 | Philosophical | Quantum Nexus + special item |
| Cosmic Rift | 3 | 0.7 | 1.6 | Cosmic | 100 waste collected |
| Temporal Void | 4 | 0.9 | 1.8 | Temporal | All Sector 3 locations |
| Ethereal Plane | 4 | 1.0 | 2.0 | Ethereal | Temporal Void + special item |

## **5. UI Implementation**

### **5.1 Location Selection UI**

The location selection interface includes:

- **Location List**: Scrollable list of available locations
- **Lock Icons**: Visual indicator for locked locations
- **Selection Indicators**: Highlight the currently selected location
- **Location Buttons**: Interactive elements to select locations
- **Current Location Indicator**: Shows which location is active

### **5.2 Location Detail View**

When a location is selected, the detail view shows:

- **Location Name**: Full name of the location
- **Description**: Detailed information about the location
- **Danger Level**: Visual meter showing risk level
- **Available Waste Types**: Icons for waste types found here
- **Travel Button**: Option to travel to this location (if unlocked)

### **5.3 Current Location Display**

A persistent display showing:

- **Location Image**: Visual representation of current location
- **Location Name**: Name of current location
- **Mini-stats**: Small indicators for danger and quality

### **5.4 Visual Elements**

- **Location Icons**: Distinctive icon for each location
- **Background Images**: Full background image for the location view
- **Danger Indicators**: Color-coded warnings for high-danger areas
- **Dimensional Effects**: Visual filters based on location type

## **6. Integration with Other Systems**

### **6.1 Waste Generation**

- Location determines which waste types can be generated
- Danger level affects contamination of collected waste
- Value multiplier increases recycling points from waste
- Discovery rate affects chance for higher rarity items

### **6.2 Probe System**

- Probes can be dispatched to specific locations
- Multiple probes can operate in different locations
- Probe efficiency affected by location properties
- Location-specific challenges for probes

### **6.3 Ship System**

- Bridge compartment improves location discovery
- Scanner compartment reveals more location details
- Engine improves travel capabilities
- Ship upgrades may be required for dangerous locations

### **6.4 Combat System**

- Different enemy types appear based on location
- Location danger level affects enemy strength
- Higher-difficulty locations yield better combat rewards

## **7. Implementation Guide**

### **7.1 Setting Up the Location System**

1. Create a GameObject named "LocationManager" in your scene
2. Attach the LocationManager.cs script
3. Create LocationData assets for each location (ScriptableObjects)
4. Configure starting location and unlocking requirements

### **7.2 Creating Location UI**

1. Create a prefab for LocationButton with all required elements
2. Set up the LocationUI script to populate buttons and handle selection
3. Design the location detail view with appropriate text fields and visuals
4. Implement the current location display with image and text

### **7.3 Testing Considerations**

- Verify location unlocking logic works correctly
- Test location travel mechanics and visual transitions
- Ensure waste generation properly adapts to current location
- Validate discovery rate and value multiplier effects

## **8. Future Expansions**

### **8.1 Advanced Location Features**

- **Weather/Conditions**: Changing states affecting collection
- **Events**: Random or scheduled special occurrences
- **Resources**: Location-specific resources besides waste
- **Hazards**: Location-specific dangers requiring mitigation

### **8.2 Location Progression**

- **Location Leveling**: Locations that improve with repeated visits
- **Resource Depletion**: Locations that change over time with use
- **Dimensional Shifts**: Locations that transform periodically
- **Seasonal Changes**: Time-based variations in locations

### **8.3 Exploration Mechanics**

- **Location Mapping**: Revealing parts of locations over time
- **Hidden Areas**: Secret sub-locations requiring discovery
- **Multi-Zone Locations**: Larger locations with distinct areas
- **Dimensional Depth**: Multiple layers to explore within one location

## **9. Best Practices**

1. Maintain clear visual distinction between location types and sectors
2. Provide adequate information about location properties to players
3. Ensure location progression feels rewarding and meaningful
4. Balance risk vs. reward for higher danger locations
5. Create distinctive visual identity for each location
