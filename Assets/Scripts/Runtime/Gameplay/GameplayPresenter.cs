using CustomLogger;
using NewFrogger.Gameplay.Data;
using NewFrogger.Traffic.Data.Datasources;
using NewFrogger.Traffic.Data.Repositories;
using NewFrogger.Traffic.Domain.Services;
using NewFrogger.Vehicle.Domain;
using NewFrogger.Vehicle.Presentation;
using UnityEngine;

public class GameplayPresenter : MonoBehaviour
{
    [SerializeField] private GameplaySettingsSO _gameplaySettingsSO;
    [SerializeField] private TrafficSpawner _trafficSpawner;

    private GameplayModel _model;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        Log.SetLogger(new EditorLogger());

        var datasource = new ApiTrafficDataSource("https://f98dcad7-664e-44fb-8c6f-79c727a98caf.mock.pstmn.io");
        var repo = new TrafficRepositoryImpl(datasource);
        var useCase = new GetTrafficStatsService(repo);

        _model = new GameplayModel(_gameplaySettingsSO, useCase);
        _model.OnTrafficSettingsChanged += HandleOnTrafficSettingsChanged;
        _model.OnStartNewLevel += HandleOnStartNewLevel;
        _model.OnStopLevel += HandleOnStopLevel;

        Log.log("Gameplay Presenter Initialized");
    }

    private void HandleOnStopLevel()
    {
        _trafficSpawner.StopSpawning();
    }

    private void HandleOnStartNewLevel()
    {
        _trafficSpawner.HandleStart(_model.CurrentTrafficSettings, 10);
    }

    private void HandleOnTrafficSettingsChanged(TrafficSettings newSettings)
    {
        Log.log("New traffic settings set");
        _trafficSpawner.UpdateTrafficSettings(newSettings);
    }

    [ContextMenu("Start")]
    private void StartGameplay()
    {
        _ = _model.StartNewLevel();
    }

    [ContextMenu("End")]
    private void EndGameplay()
    {
        _model.EndLevel();
    }
}
