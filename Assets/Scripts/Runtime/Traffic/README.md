# Traffic Module

## Module Purpose

The **Traffic** module is responsible for orchestrating the overall flow, spawning patterns, and configuration of vehicles on the roads based on external rules or API states. While the `Vehicle` module governs a *single* car, the `Traffic` module governs the *entire road*.

As same as Player, it deals with the feature as more like a MVP or MVVM.

## Architecture & Folder Structure

### 1. Domain Layer (`/Domain/`)
Pure logic layer handling the mathematical models and behavior parameters.
- **`TrafficModel`**: Represents the current snapshot of the traffic logic internally.
- **`ITrafficLevelSettings`**: An interface contract defining how specific levels dictate traffic constraints.
- **`TrafficSettings` / `TrafficSettingsPredicted`**: Structures aggregating stats like speed multipliers and lane densities, heavily mapping to concepts retrieved by `TrafficData`.

### 2. Presentation Layer (`/Presentation/`)
Handles instantiation and active rendering within Unity.
- **`TrafficController`**: The Unity-aware manager that orchestrates updates across lanes. It connects to the Domain data to understand *when* to spawn.
- **`TrafficSpawner`**: A factory/instantiator element (often paired with an Object Pool) that creates `Vehicle` views at specific lane points based on frequency metrics.
- **`TrafficView`**: Contains the physical representations of roads or lanes.

## Script Communication Flow

1. **Preparation:** `GameplayPresenter` retrieves traffic analytics via the `TrafficData` module and pushes these stats into `TrafficModel`.
2. **Spawning Logic:** `TrafficController` observes `TrafficModel`. Using the `vehicleDensity` and `averageSpeed`, it calculates wait timers for its lanes (e.g., heavily dense lanes get a shorter timer).
3. **Execution:** Once the mathematical timer triggers, `TrafficController` signals the `TrafficSpawner`.
4. **Binding:** `TrafficSpawner` fetches a `VehicleView` from the pool, instantiates a `VehicleModel` injecting the `TrafficSettings` modifiers (like speed), and sets the car rolling.

## Scaling/Extension Guide

- **Dynamic Weather Integration:** When changing the system to react dynamically to weather changes (e.g., slowing cars when it rains), the `TrafficModel` should listen to the condition updates, calculate the speed offset, and ripple those values down to spawned `VehicleModel`s automatically.
- **New Vehicle Types (Trucks, Bikes):** Create sub-spawners or randomized factory methods in `TrafficSpawner`. Feed the probability ratios into the `TrafficModel` so spawning logic remains pure.
- **Testing:** The spawning interval math tied to density variables can be fully isolated in Unit Tests across `TrafficModel` checking proper millisecond return values without invoking Unity's `Time.deltaTime`.
