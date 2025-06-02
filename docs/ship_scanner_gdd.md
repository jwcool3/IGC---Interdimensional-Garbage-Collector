# **Ship Scanner & Contacts System - Game Design Document**

## **1. Overview**

The Ship Scanner & Contacts System introduces dynamic ship discovery and interaction mechanics to the dimensional recycling game. Players use energy-based scanning to discover unique ships of varying rarities, then engage with them through combat or trading. The system creates emergent gameplay through randomly generated encounters with persistent ship relationships.

## **2. Core Mechanics**

### **2.1 Scanner Energy System**

The scanner operates on a limited energy resource:

1. **Maximum Energy**: 100 units total capacity
2. **Energy Regeneration**: 5 units per second automatic restoration
3. **Scan Costs**:
   - Common Scan: 15 energy (80% base success rate)
   - Rare Scan: 35 energy (30% base success rate)
4. **Success Rate Modifiers**:
   - Scanner compartment level: +10% per level
   - Location danger level: Reduces success rates
   - Bridge compartment: Improves discovery chances

### **2.2 Ship Rarity Categories**

Ships are divided into two main discovery categories:

**Common Category (Common Scans)**:
- Very Common (70% chance): Basic scavenger vessels
- Common (20% chance): Standard trading ships
- Slightly Rare (10% chance): Enhanced civilian craft

**Rare Category (Rare Scans)**:
- Rare (60% chance): Military or specialized vessels
- Epic (30% chance): Advanced technology ships
- Legendary (9% chance): Unique prototype vessels
- Anomaly (1% chance): Mysterious dimensional entities

### **2.3 Ship Properties**

Each discovered ship has comprehensive attributes:

1. **Basic Information**:
   - Ship Name: Procedurally generated unique identifier
   - Ship Type: Category description (Battle Cruiser, Trading Pod, etc.)
   - Rarity: Visual and mechanical rarity classification
   - Level: Determines stat scaling and reward value

2. **Combat Statistics**:
   - Attack Power: Offensive capability in combat
   - Defense: Damage reduction against player attacks
   - Health: Total and current hit points
   - Combat behavior varies by ship type

3. **Trading Properties**:
   - Trade Value: Number of waste items required
   - Available Items: Ship parts, alien tech, or special resources
   - Trade relationship status

4. **Interaction State**:
   - Can Fight: Available for combat encounter
   - Can Trade: Available for resource exchange
   - Has Fought: Previously engaged in combat
   - Has Traded: Previously completed trade
   - Is Destroyed: Defeated in combat

### **2.4 Ship Interaction Types**

Players can interact with discovered ships in two ways:

**Combat Encounters**:
- Engage in single-ship combat mode
- Victory provides combat rewards (ship parts, alien tech, combat data)
- Defeat marks ship as destroyed
- Higher rarity ships provide better rewards

**Trading Exchanges**:
- Exchange waste items for valuable resources
- Fixed costs based on ship rarity and level
- One-time transaction per ship
- Alternative to combat for resource acquisition

## **3. Technical Implementation**

### **3.1 Key Classes**

1. **ShipScanner** (Singleton)
   - Manages energy system and scanning operations
   - Generates discovered ships based on scan type
   - Handles scan timing and success calculations
   - Tracks current discovered ship state

2. **DiscoveredShip**
   - Contains all ship properties and stats
   - Manages interaction state (fought/traded)
   - Converts to EnemyShip for combat integration
   - Provides UI display information

3. **ShipInteractionManager** (Singleton)
   - Handles combat and trading mechanics
   - Integrates with CombatManager for single-ship encounters
   - Manages reward distribution
   - Tracks interaction outcomes

4. **ScannerUI**
   - Energy display and scanning controls
   - Scan progress visualization
   - Discovery result presentation
   - Quick action buttons (Fight Now, View Contacts)

5. **ContactsUI**
   - List management for all discovered ships
   - Ship selection and detail viewing
   - Interaction buttons (Fight/Trade/Dismiss)
   - Ship status visualization

6. **ContactItemUI**
   - Individual ship display in contacts list
   - Health bars and status icons
   - Selection state management
   - Visual feedback for ship condition

### **3.2 Hierarchy Structure**

```
GameManager
├── ShipScanner (Singleton)
├── ShipInteractionManager (Singleton)
└── ScannerTester (Development/Testing)

UI Canvas
├── ScannerPanel
│   ├── EnergyDisplay
│   │   ├── EnergySlider
│   │   ├── EnergyText
│   │   └── RegenRateText
│   ├── ScanControls
│   │   ├── CommonScanButton
│   │   │   ├── CostText
│   │   │   └── SuccessRateText
│   │   ├── RareScanButton
│   │   │   ├── CostText
│   │   │   └── SuccessRateText
│   │   └── ScanProgressSlider
│   ├── ScanStatus
│   │   ├── ScanningIndicator
│   │   ├── StatusText
│   │   └── ScanningEffect
│   ├── ResultPanel
│   │   ├── ResultText
│   │   ├── ViewContactsButton
│   │   └── FightNowButton
│   └── ScannerInfo
│       ├── ScannerLevelText
│       └── LocationBonusText
└── ContactsPanel
    ├── ContactsList
    │   ├── ContactsListContent (ScrollRect)
    │   └── ContactItemPrefab (instantiated)
    │       ├── BackgroundImage
    │       ├── ShipIcon
    │       ├── ShipNameText
    │       ├── RarityBadge
    │       ├── HealthBar
    │       └── StatusIcons
    ├── ContactDetails
    │   ├── ShipInfoPanel
    │   │   ├── ShipNameText
    │   │   ├── ShipTypeText
    │   │   ├── RarityText
    │   │   └── StatsDisplay
    │   ├── TradeInfo
    │   │   ├── TradeValueText
    │   │   └── TradeItemsText
    │   └── ActionButtons
    │       ├── FightButton
    │       ├── TradeButton
    │       └── DismissButton
    └── HeaderInfo
        ├── TitleText
        ├── ContactCountText
        └── NoContactsMessage
```

## **4. Scanning Process Flow**

### **4.1 Energy Management**

1. **Energy Check**: Verify sufficient energy for scan type
2. **Energy Consumption**: Deduct scan cost immediately
3. **Continuous Regeneration**: Energy restores automatically over time
4. **UI Feedback**: Real-time energy display and availability indicators

### **4.2 Scanning Operation**

1. **Scan Initiation**:
   - Player clicks Common or Rare scan button
   - System validates energy requirements and ship status
   - Scanning animation and progress bar begin

2. **Scan Duration**: 3-second scanning period with visual feedback

3. **Success Calculation**:
   - Base success rate for scan type
   - Scanner compartment level bonus
   - Location difficulty modifier
   - Random roll determines outcome

4. **Result Processing**:
   - Success: Generate DiscoveredShip with appropriate rarity
   - Failure: Show failure message and reset for next scan

### **4.3 Ship Generation**

When a scan succeeds:

1. **Rarity Determination**: Based on scan type and probability tables
2. **Stat Calculation**: Level, combat stats, and trade value based on rarity
3. **Name Generation**: Procedural naming system with rarity-appropriate prefixes
4. **Property Assignment**: Trade items, ship type, and visual elements
5. **State Initialization**: Set to available for both combat and trading

## **5. Contact Management System**

### **5.1 Contact List Features**

The contacts system maintains all discovered ships:

1. **Persistent Storage**: Ships remain available until dismissed or destroyed
2. **Status Tracking**: Visual indicators for interaction availability
3. **Sorting Options**: Organize by rarity, level, or interaction status
4. **Search/Filter**: Find specific ships quickly (future enhancement)

### **5.2 Ship Status Visualization**

Each contact displays:

1. **Health Bar**: Current/maximum health with color coding
2. **Status Icons**:
   - Can Fight: Red combat icon
   - Can Trade: Green trade icon
   - Defeated: Gray destruction icon
3. **Background Color**: Changes based on selection and ship status
4. **Rarity Badge**: Color-coded rarity indicator

### **5.3 Interaction Buttons**

Contact detail panel provides:

1. **Fight Button**:
   - Available if ship can fight
   - Shows "FOUGHT" or "DESTROYED" if unavailable
   - Initiates single-ship combat encounter

2. **Trade Button**:
   - Available if ship can trade and player has enough waste
   - Shows "TRADED" if already completed
   - Displays trade requirements and player's current waste

3. **Dismiss Button**:
   - Removes ship from contacts list
   - Permanent action (ship cannot be recovered)
   - Available for any non-destroyed ship

## **6. Combat Integration**

### **6.1 Single Ship Combat Mode**

When fighting a discovered ship:

1. **Combat Setup**:
   - Convert DiscoveredShip to EnemyShip format
   - Preserve original stats and properties
   - Set CombatManager to single-ship mode

2. **UI Transitions**:
   - Automatic switch to Combat tab
   - Special UI indicators for single-ship encounters
   - Yellow highlighting and "⚔️ Encounter" text

3. **Combat Resolution**:
   - Victory: Ship marked as destroyed, rewards distributed
   - Defeat: Ship marked as fought but remains alive
   - Return to Contacts tab after resolution

### **6.2 Combat Rewards**

Defeating discovered ships provides enhanced rewards:

- **Ship Parts**: 15-50+ based on rarity and level
- **Alien Tech**: 5-25+ for rare ships
- **Combat Data**: Tactical information for future encounters
- **Recycling Points**: 25-200+ standard currency
- **Dimensional Potential**: 5-25+ advancement currency

Rewards scale with ship rarity and level for meaningful progression.

## **7. Trading System**

### **7.1 Trade Requirements**

Each ship has specific trade demands:

1. **Waste Item Cost**: Number of waste items required (10-50+ based on rarity)
2. **Trade Validation**: Check player inventory for sufficient items
3. **Item Removal**: Remove required waste from player inventory
4. **Reward Distribution**: Provide ship parts, alien tech, or recycling points

### **7.2 Trade Rewards**

Trading provides different resource distribution than combat:

- **Lower Risk**: No combat danger, guaranteed success
- **Resource Focus**: Primarily ship parts and recycling points
- **Efficiency**: Convert excess waste into useful resources
- **Alien Tech**: Available from rare category ships only

### **7.3 Trade Information Display**

The trade UI shows:

1. **Requirements**: "Wants: X waste items"
2. **Player Status**: "You have: Y items"
3. **Affordability**: "✓ Can Afford" or "✗ Cannot Afford"
4. **Offered Rewards**: List of items the ship provides

## **8. UI Implementation Guide**

### **8.1 Setting Up Scanner UI**

1. **Create ScannerPanel GameObject** under Canvas
2. **Add ScannerUI script** to the panel
3. **Create Energy Display section**:
   - Add Slider for energy bar (set Max Value to 100)
   - Add TextMeshPro for energy text display
   - Add TextMeshPro for regeneration status

4. **Create Scan Controls section**:
   - Add Button for Common Scan (assign cost and success rate texts)
   - Add Button for Rare Scan (assign cost and success rate texts)
   - Add Slider for scan progress (set Max Value to 1)

5. **Create Result Panel**:
   - Add Panel GameObject (initially disabled)
   - Add TextMeshPro for result text
   - Add Button for "View Contacts"
   - Add Button for "Fight Now" (visible only when ship discovered)

6. **Assign all references** in ScannerUI script inspector

### **8.2 Setting Up Contacts UI**

1. **Create ContactsPanel GameObject** under Canvas
2. **Add ContactsUI script** to the panel
3. **Create Contacts List**:
   - Add ScrollRect for scrollable contact list
   - Set up Content area with VerticalLayoutGroup
   - Create ContactItemPrefab with all visual elements

4. **Create Contact Details Panel**:
   - Add ship info display (name, type, rarity, stats)
   - Add trade information section
   - Add action buttons (Fight/Trade/Dismiss)

5. **Create Header Information**:
   - Add title text and contact counter
   - Add "no contacts" message for empty state

6. **Design ContactItemPrefab**:
   - Background Image with selection highlighting
   - Ship icon placeholder
   - Ship name and type text
   - Rarity badge with color coding
   - Health bar with fill amount
   - Status icons for combat/trade availability

### **8.3 Setting Up Manager Scripts**

1. **Create ShipScanner GameObject**:
   - Add ShipScanner.cs script
   - Configure energy settings in inspector
   - Set scan costs and success rates

2. **Create ShipInteractionManager GameObject**:
   - Add ShipInteractionManager.cs script
   - Configure combat and trade settings

3. **Optional: Create ScannerTester GameObject**:
   - Add ScannerTester.cs script for keyboard testing
   - Enable in development builds only

## **9. Integration Points**

### **9.1 Resource System Integration**

- **ResourceManager**: Handles all reward distribution
- **WasteInventoryManager**: Manages waste item trading
- **Scanner rewards**: Ship parts, alien tech, combat data
- **Trade costs**: Waste items from player inventory

### **9.2 Combat System Integration**

- **CombatManager**: Single-ship combat mode
- **CombatUI**: Special indicators for discovered ship encounters
- **TabSystem**: Automatic switching between Scanner/Contacts/Combat
- **Victory/defeat handling**: Ship state updates and cleanup

### **9.3 Ship Compartment Integration**

- **Scanner Compartment**: Improves scan success rates and reveals ship details
- **Communications Compartment**: Increases rare ship discovery chances
- **Bridge Compartment**: Enhances general discovery rate and location bonuses

### **9.4 Location System Integration**

- **Current Location**: Affects enemy types and discovery rates
- **Danger Level**: Modifies scan success rates and ship generation
- **Location Properties**: Influence available ship types and encounter chances

## **10. Testing and Development**

### **10.1 ScannerTester Controls**

For development and testing, use keyboard shortcuts:

- **Q**: Perform common scan
- **E**: Perform rare scan  
- **F**: Fight current discovered ship
- **T**: Trade with current discovered ship
- **C**: Clear current discovered ship
- **I**: Show scanner debug information

### **10.2 Testing Scenarios**

1. **Energy Management**: Verify energy consumption and regeneration
2. **Scan Success Rates**: Test probability distributions for ship generation
3. **Combat Integration**: Ensure single-ship combat works correctly
4. **Trade Mechanics**: Validate waste item costs and reward distribution
5. **UI State Management**: Test all button states and panel transitions
6. **Ship Persistence**: Verify ships remain in contacts list correctly

## **11. Future Enhancements**

### **11.1 Advanced Scanner Features**

- **Deep Scan Mode**: Higher energy cost for detailed ship information
- **Passive Scanning**: Automatic background ship detection
- **Scanner Upgrades**: Equipment that improves scanning capabilities
- **Scanner Range**: Different energy costs based on location distance

### **11.2 Enhanced Ship Interactions**

- **Diplomacy System**: Build relationships with non-hostile ships
- **Ship Crews**: Hire crew members from defeated or allied ships
- **Ship Salvage**: Recover ship parts from destroyed vessels
- **Fleet Encounters**: Multiple ships discovered simultaneously

### **11.3 Contact Management Features**

- **Contact Categories**: Organize ships by type, faction, or relationship
- **Contact History**: Track previous interactions and outcomes
- **Reputation System**: Ship reactions based on player's combat record
- **Contact Expiration**: Ships that leave the area after time limits

## **12. Best Practices**

1. **Clear Visual Feedback**: Ensure all scanner states are clearly communicated
2. **Meaningful Choices**: Balance combat vs. trading risk/reward ratios
3. **Progressive Complexity**: Start with simple encounters, build to complex ships
4. **Resource Balance**: Ensure scanner-acquired resources complement other systems
5. **UI Responsiveness**: Provide immediate feedback for all player actions
6. **State Persistence**: Maintain ship data consistently across game sessions