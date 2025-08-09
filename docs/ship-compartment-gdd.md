# **Ship Compartment System - Game Design Document**

## **1. Overview**

The Ship Compartment System allows players to manage and upgrade various specialized areas of their vessel. Each compartment provides unique bonuses that enhance different aspects of gameplay. Players must strategically prioritize which compartments to upgrade based on their playstyle and current needs, creating a personalized ship configuration.

## **2. Core Mechanics**

### **2.1 Compartment Types**

The ship contains various specialized compartments:

1. **Engine Room**
   - Improves probe speed and collection rate
   - Enhances travel capabilities

2. **Research Laboratory**
   - Increases recycling efficiency
   - Improves resource generation

3. **Storage Bay**
   - Increases waste inventory capacity
   - Improves organization options

4. **Command Bridge**
   - Enhances location discovery
   - Improves exploration capabilities

5. **Recycling Center**
   - Reduces contamination from waste processing
   - Speeds up recycling operations

6. **Dimensional Stabilizer**
   - Improves waste stability
   - Reduces contamination risks

7. **Communications Array**
   - Increases rare waste discovery chance
   - Improves information gathering

8. **Matter Scanner**
   - Reveals hidden waste properties
   - Enhances detection capabilities

9. **Weapons Bay**
   - Increases attack power in combat
   - Provides offensive capabilities

10. **Shield Generator**
    - Improves defense in combat
    - Reduces damage taken

11. **Combat AI Core**
    - Increases critical hit chance
    - Enhances tactical decision-making

12. **Targeting System**
    - Improves attack speed in combat
    - Enhances precision

### **2.2 Upgrade Progression**

- Each compartment has 5 upgrade levels (1-5)
- Each level provides incrementally better benefits
- Costs increase with each level based on a multiplier (default: 1.5x)
- Special unlocks occur at specific levels (typically levels 3 and 5)
- Visual improvements reflect upgraded compartments

### **2.3 Resource System**

Two primary currencies are required for upgrades:

- **Recycling Points (RP)**: The main currency for basic upgrades
- **Dimensional Potential (DP)**: Advanced currency for higher-tier improvements

Each compartment type has different base costs and scaling factors.

## **3. Technical Implementation**

### **3.1 Key Classes**

1. **ShipManager** (Singleton)
   - Manages all ship compartments
   - Handles upgrade purchasing and effects
   - Updates combat stats and gameplay effects

2. **ShipCompartment**
   - Represents a single compartment
   - Stores current level, max level, costs, and benefits
   - Provides methods for applying effects based on type

3. **CompartmentType** (Enum + Extensions)
   - Defines all possible compartment types
   - Extension methods provide base values and attributes
   - Contains helper methods for UI and effects

4. **ShipUI** / **ShipCompartmentSelector**
   - Visual representation of ship compartments
   - Handles user interaction with compartments
   - Displays upgrade options and costs

### **3.2 Hierarchy Structure**

```
GameManager
└── ShipManager (Singleton)
    ├── Engine (ShipCompartment)
    ├── Lab (ShipCompartment)
    ├── Storage (ShipCompartment)
    ├── Bridge (ShipCompartment)
    └── ... (other compartments)
```

UI Hierarchy:
```
Canvas
└── ShipPanel
    ├── ShipVisualization
    │   ├── CompartmentVisuals
    │   │   ├── Engine (with ShipCompartment component)
    │   │   ├── Lab (with ShipCompartment component)
    │   │   └── ... (other compartment visuals)
    │   └── SelectionIndicator
    ├── CompartmentDetailPanel
    │   ├── CompartmentNameText
    │   ├── CompartmentDescriptionText
    │   ├── LevelText
    │   ├── EffectText
    │   ├── NextLevelPreviewText
    │   ├── CostText
    │   └── UpgradeButton
    └── ResourceDisplay
```

## **4. Compartment Details**

### **4.1 Engine Room**

| Level | Collection Rate Boost | RP Cost | DP Cost | Special Unlock |
|-------|----------------------|---------|---------|----------------|
| 1     | 10%                  | 100     | 10      | -              |
| 2     | 25%                  | 150     | 15      | -              |
| 3     | 40%                  | 225     | 22.5    | Auto-Collection |
| 4     | 55%                  | 337.5   | 33.75   | -              |
| 5     | 70%                  | 506.25  | 50.63   | Warp Drive     |

### **4.2 Research Laboratory**

| Level | Recycling Efficiency | RP Cost | DP Cost | Special Unlock |
|-------|---------------------|---------|---------|----------------|
| 1     | 15%                 | 150     | 15      | -              |
| 2     | 30%                 | 225     | 22.5    | -              |
| 3     | 45%                 | 337.5   | 33.75   | Advanced Analysis |
| 4     | 60%                 | 506.25  | 50.63   | -              |
| 5     | 75%                 | 759.38  | 75.94   | Dimensional Synthesis |

### **4.3 Storage Bay**

| Level | Storage Capacity | RP Cost | DP Cost | Special Unlock |
|-------|-----------------|---------|---------|----------------|
| 1     | 50 units        | 125     | 12      | -              |
| 2     | 70 units        | 187.5   | 18      | -              |
| 3     | 90 units        | 281.25  | 27      | Auto-Sort      |
| 4     | 110 units       | 421.88  | 40.5    | -              |
| 5     | 140 units       | 632.81  | 60.75   | Compression Field |

### **4.4 Command Bridge**

| Level | Discovery Efficiency | RP Cost | DP Cost | Special Unlock |
|-------|---------------------|---------|---------|----------------|
| 1     | 20%                 | 200     | 20      | -              |
| 2     | 35%                 | 300     | 30      | -              |
| 3     | 50%                 | 450     | 45      | Advanced Navigation |
| 4     | 65%                 | 675     | 67.5    | -              |
| 5     | 80%                 | 1012.5  | 101.25  | Dimensional Mapping |

### **4.5 Recycling Center**

| Level | Contamination Reduction | RP Cost | DP Cost | Special Unlock |
|-------|------------------------|---------|---------|----------------|
| 1     | 25%                    | 175     | 17      | -              |
| 2     | 40%                    | 262.5   | 25.5    | -              |
| 3     | 55%                    | 393.75  | 38.25   | Batch Processing |
| 4     | 70%                    | 590.63  | 57.38   | -              |
| 5     | 85%                    | 885.94  | 86.06   | Quantum Recycling |

### **4.6 Dimensional Stabilizer**

| Level | Stability Improvement | RP Cost | DP Cost | Special Unlock |
|-------|----------------------|---------|---------|----------------|
| 1     | 10%                  | 225     | 22      | -              |
| 2     | 20%                  | 337.5   | 33      | -              |
| 3     | 30%                  | 506.25  | 49.5    | Contamination Shield |
| 4     | 40%                  | 759.38  | 74.25   | -              |
| 5     | 50%                  | 1139.06 | 111.38  | Reality Anchor |

### **4.7 Communications Array**

| Level | Rare Find Chance | RP Cost | DP Cost | Special Unlock |
|-------|-----------------|---------|---------|----------------|
| 1     | 15%             | 150     | 15      | -              |
| 2     | 25%             | 225     | 22.5    | -              |
| 3     | 35%             | 337.5   | 33.75   | Dimensional Beacon |
| 4     | 45%             | 506.25  | 50.63   | -              |
| 5     | 55%             | 759.38  | 75.94   | Trade Network |

### **4.8 Matter Scanner**

| Level | Property Visibility | RP Cost | DP Cost | Special Unlock |
|-------|-------------------|---------|---------|----------------|
| 1     | 20%               | 175     | 18      | -              |
| 2     | 35%               | 262.5   | 27      | -              |
| 3     | 50%               | 393.75  | 40.5    | Deep Scanning |
| 4     | 65%               | 590.63  | 60.75   | -              |
| 5     | 80%               | 885.94  | 91.13   | Predictive Analysis |

### **4.9-4.12 Combat Compartments**

*Similar tables for Weapons Bay, Shield Generator, Combat AI Core, and Targeting System with appropriate stats*

## **5. UI Implementation**

### **5.1 Ship Visualization**

The ship visualization shows:

- **Visual Ship Model**: Representation of the ship with distinct compartment areas
- **Selectable Areas**: Clickable regions for each compartment
- **Visual State**: Compartments visually reflect their upgrade level
- **Selection Indicator**: Highlight showing currently selected compartment

### **5.2 Compartment Detail Panel**

When a compartment is selected, the detail panel shows:

- **Compartment Name**: Full name of the compartment
- **Description**: Function and purpose of the compartment
- **Current Level**: Current/Maximum level display
- **Current Effect**: Description of active benefits
- **Next Level Preview**: Benefits of the next upgrade
- **Upgrade Cost**: Required resources for next level
- **Special Unlocks**: Any special features at certain levels

### **5.3 Upgrade Process Flow**

1. Player selects a compartment from the ship visualization
2. Detail panel updates to show compartment information
3. System checks if the player has sufficient resources
4. If affordable, the upgrade button is enabled
5. Player clicks the upgrade button
6. Resources are deducted and compartment level increases
7. Visual and functional effects are updated
8. Special unlocks are announced if applicable

### **5.4 Visual Feedback**

- **Color Coding**: Green for affordable, red for unaffordable, yellow for maxed
- **Progress Indicators**: Visual display of current level progress
- **Upgrade Effects**: Visual effects when upgrade is purchased
- **Special Unlock Announcements**: Highlighted messages for new features

## **6. Integration with Other Systems**

### **6.1 Probe System**

- Engine compartment affects probe collection speed
- Research Lab improves recycling from probe-collected waste
- Communications improves rare find chance for probes

### **6.2 Location System**

- Bridge compartment improves location discovery rate
- Scanner reveals more information about locations
- Engine enables travel to more dangerous locations

### **6.3 Waste Processing**

- Recycling Center reduces contamination from processing
- Stabilizer improves waste stability
- Scanner reveals hidden waste properties

### **6.4 Combat System**

- Weapons Bay increases attack power
- Shield Generator improves defense
- Combat AI increases critical hit chance
- Targeting System improves attack speed

## **7. Implementation Guide**

### **7.1 Setting Up the Ship Manager**

1. Create a GameObject named "ShipManager" in your scene
2. Attach the ShipManager.cs script
3. Create child objects for each compartment type
4. Set up the CompartmentType enum and extensions

### **7.2 Creating Ship Compartments**

1. Design the visual ship layout
2. Add colliders to compartment areas for selection
3. Add ShipCompartment component to each area
4. Configure base settings for each compartment

### **7.3 Building the UI**

1. Create the ShipPanel with visualization area
2. Design the compartment detail panel
3. Set up the ShipCompartmentSelector script
4. Connect UI elements to events from ShipManager

### **7.4 Testing Considerations**

- Verify upgrade effects apply correctly
- Test resource deduction and affordability checks
- Ensure visual state reflects upgrade levels
- Validate special unlock functionality

## **8. Special Unlocks Details**

### **8.1 Engine Room Unlocks**

- **Level 3: Auto-Collection**
  - Probes automatically collect waste without manual activation
  - Collects waste every X minutes based on level

- **Level 5: Warp Drive**
  - Instant travel between known locations
  - Reduces travel time to zero

### **8.2 Research Lab Unlocks**

- **Level 3: Advanced Analysis**
  - Automatically identifies valuable components in waste
  - Shows value prediction before recycling

- **Level 5: Dimensional Synthesis**
  - Create synthetic waste from existing resources
  - Combine waste items for improved properties

### **8.3 Other Compartment Unlocks**

*Similar detailed descriptions for special unlocks of other compartments*

## **9. Future Expansions**

### **9.1 Compartment Specializations**

- Branch upgrades at level 3 into specialized paths
- Allow players to choose between different upgrade effects
- Create unique ships based on specialization choices

### **9.2 Compartment Synergies**

- Bonus effects when certain compartments are upgraded together
- Special combinations unlocking hidden features
- Cross-compartment benefits

### **9.3 Crew System**

- Assign crew members to compartments
- Crew skills enhance compartment effectiveness
- Crew training and specialization

## **10. Best Practices**

1. Maintain clear visual distinction between compartment types
2. Provide adequate feedback about upgrade effects
3. Balance upgrade costs for smooth progression
4. Ensure special unlocks feel meaningful and impactful
5. Create a cohesive visual identity for the ship
