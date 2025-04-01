# Medieval Dungeon Crawler - Setup Guide

## Table of Contents
1. [Project Setup](#project-setup)
2. [Core Systems](#core-systems)
3. [Player Setup](#player-setup)
4. [Enemy Setup](#enemy-setup)
5. [Level Generation](#level-generation)
6. [UI System](#ui-system)
7. [Item System](#item-system)
8. [Testing & Debugging](#testing--debugging)

## Project Setup

### Requirements
- Unity 2022.3 LTS or newer
- Universal Render Pipeline (URP)
- 2D Sprite package
- Input System package
- TextMeshPro package

### Initial Setup
1. Create a new 2D URP project
2. Set up the following folder structure:
   ```
   Assets/
   ├── Animations/
   ├── Audio/
   ├── Materials/
   ├── Prefabs/
   ├── Resources/
   ├── Scenes/
   ├── Scripts/
   ├── Settings/
   └── Sprites/
   ```
3. Configure URP settings for 2D rendering
4. Set up layers:
   - Player
   - Enemy
   - Projectile
   - Item
   - Wall
   - Room

### Required Materials
1. Create `WhiteFlash.mat` in Materials folder for damage feedback
2. Set up sprite materials for different effects

## Core Systems

### Game Manager
1. Create empty GameObject named "GameManager"
2. Attach `GameManager.cs`
3. Configure:
   - Levels per run
   - Difficulty settings
   - Player prefab reference

### Scene Setup
1. Create main scene
2. Add essential managers:
   - GameManager
   - UIManager
   - DungeonGenerator
   - AudioManager (if using)

## Player Setup

### Player Prefab
1. Create Player GameObject
2. Add components:
   - Sprite Renderer
   - Rigidbody2D (Kinematic)
   - Circle Collider 2D
   - PlayerController
   - PlayerHealth
   - PlayerCombat
3. Configure player settings:
   - Movement speed
   - Health values
   - Attack settings
   - Collision layers

### Player Animations
1. Create animation controller
2. Add required animations:
   - Idle
   - Walk
   - Attack
   - Hurt
   - Die
3. Set up animation parameters:
   - MoveX
   - MoveY
   - IsMoving
   - Attack
   - IsDamaged
   - Die

## Enemy Setup

### Basic Enemy Prefab
1. Create Enemy GameObject
2. Add components:
   - Sprite Renderer
   - Rigidbody2D
   - Collider 2D
   - EnemyAI
   - EnemyHealth
3. Configure settings:
   - Detection range
   - Movement speed
   - Attack damage
   - Health values

### Boss Enemy Setup
1. Create BossEnemy prefab
2. Add components:
   - BossEnemy script
   - Enhanced health/damage values
3. Configure:
   - Phase transitions
   - Special attacks
   - Attack patterns
   - Warning effects

### Enemy Animations
1. Create animation controllers for each enemy type
2. Add required states:
   - Idle
   - Move
   - Attack
   - Special Attack (Boss)
   - Hurt
   - Die

## Level Generation

### Room Setup
1. Create room prefabs:
   - Start room
   - Normal room
   - Boss room
   - Treasure room
   - Shop room
2. Add to each room:
   - Room script
   - Spawn points
   - Door positions
   - Colliders

### Dungeon Generator
1. Configure DungeonGenerator:
   - Room prefabs
   - Generation rules
   - Room connections
   - Minimum/maximum rooms

### Room Contents
1. Create spawn point markers
2. Set up enemy spawn rules
3. Configure item spawn locations
4. Add environmental hazards

## UI System

### Required Canvases
1. Main Menu
2. HUD
   - Health bar
   - Coin counter
   - Minimap
3. Pause Menu
4. Game Over Screen
5. Victory Screen

### Minimap System
1. Set up minimap camera
2. Configure MinimapSystem:
   - Room icons
   - Player marker
   - Visibility rules

## Item System

### Item Base Setup
1. Create base item prefab
2. Configure item types:
   - Health pickup
   - Coin pickup
   - Weapon pickup

### Weapon System
1. Create weapon prefabs
2. Set up weapon stats:
   - Damage
   - Attack speed
   - Range
3. Configure pickup behavior

## Testing & Debugging

### Test Scenes
1. Create test room for player mechanics
2. Create enemy test scene
3. Set up generation test scene

### Debug Features
1. Implement debug commands:
   - Room generation visualization
   - Enemy path display
   - Hitbox visualization

### Common Issues
- Check collision layers
- Verify event subscriptions
- Test room connections
- Validate spawn points

## Performance Optimization
1. Object pooling for:
   - Projectiles
   - Enemies
   - Items
2. Efficient room loading/unloading
3. Sprite batching

## Next Steps
1. Add more enemy types
2. Create additional room layouts
3. Implement progression system
4. Add sound effects and music
5. Polish visual effects

## Script References

### Core Scripts
- GameManager.cs: Central game state and progression
- UIManager.cs: UI handling and display
- MinimapSystem.cs: Minimap functionality

### Player Scripts
- PlayerController.cs: Movement and input
- PlayerHealth.cs: Health and damage
- PlayerCombat.cs: Attack handling

### Enemy Scripts
- EnemyAI.cs: Basic enemy behavior
- EnemyHealth.cs: Enemy health system
- BossEnemy.cs: Boss mechanics

### Level Scripts
- Room.cs: Individual room management
- DungeonGenerator.cs: Level generation
- Door.cs: Door behavior

### Item Scripts
- ItemBase.cs: Base item functionality
- HealthPickup.cs: Health restoration
- CoinPickup.cs: Currency collection
- WeaponPickup.cs: Weapon switching

## Required Resources

### Minimum Sprite Requirements
1. Player character (idle, walk, attack animations)
2. Basic enemies (2-3 types)
3. Boss enemy
4. Room tiles (floor, walls, doors)
5. Items and pickups
6. UI elements

### Audio Requirements
1. Background music
2. Player effects
3. Enemy sounds
4. Environmental audio
5. UI sounds

## Testing Checklist
- [ ] Player movement and collision
- [ ] Enemy AI and pathfinding
- [ ] Room generation and connections
- [ ] Item collection and effects
- [ ] UI functionality
- [ ] Boss battle mechanics
- [ ] Save/load system (if implemented)
- [ ] Performance testing

## Troubleshooting
1. Player Issues
   - Check input configuration
   - Verify collision layers
   - Test animation transitions

2. Enemy Issues
   - Validate pathfinding
   - Check attack ranges
   - Test AI state transitions

3. Level Generation
   - Verify room connections
   - Test door functionality
   - Check spawn points

4. Performance
   - Monitor frame rate
   - Check object pooling
   - Optimize room loading 