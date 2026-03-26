# Core Module

## Module Purpose

The **Core** module functions as the global infrastructure backbone of the project. It provides fundamental shared utilities, foundational services, and abstract providers that are universally accessible across all other specific feature modules (like Player, Traffic, or Gameplay). 

By extracting these utilities to `Core`, we prevent dependency duplication and enforce standardized mechanics—such as time tracking or logging—without tying them directly to Unity APIs everywhere in the code.

## Architecture & Folder Structure

### 1. Root / Infrastructure (`/`)
- **`Log.cs`**: A centralized, safe envelope wrapper for logging. Replaces direct `Debug.Log` calls, allowing developers to switch logging frameworks or strip logs entirely in release builds.

### 2. Services (`/Services/`)
- **`BaseService`**: A foundational class that enforces structural patterns (such as initialization states or disposable cleanup) for any complex business service created in the system.

### 3. Providers (`/Providers/`)
Providers abstract Unity's natively static variables into mockable interfaces.
- **`ITimeProvider`** and **`UnityTimeProvider`**: Replaces `Time.deltaTime` and `Time.time`. By injecting `ITimeProvider`, we decouple systems from Unity's physical time, making pause logic or deterministic unit testing seamless.
- **`TimerInSecs`**: A mathematical structure capable of measuring expiration times independent of MonoBehaviour coroutines.

## Script Communication Flow

Unlike feature modules (which feature strict unidirectional logic execution), `Core` provides horizontal utilities. Components across the architecture will depend directly inward towards `Core`.

1. **Instantiation:** At the application startup, `UnityTimeProvider` is booted. 
2. **Injection:** The `ITimeProvider` interface is distributed to needing dependencies (for example, the `TrafficSpawner` needs to count down seconds).
3. **Execution:** Feature controllers use `TimerInSecs` objects, polling them via `.Tick(_timeProvider.DeltaTime)` without needing custom float logic in every single class.

## Usage Examples

**Decoupled Time Management & Logging:**

```csharp
// 1. Logging cleanly without bringing Unity APIs into pure C# modules
Log.lessage("Application Started Successfully");

// 2. Fetching the global injected Time Provider
ITimeProvider timeProvider = new UnityTimeProvider();

// 3. Establishing a safe, Unity-agnostic Timer mechanism
TimerInSecs routineTimer = new TimerInSecs(duration: 5.0f);

public void UpdateTick() 
{
    // Feeding the abstract delta time
    routineTimer.Tick(timeProvider.DeltaTime);

    if (routineTimer.IsFinished) 
    {
        Log.Warning("Timer expired!");
        routineTimer.Reset();
    }
}
```

## Scaling/Extension Guide

- **Cross-Platform Needs:** If implementing specific system interactions (like File System writes, or Analytics tracking), place a cross-platform interface wrapper in `Core/Providers` to ensure Domain logic remains unpolluted.
- **Networking Adapters:** Basic networking constants, global configuration endpoints, or generic REST clients fit naturally into `Core/Services`.
- **Testing:** The primary feature of `Core` is enhancing testability. By utilizing `MockTimeProvider` in NUnit tests, you can fast-forward "5 seconds" in an instant mathematically, validating game reactions accurately without running Coroutines.
