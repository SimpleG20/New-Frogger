# Vehicle Module

## Module Purpose

The **Vehicle** module manages the core data, logic, and visual representation of vehicles within the game (e.g., obstacles that move across the screen). It adheres to Clean Architecture and SOLID principles by strictly separating the business logic (Domain) from the Unity Engine rendering and behaviors (Presentation).

The module is not that rigorous with the Clean Architecture, once the domain layer is known by the presentation layer, but for the instance of this project it is fine.

## Architecture & Folder Structure

The module is divided into two primary layers:

### 1. Domain Layer (`/Domain/VehicleModel.cs`)
The **Domain** layer handles the pure business rules and state of the vehicle. 
- Designed as a plain C# class without any `UnityEngine` dependencies.
- Maintains attributes like `Speed`, `Active` state, and `CanMove` permissions.
- Emits events, such as `OnActiveChanged`, whenever important state transitions occur.

### 2. Presentation Layer (`/Presentation/VehicleView.cs`)
The **Presentation** layer acts as the bridge to the Unity Engine.
- Contains the `VehicleView` class, inheriting from `MonoBehaviour`.
- Focuses entirely on rendering the object (`gameObject.SetActive`), handling transformations (`transform.Translate`), and monitoring physical bounds (checking `transform.position.z` against `_zLimit`).

## Script Communication Flow

Communication between the scripts strictly follows a clean unidirectional data flow:

1. **Initialization:** An external orchestrator (e.g., a Traffic Spawner or Object Pool) creates a `VehicleModel` instance and injects it into the presentation layer via `VehicleView.Initialize(model, zLimit)`.

2. **Event Subscription (Observer Pattern):** The `VehicleView` subscribes to the domain's `OnActiveChanged` event. This allows the model to remain unaware of the view, while ensuring the view automatically activates or deactivates its GameObject when the logical state changes.

3. **Continuous Polling:** During `Update()`, the `VehicleView` continuously polls the `VehicleModel` for state markers (`_model.Active`, `_model.CanMove`) and property values (`_model.Speed`) to govern its exact translation across the game world.

4. **Outward Messaging:** When physical boundaries are breached (`transform.position.z > _zLimit`), the `VehicleView` triggers an `OnLimitReached` event. This passes responsibility back to external orchestrators to recycle or destroy the object.

## Usage Examples

**Creating and Setting up a Vehicle:**

```csharp
// 1. Create the Domain logic independent of Unity
float referenceSpeed = 10f;
VehicleModel model = new VehicleModel(referenceSpeed);

// 2. Obtain the Presentation view (e.g., from an Object Pool or instantiated Prefab)
VehicleView view = vehicleGameObject.GetComponent<VehicleView>();

// 3. Bind the View to the injected Model and set a boundary limit
float despawnZLimit = 50f;
view.Initialize(model, despawnZLimit);

// 4. Activate and enable movement through the Domain
model.SetActive(true);
model.SetCanMove(true);

// 5. Change speed dynamically through business rules
model.ChangeSpeed(120f);
```

## Scaling/Extension Guide

- **Adding Concept Data (e.g., Vehicle Type, Mass):** Concept data and logic must reside entirely in `VehicleModel`.
- **Feature Expansions (Visuals, Audio, VFX):** If new data points (e.g. Engine RPM) dictate an audio pitch change, expose an event in `VehicleModel` and subscribe to it inside `VehicleView` (or a dedicated `VehicleAudioView`), preventing engine concepts from bleeding into the domain.
- **Testing:** Because all core data and rules reside in `VehicleModel`, you can write fast, pure C# Unit Tests for the domain. Assert that negative speeds are bypassed or that state flags toggle as intended, circumventing the need for slow Unity Play Mode tests.
