# Location-Based Expedition System Development Roadmap

## Overview
The expedition system transforms the game from a simple waste collector to a progressive location-based adventure. Players start on Earth collecting ordinary trash and gradually unlock new locations with unique waste types and mechanics.

## Core Goals
- Create a sense of progression through location unlocks
- Introduce variety in waste types based on location
- Establish risk/reward mechanics through location properties
- Build foundation for future expansion systems

## Development Phases

### Phase 1: Location Framework Setup
#### Objectives
- Implement basic location data structure
- Create location switching mechanics
- Establish location-specific waste generation

#### Key Components
1. **Location Data System**
   - Create Location ScriptableObject template
   - Define properties: name, ID, waste types, unlock requirements
   - Implement location manager to track current/available locations

2. **Basic Location UI**
   - Add "Current Location" display to main screen
   - Create simple location selection interface
   - Implement location switching functionality

3. **Location-Based Waste Generation**
   - Modify waste generator to use location-specific tables
   - Create waste pools for Earth and Space Station (initial locations)
   - Ensure proper waste type distribution per location

### Phase 2: Core Location Mechanics
#### Objectives
- Implement location unlock system
- Add location properties and effects
- Create progression path through locations

#### Key Components
1. **Unlock System**
   - Implement collection milestone tracking
   - Create unlock conditions (items collected, special discoveries)
   - Add unlock notifications and UI feedback

2. **Location Properties**
   - Add danger level system
   - Implement average value modifiers
   - Create discovery rate mechanics

3. **Starting Locations**
   ```
   Earth → Space Station Alpha → Mars Colony
   ```
   - Define unique waste types for each
   - Set progression requirements
   - Balance risk/reward ratios

### Phase 3: Enhanced Location Features
#### Objectives
- Add environmental effects
- Implement location mastery system
- Create special discovery items

#### Key Components
1. **Environmental Factors**
   - Create modifiers for collection efficiency
   - Add visual effects for different environments
   - Implement temporary location events

2. **Location Mastery**
   - Track player progress per location
   - Add bonuses for frequent collection
   - Create "Local Expert" status benefits

3. **Discovery System**
   - Implement rare "unlock items" in waste pools
   - Create branching paths for secret locations
   - Add excitement to routine collection

### Phase 4: UI and Polish
#### Objectives
- Create polished location selection interface
- Add visual feedback for location changes
- Implement smooth transitions

#### Key Components
1. **Location Map Screen**
   - Design visual map interface
   - Show location connections and requirements
   - Display completion progress

2. **Travel Interface**
   - Create transition animations
   - Add confirmation dialogs
   - Show location preview information

3. **Progress Visualization**
   - Location completion meters
   - Unlock progress indicators
   - Achievement notifications

## Technical Implementation Plan

### 1. Data Structure Setup
```
LocationData (ScriptableObject)
├── Basic Properties
│   ├── locationID
│   ├── displayName
│   ├── description
│   └── unlockRequirements
├── Gameplay Properties
│   ├── dangerLevel
│   ├── averageValue
│   └── discoveryRate
└── Waste Configuration
    ├── wasteTypePool
    ├── rarityModifiers
    └── specialItems
```

### 2. Core Systems
- **LocationManager**: Handles current location and available locations
- **LocationUnlockSystem**: Tracks progress and manages unlocks
- **LocationEffectsController**: Applies environmental modifiers

### 3. UI Components
- **LocationButton**: Individual location selection button
- **LocationMapPanel**: Full map interface
- **LocationInfoDisplay**: Detailed location information panel

## Implementation Timeline

### Week 1: Foundation
- Set up location data structures
- Implement basic location switching
- Create Earth and Space Station waste pools

### Week 2: Core Mechanics
- Build unlock system
- Add location properties
- Implement progression tracking

### Week 3: UI Development
- Create location selection interface
- Add visual feedback systems
- Implement location info displays

### Week 4: Polish and Testing
- Balance location progression
- Add environmental effects
- Test and refine user experience

## Success Metrics
- Players understand location progression
- Clear sense of advancement when unlocking locations
- Variety in gameplay between locations
- Smooth user experience when switching locations

## Future Expansion Possibilities
- Location upgrades and outposts
- Timed events and challenges
- Dynamic location properties
- Cross-location waste interactions

## Dependencies
- Basic inventory system must be complete
- Waste generation system must be functional
- Core UI framework must be in place

## Risk Mitigation
- Start with simple location switching
- Add complexity incrementally
- Test progression curve early
- Get player feedback on unlock pacing

## Next Steps
1. Create LocationData ScriptableObject template
2. Implement basic Earth location
3. Add location display to main UI
4. Test location-based waste generation
5. Build simple location switching mechanic