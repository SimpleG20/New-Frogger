# New Frogger

## Overview
This project is a new version of the classic Frogger game, built with Unity and following the Clean Architecture pattern.

The game is divided into modules, each module is a feature of the game.
Each feature is a module mostly divided into 3 layers:
 - Domain: layer that contains the business logic and data structures
 - Data: layer that contains the rawn data adjusted to be used by Presentation;
 - Presentation: layer that contains the UnityEngine components and communicate with the Unity World and commands

## Dependencies
- Unity version: **6000.0.62f1**
- Leantween (*most recent, click [here](https://assetstore.unity.com/packages/tools/animation/leantween-3595)*)
- Cysharp.UniTask (*most recent, click [here](https://github.com/Cysharp/UniTask)*)
- Internet Connection (for the Remote API) or Local API for offline play (*instructions below*)

## Files Structure

    Assets/
    ├── Art/
    ├── Data/
    │   ├── ScriptableObjects/
    │   └── Prefab/
    ├── Scenes/
    └── Scripts/
        └── Runtime/
            ├── Core/
            ├── Gameplay/
            ├── Player/
            ├── Traffic/
            ├── TrafficData/
            └── Vehicle/


                    

## Getting Started

To start this project you need to install the version passed in the Unity Hub. After that, you need to open the project in unity Hub by clicking `Add > Add project from disk`..

After opening the project you must install the packages needed. You can do this by going to `Window > Package Manager` and clicking `Install` for each package.

Follow the instructions from the other packages to install them.

**After that, you may need to go to the `Scenes/` folder and open the `GameScene`. If you have an active internet connection, you can start the gameplay immediately using the default Remote API.**

- ### API Setup

The game communicates with a backend API. By default, it connects to a **Remote Mock API** hosted on Postman (`https://f98dcad7-664e-44fb-8c6f-79c727a98caf.mock.pstmn.io`), which requires an active internet connection.

#### Offline Play (Local API)
If you do not have internet access or wish to run the API locally, you must set up the Local API:
1. Run your local API server environment.
2. In Unity, find the object that holds the script `Assets/Scripts/Runtime/Gameplay/Presentation/GameplayPresenter.cs` and change the target URL from the Postman string to your `localhost` address. **DO NOT need to put the endpoints, just the localhost with port address**

To run the local API in Postman, follow the instructions below:

1. Open Postman
2. Click on `+` and select  `Mock server`
3. Choose on `Create a new collection` and give it a name.
4. In `request URL` put `v1/traffic/status` and in `Response Body` put `{}`.
5. Choose `Save the mock server URL as new environment variables`.
6. Click on `Create Mock Server` button.
7. Choose `Collection Window` and select the collection with the name given to the Mock Server.
8. There must be a endpoint `v1/traffic/status` in the collection with a Default response, modify it to match the Remote API response.

``` json
// Here is an example
{
  "current_status": {
    "vehicleDensity": 0.3,
    "averageSpeed": 30.0,
    "weather": "sunny"
  },
  "predicted_status": [
    {
      "estimated_time": 15000,
      "status": {
        "vehicleDensity": 0.3,
        "averageSpeed": 40.0,
        "weather": "clouded"
      }
    },
    {
      "estimated_time": 20000,
      "status": {
        "vehicleDensity": 0.2,
        "averageSpeed": 35.0,
        "weather": "sunny"
      }
    }
  ]
}
```

9. Create two more default by clicking on `...` next to `GET` and select `Add example`. Change the response.
10. Do not forget to add the key `level` to each default response with the respective value (1, 2, 3).

## Notes

AI was used exclusively for commits and documentation, with the exception of this one. This project was not created through intuitive programming, but rather based on my programming skills, with the aid of AI to enhance some code.


## Author

Arthur Goncalves (SimpleG20)