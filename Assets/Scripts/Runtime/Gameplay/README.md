# Gameplay Module

## Module Purpose

The **Gameplay** module functions as the global orchestrator of a running level. It binds all parallel systems together—such as Player, Traffic, and TrafficData—managing the holistic game state (Init, Playing, Game Over, Win) and ensuring clean dependency injection.

## Architecture & Folder Structure

This module interacts heavily with all other domains and serves as the highest-level architectural boundary for a Scene.

### 1. Composition Layer (`/Composition/`)
- **`GameplayCompositionRoot`**: The Dependency Injection entry point. It creates all Domain Models, Use Cases, and Controllers, connecting them together upon Scene Load.

### 2. Data Layer (`/Data/`)
- **`GameplaySettingsSO.cs`**: A structural `ScriptableObject` containing designer-tweakable numbers that bind to Domain interfaces (like `IPlayerSettings`). 

### 3. Domain Layer (`/Domain/`)
- **`LevelData`**: Holds universal states pertaining strictly to the level's business rules, such as max score, time bounds, or specific layout IDs.

### 4. Presentation Layer (`/Presentation/`)
- **`GameplayInputHandler`**: Translates Unity's specific cross-platform UI/Keyboard Input System into pure Domain commands.
- **`GameplayPresenter`**: Evaluates level states alongside input instructions, funneling data between different subsystems.
- **`GameplayLifecycle`**: Maintains the Unity MonoBehaviour execution tracking loops.
- **`GameplayView`**: Manages the main scene visual representations.

## Script Communication Flow

1. **Bootstrap:** When Unity opens the Scene, `GameplayPresenter.Awake()` runs. It reads `GameplaySettingsSO`, crafts raw Domain Interfaces out of it, and injects them into the `Traffic`, `Player`, and `Data` modules.
2. **API Connection:** It uses the `TrafficData` module to pull external mocked statuses over the network via `GetTrafficStats`.
3. **Execution Loop:** `GameplayLifecycle` coordinates Unity's `Update()`. It triggers `GameplayInputHandler` to read inputs.
4. **Command Routing:** Input commands route to `GameplayPresenter`, which distributes them (e.g., passing directional actions to the `PlayerController`).
5. **View:** `GameplayView` is the script that handles the visualization of events related to the game in general, such as the game starting, the game ending, the player dying, the player reaching the goal, etc.
6. **Game Over Check:** `GameplayPresenter` watches conditions across multiple objects. If `Player` signals death or reaching the goal, the Presenter shifts the state in the Composition layer and commands the `GameplayView` to display UI changes.


## Scaling/Extension Guide

- **Adding a New System Elements (Audio, Analytics):** Add the instantiation in `GameplayCompositionRoot`. Wire it cleanly via interfaces to the `GameplayPresenter` without tightly coupling components via `FindObjectOfType` inside Unity.
- **Scene State Handlers:** To extend states (like implementing a "Pause"), manage the core enum flag inside `GameplayPresenter` or a dedicated `GameStateMachine` Domain class, locking input handlers recursively.
- **Testing:** Because everything hinges heavily on interfaces generated at the Composition Root, you can perform integration-level tests completely isolated from Unity Editor Scene configurations manually by stitching Mock interfaces together.
