# **Combat System - Game Design Document**

## **1. Overview**

The Combat System introduces strategic ship-to-ship battles in the dimensional recycling game. Players engage with various enemy types across different sectors, gaining valuable resources and progression opportunities. The system features turn-based combat with strategic depth, ship upgrades, and zone-based progression.

## **2. Core Mechanics**

### **2.1 Combat Stats**

Each ship (player and enemy) has several core combat attributes:

1. **Attack Power**
   - Base damage dealt per attack
   - Modified by ship compartments and upgrades
   - Determines offensive capability

2. **Defense**
   - Damage reduction from incoming attacks
   - Acts as a percentage-based reduction
   - Modified by ship compartments

3. **Hit Points (HP)**
   - Ship's health/durability
   - When reduced to zero, ship is defeated
   - Player HP persists between battles (limited healing)

4. **Attack Speed**
   - Rate of attacks in auto-combat mode
   - Determines turns per minute
   - Modified by targeting systems

5. **Critical Chance**
   - Probability of landing a critical hit (double damage)
   - Modified by Combat AI compartment
   - Strategic component for damage spikes

### **2.2 Combat Flow**

The battle system follows a turn-based approach:

1. **Manual Control**
   - Player clicks Attack button to initiate attacks
   - Provides full control over timing
   - Allows strategic consideration between attacks

2. **Auto-Combat**
   - Toggle option for automatic attacks
   - Attacks occur at intervals based on attack speed
   - Hands-off option for easier encounters

3. **Enemy Attacks**
   - Enemies counter-attack after player attacks
   - Attack patterns vary by enemy type
   - Strategic consideration of enemy behavior

4. **Visual Feedback**
   - Attack animations (laser beams, impacts)
   - HP bar updates
   - Critical hit effects
   - Victory/defeat notifications

### **2.3 Enemy Types**

Four distinct enemy categories with unique attributes:

1. **Scavenger**
   - Common enemy (60% probability)
   - Lower HP and attack
   - Balanced defense
   - Gray color scheme
   - Drops primarily Ship Parts

2. **Rival**
   - Uncommon enemy (25% probability)
   - Balanced stats with higher attack
   - Blue color scheme
   - Good source of all resources

3. **Anomaly**
   - Rare enemy (10% probability)
   - High attack, low HP
   - Pink color scheme
   - Primary source of Alien Tech

4. **Boss**
   - Special encounter (5% probability, guaranteed every 10 enemies)
   - High HP, attack, and defense
   - Red color scheme
   - Drops all resource types in large quantities

## **3. Technical Implementation**

### **3.1 Key Classes**

1. **CombatManager** (Singleton)
   - Controls the combat state and flow
   - Manages player and enemy stats
   - Handles turn processing and damage calculation
   - Tracks zone progression

2. **EnemyShip**
   - Contains enemy properties and behavior
   - Calculates attack damage
   - Stores enemy visual elements
   - Handles defeat conditions

3. **CombatZone**
   - Defines a set of enemies in a particular area
   - Contains zone progression and completion logic
   - Manages enemy distribution and types
   - Tracks zone-specific rewards

4. **CombatUI**
   - Updates visual elements during combat
   - Manages attack animations and effects
   - Displays ship stats and health
   - Handles user input for combat actions

5. **ShipDatabase** / **EnemyIconManager**
   - Store visual assets for different enemy types
   - Manage icons based on enemy type and sector
   - Handle fallback visuals when specific assets unavailable

### **3.2 Hierarchy Structure**

```
GameManager
├── CombatManager (Singleton)
│   ├── CurrentZone (CombatZone)
│   ├── AvailableZones (List<CombatZone>)
│   └── CurrentEnemy (EnemyShip)
├── ShipDatabase (Singleton)
└── EnemyIconManager (Singleton)
```

UI Hierarchy:
```
Canvas
└── CombatPanel
    ├── ResourceDisplay
    ├── CombatView
    │   ├── PlayerShip
    │   │   ├── ShipImage
    │   │   └── HPBar
    │   ├── EnemyShip
    │   │   ├── ShipImage
    │   │   ├── TypeIcon
    │   │   └── HPBar
    │   └── Effects
    │       ├── LaserBeam
    │       ├── ImpactEffect
    │       └── CriticalHitEffect
    ├── ShipStats
    ├── EnemyInfo
    ├── ZoneProgress
    └── Controls
        ├── AttackButton
        ├── AutoToggle
        └── RetreatButton
```

## **4. Combat Zones and Sectors**

### **4.1 Sector Organization**

Combat zones are organized into sectors of increasing difficulty:

1. **Sector 1: Scavenger Territory**
   - Primarily Scavenger enemies (80%)
   - Low enemy levels (1-3)
   - Easy introduction to combat
   - Basic resource rewards

2. **Sector 2: Contested Space**
   - Mix of Scavengers (60%) and Rivals (30%)
   - Medium enemy levels (4-7)
   - Moderate difficulty
   - Improved resource drops

3. **Sector 3: Anomaly Cluster**
   - Balanced mix of all enemy types
   - Higher enemy levels (8-12)
   - Challenging encounters
   - Valuable resource rewards

4. **Sector 4: Deep Space**
   - Primarily Rivals and Anomalies
   - Very high enemy levels (13-20)
   - Extremely challenging
   - Premium resource rewards

### **4.2 Zone Structure**

Each zone has specific attributes:

- **Zone Name**: Descriptive identifier
- **Sector Number**: Which sector it belongs to (1-4)
- **Enemy Count**: Number of enemies to defeat to complete the zone
- **Enemy Distribution**: Percentage breakdown of enemy types
- **Level Range**: Min/max enemy levels in this zone
- **Lock Status**: Whether the zone is available or needs unlocking
- **Completion Rewards**: Special bonuses for clearing the zone

### **4.3 Example Zones**

| Zone Name | Sector | Enemies | Scavenger % | Rival % | Anomaly % | Level Range | Unlock Requirement |
|-----------|--------|---------|------------|---------|-----------|-------------|-------------------|
| Alpha Sector | 1 | 10 | 80 | 15 | 5 | 1-2 | Starting Zone |
| Beta Sector | 1 | 10 | 70 | 20 | 10 | 2-3 | Alpha Complete |
| Gamma Sector | 2 | 15 | 60 | 30 | 10 | 4-5 | Beta Complete |
| Delta Sector | 2 | 15 | 50 | 35 | 15 | 5-7 | Gamma Complete |
| Epsilon Sector | 3 | 20 | 40 | 35 | 25 | 8-10 | Delta Complete |
| Omega Sector | 4 | 25 | 20 | 40 | 40 | 13-15 | All Sector 3 Complete |

## **5. Resource Generation**

### **5.1. Combat Rewards**

Each defeated enemy provides resources:

1. **Base Rewards by Enemy Type**

   | Enemy Type | Ship Parts | Alien Tech | Combat Data | RP | DP |
   |------------|------------|------------|-------------|----|----|
   | Scavenger  | 5-10       | 0-2        | 2-5         | 50 | 5  |
   | Rival      | 10-20      | 3-7        | 5-10        | 75 | 10 |
   | Anomaly    | 5-15       | 8-15       | 15-20       | 100| 15 |
   | Boss       | 25-50      | 15-25      | 20-30       | 200| 25 |

2. **Level Multipliers**
   - Resources are multiplied by (1 + 0.1 × enemy level)
   - Higher-level enemies are significantly more rewarding
   - Creates progression incentive

3. **Zone Completion Bonuses**
   - One-time bonus for first completion of a zone
   - Scaled based on zone difficulty
   - Includes special resources or upgrades

### **5.2 Resource Applications**

Combat resources are used for:

1. **Ship Parts**
   - Basic ship repairs
   - Physical compartment upgrades
   - Structural improvements

2. **Alien Tech**
   - Advanced compartment upgrades
   - Special weapon systems
   - Unique ship capabilities

3. **Combat Data**
   - Tactical upgrades
   - Enemy weakness analysis
   - Combat efficiency improvements

## **6. UI Implementation**

### **6.1 Combat Screen Layout**

The main combat interface includes:

1. **Ship Visualization**
   - Player ship on left side
   - Enemy ship on right side
   - Visual representation of ship types

2. **HP Bars**
   - Horizontal bars showing current/maximum health
   - Color coding (green to red) for health status
   - Numerical display of current/max HP

3. **Combat Effects**
   - Laser beams between ships during attacks
   - Impact effects on receiving ship
   - Critical hit visual/sound effects
   - Victory/defeat animations

4. **Enemy Information**
   - Enemy name and level
   - Enemy type icon
   - Brief description or stats

### **6.2 Zone and Progress Display**

Shows the current battle context:

1. **Zone Information**
   - Current zone name
   - Sector number
   - Progress indicator (X/Y enemies defeated)

2. **Zone Selection**
   - Dropdown or buttons to change zones
   - Indicator for locked/unlocked status
   - Preview of zone attributes

### **6.3 Controls**

User interface for combat actions:

1. **Attack Button**
   - Prominent, centered button
   - Initiates manual attack
   - Visual feedback when clicked

2. **Auto-Combat Toggle**
   - Switch to enable/disable automatic attacks
   - Visual indicator of active state
   - Configurable attack speed

3. **Retreat Button**
   - Option to leave current zone
   - Return to safer area
   - Strategic option for difficult encounters

### **6.4 Stats Display**

Shows current ship combat attributes:

1. **Attack Power**: Numerical value with any modifiers
2. **Defense**: Damage reduction percentage
3. **HP**: Current/maximum health
4. **Attack Speed**: Attacks per minute
5. **Critical Chance**: Percentage probability

## **7. Integration with Other Systems**

### **7.1 Ship Compartment System**

Combat effectiveness is enhanced by ship upgrades:

1. **Weapons Bay**
   - Increases attack power
   - Unlocks special attack patterns
   - Primary offensive upgrade

2. **Shield Generator**
   - Improves defense
   - Reduces damage taken
   - Primary defensive upgrade

3. **Combat AI Core**
   - Increases critical hit chance
   - Improves tactical decisions
   - Enhances overall damage output

4. **Targeting System**
   - Improves attack speed
   - Increases accuracy
   - Enhances combat efficiency

### **7.2 Resource System**

Combat is a key source of specialized resources:

1. **Primary Combat Resources**
   - Ship Parts, Alien Tech, Combat Data
   - Exclusive to combat encounters
   - Essential for advanced upgrades

2. **Standard Resources**
   - Supplementary source of RP and DP
   - Alternative to waste processing
   - Diversifies resource generation

### **7.3 Tab System**

Combat is integrated into the main game interface:

1. **Combat Tab**
   - Accessible from the main tab interface
   - Dedicated screen for combat activities
   - Clearly separated from other gameplay

2. **Notification System**
   - Alerts for new zone unlocks
   - Warnings for high-level enemies
   - Celebration of zone completion

## **8. Implementation Guide**

### **8.1 Setting Up the Combat Manager**

1. Create a GameObject named "CombatManager" in your scene
2. Attach the CombatManager.cs script
3. Configure initial combat zones in the Inspector
4. Set up player's initial combat stats

### **8.2 Setting Up Enemy Visualization**

1. Create a GameObject named "ShipDatabase"
2. Attach the ShipDatabase.cs script
3. Create another GameObject named "EnemyIconManager"
4. Attach the EnemyIconManager.cs script
5. Organize enemy sprites in the Resources folder:
   - `Resources/EnemyIcons/Default/`
   - `Resources/EnemyIcons/Sector1/`
   - `Resources/EnemyIcons/Sector2/`
   - etc.

### **8.3 Creating the Combat UI**

1. Create a "CombatPanel" under your Canvas
2. Add the CombatUI.cs script to this panel
3. Set up all child elements following the hierarchy in Section 3.2
4. Connect UI elements to the CombatUI script in the Inspector

### **8.4 Testing Considerations**

- Test combat balance across different enemy types
- Verify resource rewards are granted correctly
- Ensure zone progression works properly
- Test auto-combat functionality
- Validate visual effects and animations

## **9. Future Expansions**

### **9.1 Advanced Combat Mechanics**

- **Special Abilities**: Unique attacks with cooldowns
- **Counter System**: Timing-based defense mechanic
- **Weakness Exploitation**: Target specific enemy vulnerabilities
- **Formation Battles**: Multiple enemies simultaneously

### **9.2 Environmental Factors**

- **Dimensional Storms**: Affect combat stats periodically
- **Debris Fields**: Chance to block attacks
- **Radiation Zones**: Damage over time to both ships
- **Unstable Space**: Random stat fluctuations

### **9.3 Specialized Enemy Types**

- **Carrier**: Spawns smaller ships during battle
- **Engineer**: Self-repairs during combat
- **Stealth**: Temporarily becomes untargetable
- **Dimensional Shifter**: Changes attack patterns randomly

## **10. Best Practices**

1. Maintain clear visual distinction between enemy types
2. Provide adequate feedback for all combat actions
3. Balance combat difficulty for satisfying progression
4. Ensure zone rewards justify the increased challenge
5. Make strategic choices meaningful and impactful
