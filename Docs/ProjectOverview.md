# Archero Demo Project Overview

## Project Goal

This project is a small Archero-like 3D demo. The current focus is not to fully reproduce the original game, but to build a playable prototype around the core loop:

1. Use a virtual joystick to control movement speed and direction.
2. Stop moving to auto aim and shoot enemies.
3. Clear enemies in a room, collect rewards, and enter the next level.
4. Gain experience, level up, and choose one of several upgrades.

## Current Features

### Virtual Joystick Movement

The player uses `VirtualJoystick` for mobile-style movement input.

Main idea:

- `Intensity` means how far the joystick is pushed.
- `SpeedFactor` is the movement intent after applying an `AnimationCurve`.
- `PlayerController` reads joystick input and writes `MoveSpeed` / `HasInput` into the Animator.
- The Blend Tree maps `MoveSpeed` to idle, slow walk, fast walk, and run.

Related scripts:

- `Assets/Scripts/UI/VirtualJoystick.cs`
- `Assets/Scripts/Characters/Controller/Player/PlayerController.cs`
- `Assets/Scripts/Debug/JoystickDebugOverlay.cs`

### Player Rotation And Auto Aim

When the player is moving, the character faces the joystick direction.

When the player stops moving, the character aims at the currently selected enemy.

The target selection logic follows the current Archero-like rule used in the demo:

- While moving, the player can switch to a nearer enemy.
- After stopping, the selected enemy is locked until it dies or becomes invalid.

Related scripts:

- `Assets/Scripts/Characters/Combat/Player/PlayerDetectionController.cs`
- `Assets/Scripts/Data/PlayerRuntimeContext.cs`
- `Assets/Scripts/Characters/Controller/Player/PlayerController.cs`

### Player Combat

The player shoots automatically while standing still and having a valid target.

The combat controller reads weapon configuration from `PlayerWeaponConfig`.

Implemented projectile features:

- Arrow prefab spawning through `PoolManager`.
- Projectile speed and max range.
- Damage.
- Environment collision stop/despawn behavior.
- Multiple arrows from upgrades, including front arrows and diagonal arrows.

Related scripts:

- `Assets/Scripts/Characters/Combat/Player/PlayerCombatController.cs`
- `Assets/Scripts/Weapon/ProjectileArrow.cs`
- `Assets/Scripts/ScriptableObjects/PlayerWeaponConfig.cs`

### Enemy System

Enemies use `NavMeshAgent` to chase the player.

Enemy data is split into configuration assets:

- `EnemyHealthBase`: max HP and kill experience.
- `EnemyChaseConfig`: move speed, stop distance, touch damage, and damage interval.

Enemy prefab requirements:

- `EnemyController`
- `EnemyHealthController`
- `EnemyChaseController`
- `EnemyCombatController`
- `NavMeshAgent`
- `Rigidbody`
- `Collider`
- hit feedback components

There is also an editor validation tool for enemy prefabs.

Related scripts:

- `Assets/Scripts/Characters/Combat/Enemy/EnemyChaseController.cs`
- `Assets/Scripts/Characters/Combat/Enemy/EnemyCombatController.cs`
- `Assets/Scripts/Characters/Health/Enemy/EnemyHealthController.cs`
- `Assets/Scripts/Editor/EnemyPrefabValidatorEditorTools.cs`

### Health And Damage Feedback

Player health is controlled by `PlayerHealthController`.

Current feedback:

- Player hit flash.
- Screen red flash.
- Player hit sound.
- Enemy hit flash.
- Enemy floating damage numbers.

Damage is routed through `GameEventManager` using `EventName.PlayerHit`, so enemies and traps can damage the player without directly depending on the health controller.

Related scripts:

- `Assets/Scripts/Characters/Health/Player/PlayerHealthController.cs`
- `Assets/Scripts/Characters/Feedback/HitFlashFeedback.cs`
- `Assets/Scripts/Characters/Feedback/ScreenHitFlash.cs`
- `Assets/Scripts/Characters/Feedback/DamageNumberFeedback.cs`
- `Assets/Scripts/Characters/Feedback/DamageNumberItem.cs`

### Trap System

`PF_Trap_Spikes` is treated as a damage trigger, not a path-blocking obstacle.

Trap behavior:

- The trap collider is set as Trigger.
- `TrapDamageController` applies 10 damage every 0.5 seconds while the player stays inside.
- Trap damage uses the same player hit event, so existing hit feedback is reused.

Related script:

- `Assets/Scripts/Traps/TrapDamageController.cs`

### Room And Level Flow

The project has a room-based level flow.

Current design:

- `RoomLevelCatalog` stores the list of room level configs.
- Each `RoomLevelConfig` defines which obstacles and enemies should appear.
- `RoomLevelSpawner` spawns configured obstacles and enemies.
- `RoomController` tracks room state and reacts when all enemies are dead.
- `DoorExitTrigger` enters the next level.
- If the player dies, the game returns to the game-over/menu flow.

Obstacle prefabs are expected to own their own Collider and NavMeshObstacle setup. The spawner only validates them and does not dynamically install obstacle components.

Related scripts:

- `Assets/Scripts/Rooms/RoomLevelSpawner.cs`
- `Assets/Scripts/Rooms/RoomController.cs`
- `Assets/Scripts/Rooms/DoorController.cs`
- `Assets/Scripts/Rooms/DoorExitTrigger.cs`
- `Assets/Scripts/ScriptableObjects/RoomLevelConfig.cs`
- `Assets/Scripts/ScriptableObjects/RoomLevelCatalog.cs`

### Rewards

Enemies can drop rewards such as coins.

Current reward behavior:

- Rewards are spawned after enemy death.
- Rewards stay in the room during combat.
- After the room is cleared, rewards fly toward the player.
- Reward objects use the object pool.

Related script:

- `Assets/Scripts/Rewards/RewardController.cs`

### Experience And Upgrade Selection

The player gains experience from enemy kills.

When leveling up:

1. Player first heals for a percentage of max health.
2. The game opens a 3-choice upgrade panel.
3. After choosing an upgrade, the game continues.

Current upgrade types:

- Attack speed.
- Damage.
- Front arrow.
- Diagonal arrows.
- Max health and current health increase.

Related scripts:

- `Assets/Scripts/Characters/Experience/PlayerExperienceController.cs`
- `Assets/Scripts/Characters/Upgrade/PlayerUpgradeController.cs`
- `Assets/Scripts/ScriptableObjects/PlayerExperienceConfig.cs`
- `Assets/Scripts/ScriptableObjects/PlayerUpgradeConfig.cs`
- `Assets/Scripts/UI/UIManager.cs`

### UI

Current UI includes:

- Main menu.
- In-game HUD.
- Health slider and health text.
- Experience bar and level text.
- Upgrade choice panel.
- Pause panel.
- Game-over panel.
- Joystick debug overlay for recording/demo explanation.

Related scripts:

- `Assets/Scripts/UI/MainMenuUI.cs`
- `Assets/Scripts/UI/UIManager.cs`
- `Assets/Scripts/Debug/JoystickDebugOverlay.cs`

### Audio

Audio is managed by `AudioManager`.

Current supported audio:

- Background music.
- UI click.
- Shoot.
- Enemy hit.
- Player hit.

Volume sliders are connected to BGM and SFX volume controls.

Related script:

- `Assets/Scripts/Manager/AudioManager.cs`

### Object Pooling

`PoolManager` provides a general object pool for reusable prefabs.

Currently used for:

- Projectiles.
- Rewards.
- Floating damage numbers.

Related scripts:

- `Assets/Scripts/Manager/PoolManager.cs`
- `Assets/Scripts/Manager/PoolItem.cs`

### Camera

The camera follows the player using a config asset.

The camera has a configured standard Z offset instead of calculating it only from the initial scene position. This avoids the camera breaking when the player start position changes.

Related scripts:

- `Assets/Scripts/Camera/CameraController.cs`
- `Assets/Scripts/ScriptableObjects/CameraFollowConfig.cs`

## Main Architecture Notes

### Configuration By ScriptableObject

Most gameplay numbers are stored in ScriptableObject assets instead of being hardcoded in scripts.

Examples:

- Player movement and detection: `PlayerSO`
- Weapon stats: `PlayerWeaponConfig`
- Player health: `PlayerHealthConfig`
- Player experience: `PlayerExperienceConfig`
- Player upgrades: `PlayerUpgradeConfig`
- Enemy health: `EnemyHealthBase`
- Enemy chase/combat: `EnemyChaseConfig`
- Camera: `CameraFollowConfig`
- Room levels: `RoomLevelConfig`, `RoomLevelCatalog`

This makes balancing easier and keeps scripts focused on behavior.

### Event-Based Decoupling

Some cross-system interactions use `GameEventManager`.

Examples:

- Enemy or trap damages player through `PlayerHit`.
- Enemy death broadcasts room/experience related events.
- Room clear can trigger door/reward behavior.

This avoids hard references between unrelated systems.

### Prefab-Owned Components

Runtime code should not silently install important scene/prefab components for normal gameplay objects.

Current intended rule:

- Enemy prefabs own enemy components.
- Obstacle prefabs own Collider/NavMeshObstacle.
- Trap prefabs own Trigger Collider and `TrapDamageController`.
- Runtime spawners instantiate and validate.

This keeps prefab behavior visible in the editor and easier to debug.

## Suggested Next Improvements

Recommended next steps:

1. Improve level variety with more room layouts and enemy combinations.
2. Add enemy attack animations and stronger enemy type differences.
3. Polish upgrade UI and add more upgrade types.
4. Add more sound effects and better hit/kill feedback.
5. Add simple save/settings persistence for audio and options.
6. Add editor validation tools for room configs, obstacle prefabs, and trap prefabs.
7. Review all generated levels and tune difficulty progression.

