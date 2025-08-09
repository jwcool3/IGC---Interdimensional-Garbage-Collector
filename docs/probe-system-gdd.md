# **Probe System - Game Design Document**

## **1. Overview**

The Probe System enables players to automate waste collection through deploying, upgrading, and managing collection probes. These probes operate autonomously in different locations, providing a steady stream of resources without direct player intervention. The system offers strategic depth through probe placement, efficiency management, and upgrades.

## **2. Core Mechanics**

### **2.1 Probe Properties**

Each probe has several key attributes:

1. **Probe ID**: Unique identifier for tracking specific probes
2. **Location**: Where the probe is currently deployed
3. **Level**: Determines the probe's effectiveness (starts at level 1)
4. **Collection Rate**: How quickly the probe collects waste (items per minute)
5. **Efficiency Multiplier**: Affects the quality and value of collected items

### **2.2 Probe Management**

Players can perform several actions with probes:

1. **Dispatch**: Deploy a new probe to the current location
2. **Recall**: Remove a probe from its location
3. **Upgrade**: Improve a probe's level, rate, and efficiency
4. **Monitor**: View collection statistics and status of all probes

### **2.3 Collection Mechanics**

Probes collect waste automatically based on:

1. **Collection Timer**: Global timer that triggers all probes simultaneously
2. **Collection Rate**: Each probe's individual rate affects collected quantity
3. **Location Modifiers**: Location properties affect collection results
4. **Efficiency Bonus**: Higher efficiency probes find better quality waste

## **3. Technical Implementation**

### **3.1 Key Classes**

1. **ProbeManager** (Singleton)
   - Manages all active probes
   - Handles the global collection timer
   - Provides methods for dispatching, recalling, and upgrading probes

2. **Probe** (Data Class)
   - Contains all probe properties
   - Maintains state information for each deployed probe
   - Calculates collection effectiveness

3. **ProbeUpgradeManager** (Singleton)
   - Manages available probe upgrades
   - Handles upgrade costs and effects
   - Tracks global upgrade levels

4. **ProbeUI**
   - Displays active probes and their status
   - Provides interface for probe management
   - Shows upgrade options and costs

### **3.2 Hierarchy Structure**

```
GameManager
├── ProbeManager (Singleton)
│   └── Active Probes List
└── ProbeUpgradeManager (Singleton)
    ├── ProbeCount Upgrade
    ├── Efficiency Upgrade
    └── Speed Upgrade
```

UI Hierarchy:
```
Canvas
└── ProbesPanel
    ├── ProbeListContainer
    │   ├── ProbeItem_1 (ProbeItemUI)
    │   ├── ProbeItem_2 (ProbeItemUI)
    │   └── ProbeItem_n (ProbeItemUI)
    ├── ProbeControls
    │   ├── DispatchProbeButton
    │   └── ProbeCountText
    └── UpgradesContainer
        ├── ProbeCountUpgrade (ProbeUpgradeItemUI)
        ├── EfficiencyUpgrade (ProbeUpgradeItemUI)
        └── SpeedUpgrade (ProbeUpgradeItemUI)
```

## **4. Probe System Upgrades**

### **4.1 Global Probe Upgrades**

The ProbeUpgradeManager offers three main upgrade paths:

1. **More Probes**
   - Increases maximum number of deployable probes
   - Each level adds one probe slot
   - Base cost: 100 RP, 10 DP, with 1.5x multiplier per level
   - Maximum level: 5 (6 total probes)

2. **Better Efficiency**
   - Improves the value of collected waste
   - Each level adds +20% efficiency multiplier
   - Base cost: 150 RP, 15 DP, with 1.4x multiplier per level
   - Maximum level: 5 (2.0x efficiency at max)

3. **Faster Collection**
   - Decreases collection interval time
   - Each level adds +20% collection rate
   - Base cost: 200 RP, 20 DP, with 1.3x multiplier per level
   - Maximum level: 5 (2.0x speed at max)

### **4.2 Individual Probe Upgrades**

Each deployed probe can be individually upgraded:

| Level | Collection Rate | Efficiency | RP Cost |
|-------|-----------------|------------|---------|
| 1     | Base Rate       | 1.0x       | -       |
| 2     | Base × 1.25     | 1.1x       | 100     |
| 3     | Base × 1.5      | 1.2x       | 200     |
| 4     | Base × 1.75     | 1.3x       | 400     |
| 5     | Base × 2.0      | 1.5x       | 800     |

## **5. UI Implementation**

### **5.1 Probe List Display**

Each probe in the list shows:

- **Probe Name/ID**: Unique identifier (e.g., "Probe #A23F")
- **Location**: Current deployment location
- **Level**: Current probe level
- **Rate**: Collection rate in items per minute
- **Controls**: Buttons for upgrade and recall

### **5.2 Probe Controls**

The main probe control panel includes:

- **Dispatch Button**: Add a new probe at current location
- **Probe Count**: Current/Maximum probes deployed
- **Force Collect Button** (Debug): Manually trigger collection

### **5.3 Upgrade Display**

Each upgrade option shows:

- **Upgrade Name**: Type of upgrade
- **Description**: Effects of the upgrade
- **Current Level**: Current/Maximum level
- **Cost**: Required resources for next level
- **Progress Bar**: Visual representation of upgrade level
- **Upgrade Button**: Purchase next level

### **5.4 Color Coding**

- **Green**: Available/affordable upgrades
- **Red**: Unavailable/unaffordable upgrades
- **Yellow**: Maximum level reached

## **6. Collection Process**

### **6.1 Collection Timing**

The ProbeManager maintains a global collection timer:

1. Timer counts down based on collection interval (default: 60 seconds)
2. When timer reaches zero, all probes collect waste simultaneously
3. Collection interval adjusts based on Speed upgrade level
4. UI shows time until next collection

### **6.2 Collection Results**

When collection occurs:

1. For each active probe:
   - Determine waste types based on probe location
   - Apply location and probe modifiers to generated waste
   - Add waste to player inventory
   - Update collection statistics

2. Update UI to reflect:
   - New inventory items
   - Resources gained
   - Any special discoveries

### **6.3 Location Effects**

Probes are affected by their deployment location:

- **Danger Level**: Affects contamination of collected waste
- **Value Multiplier**: Increases recycling value of items
- **Discovery Rate**: Affects chance for rare items
- **Waste Types**: Determines what dimensions waste comes from

## **7. Integration with Other Systems**

### **7.1 Ship System**

- Engine room upgrades improve probe collection rate
- Scanner improves detection of valuable waste
- Communications increases chance for rare waste
- Stabilizer reduces contamination from probes

### **7.2 Waste Inventory**

- Collected waste goes directly to inventory
- Inventory capacity limits total waste storage
- Stacking applies to similar waste items

### **7.3 Location System**

- Probes can only be dispatched to unlocked locations
- Probe effectiveness varies by location
- Certain locations may have special probe modifiers

### **7.4 Resource System**

- Probe upgrades require Recycling Points and Dimensional Potential
- Collected waste provides resources when processed
- Resource management affects upgrade availability

## **8. Implementation Guide**

### **8.1 Setting Up the Probe System**

1. Create a GameObject named "ProbeManager" in your scene
2. Add the ProbeManager.cs script
3. Create another GameObject for ProbeUpgradeManager
4. Configure initial settings in Inspector:
   - Base Collection Rate
   - Maximum Probe Count
   - Collection Timer

### **8.2 Creating Probe UI**

1. Design a prefab for ProbeItemUI with all necessary components
2. Create a prefab for ProbeUpgradeItemUI
3. Set up ProbeUI script to handle probe list display and controls
4. Connect UI elements to respective managers through events

### **8.3 Testing Considerations**

- Verify probe collection works on timer
- Test probe dispatch/recall functionality
- Ensure upgrades apply proper effects
- Validate maximum probe limits
- Test location-specific modifiers

## **9. Future Expansions**

### **9.1 Specialized Probe Types**

- **Scanner Probes**: Focus on finding rare items
- **Heavy-Duty Probes**: Collect more items but at slower rate
- **Stealth Probes**: Reduced contamination but lower value
- **Research Probes**: Generate knowledge instead of waste

### **9.2 Probe Malfunctions**

- Random malfunction chance based on location danger
- Temporary or permanent probe loss
- Repair mechanics for damaged probes
- Prevention upgrades to reduce malfunction chance

### **9.3 Probe Network Effects**

- Synergy bonuses when multiple probes operate in same location
- Communication network between probes for efficiency boost
- Data sharing between probes in different locations
- Network-wide upgrades affecting all probes

## **10. Best Practices**

1. Keep the UI responsive and informative
2. Provide clear feedback on collection events
3. Balance probe effectiveness with manual collection
4. Ensure probe management remains engaging, not tedious
5. Make strategic probe placement meaningful
