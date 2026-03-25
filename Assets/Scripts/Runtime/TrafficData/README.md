# TrafficData Module

## Module Purpose

The **TrafficData** module handles fetching, deserializing, and structuring external traffic data coming from the remote application interface (API). It strictly applies Clean Architecture by hiding data fetching intricacies (like JSON parsing or HTTP clients) from the core game rules, ensuring the rest of the application remains ignorant of where the data comes from.

## Architecture & Folder Structure

The module follows the Domain-Driven Design and Clean Architecture principles across two layers:

### 1. Domain Layer (`/Domain/`)
The absolute core of the module, containing no dependencies on external frameworks.
- **Entities (`TrafficStatusModel`, `TrafficPredictModel`)**: Pure C# representations of the traffic data.
- **Enums (`ETrafficWeather`)**: Business-level categorizations of the weather.
- **Repositories (`ITrafficRepository`)**: Abstract contract defining how traffic data should be retrieved.
- **Services/Use Cases (`GetTrafficStats.cs`)**: Contains the business logic orchestrator that commands the repository to fetch the data.

### 2. Data Layer (`/Data/`)
The concrete implementation layer responsible for interacting with the external world (the Postman Mock API or Local API).
- **DTOs (Data Transfer Objects)**: Raw representations of the JSON response payload.
- **Datasources (`ApiTrafficDataSource`)**: Logic for making actual HTTP requests (e.g., using `UniTask` and UnityWebRequest).
- **Repositories (`TrafficRepositoryImpl`)**: The concrete implementation of `ITrafficRepository`. It aggregates data from the datasource and maps it.
- **Mappers (`TrafficMappers`)**: Static or injected utilities that convert external DTOs into Domain Entities.

## Script Communication Flow

1. **Request Initiation:** A game controller calls the Domain Service `GetTrafficStats.Execute()`.
2. **Repository Delegation:** The Service calls the interface `ITrafficRepository.GetTrafficData()`.
3. **Data Fetching:** The Application resolves `ITrafficRepository` to `TrafficRepositoryImpl`, which asks `ITrafficDataSource` to fetch data from the API endpoint.
4. **Mapping:** The Payload is received as a `TrafficStatusDTO`, which is then passed to `TrafficMappers` to be converted into a `TrafficStatusModel`.
5. **Return:** The Domain layer receives the clean `TrafficStatusModel`, ready to be used by the Spawner system.

## Usage Examples

**Fetching Traffic Data:**

```csharp
// 1. Dependency Injection setup (usually at Composition Root)
ITrafficDataSource dataSource = new ApiTrafficDataSource("http://localhost:3000/v1/");
ITrafficRepository repository = new TrafficRepositoryImpl(dataSource);
GetTrafficStats getStatsUseCase = new GetTrafficStats(repository);

// 2. Executing the fetch logic inside a clean architecture boundary
TrafficStatsModel currentStats = await getStatsUseCase.ExecuteAsync(level: 1);

// 3. Using the clean model
Log.log($"Current Weather: {currentStats.Weather}");
Log.log($"Vehicle Density: {currentStats.VehicleDensity}");
```

## Scaling/Extension Guide

- **Adding New Endpoints:** To consume a new endpoint (like `/v1/traffic/events`), create the respective DTOs, define a new method mapping in `ITrafficRepository`, implement it in `TrafficRepositoryImpl`, and create a new Use Case in the Domain Layer.
- **Changing Data Providers:** If you switch from an HTTP API to a local JSON file or a Firebase database, you only need to create a new `ITrafficDataSource` (e.g., `LocalFileTrafficDataSource`). The Domain and logic won't change at all.
- **Testing:** Since the `GetTrafficStats` Service relies on interfaces, you can easily mock `ITrafficRepository` passing fake data strictly for Unit Testing, without making actual network calls.
