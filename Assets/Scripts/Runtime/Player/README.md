# Player Module

## Module Purpose

The **Player** module encompasses the core logic, state management, and physical representation of the user-controlled character. Similar to the Vehicle module, it isolates the business rules defining player constraints (movement scope, health, score) from the technical rendering and input polling provided by the Unity Engine.

The module is not extremely rigorous with Clean Architecture principles if the presentation layer knows about the domain models for simplification, but it correctly enforces unidirectional logic execution, however it deals with the feature as a MVP or MVVM.

## Architecture & Folder Structure

The module is broken down primarily into Domain and Presentation components:

### 1. Domain Layer (`/Domain/`)
Stores the absolute truths regarding the player state.
- **`PlayerModel`**: Holds internal states like alive status, coordinates (if logically tracked), or score.
- **`IPlayerSettings`**: Interface denoting constraints (speed limits, movement bounds) ensuring the domain logic doesn't depend on ScriptableObjects.

### 2. Presentation Layer (`/Presentation/`)
Binds the player to the Unity scene context.
- **`PlayerController`**: Acts as the brain/mediator. It receives raw commands (e.g., "Move Up") and verifies against business rules (e.g., "Is movement blocked by borders?").
- **`PlayerView`**: The `MonoBehaviour` script attached to the character GameObject. It deals with `Transform` repositioning, animations, collision events (like Trigger Enters with Vehicles), and visual updates.

## Script Communication Flow

1. **Initialization:** The game composition root creates a `PlayerModel` and passes it alongside `IPlayerSettings` into the `PlayerController`. The `PlayerView` is then linked to monitor these rules.
2. **Input Processing:** Raw input (e.g., from `GameplayInputHandler`) routes a directional command into the `PlayerController`.
3. **Logic Validation:** `PlayerController` validates the move. If valid, it updates the `PlayerModel` state or directly processes the coordinate calculation.
4. **Visual Execution:** `PlayerView` senses the model update (via Observer Pattern/Events, or through direct exposure/polling) and physically uses Unity's `Leantween` or `Transform` components to slide the GameObject to the new exact location.


## Scaling/Extension Guide

- **Adding Mechanics:** To add abilities (like a "jump over" mechanic), apply the logic check in `PlayerController`, toggle a state in `PlayerModel`, and have `PlayerView` trigger the respective animation or Y-axis displacement.
- **Physics Abstraction:** Keep Unity's Physics engine (Colliders, Rigidbody) completely inside `PlayerView`. If a collision with an enemy triggers death, `PlayerView.OnTriggerEnter` should call `controller.HandleCollision()`, not apply logic itself.
- **Testing:** `PlayerController` can be exhaustively unit-tested against varying map boundaries simulated by `IPlayerSettings` stubs to ensure the player cannot walk out of bounds, without running Unity play-mode tests.
