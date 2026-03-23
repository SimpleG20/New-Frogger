using System;
using System.Threading;
using CustomLogger;
using Cysharp.Threading.Tasks;
using NewFrogger.Gameplay.Data;
using NewFrogger.Traffic.Domain.Services;
using NewFrogger.Vehicle.Domain;

public class GameplayModel
{
    public event Action OnStartNewLevel;
    public event Action OnStopLevel;
    public event Action<TrafficSettings> OnTrafficSettingsChanged;

    public int CurrentLevel { get; private set; }
    public TrafficSettings CurrentTrafficSettings { get; private set; }

    private int _predictionIndex;
    private bool _isRunningLevel;
    private LevelData _levelData;
    private GetTrafficStatsService _trafficStatsService;
    private GameplaySettingsSO _gameplaySettingsSO;
    private CancellationTokenSource _predictionCTS;

    public GameplayModel(GameplaySettingsSO settingsSO, GetTrafficStatsService _trafficService)
    {
        _gameplaySettingsSO = settingsSO;
        _trafficStatsService = _trafficService;
    }

    public async UniTaskVoid StartNewLevel()
    {
        if (_isRunningLevel)
        {
            Log.log("Level is running, please end the level, before start a new one");
            return;
        }

        _isRunningLevel = true;
        _predictionIndex = 0;

        CurrentLevel++;

        var status = await _trafficStatsService.call();
        _levelData = new LevelData(_gameplaySettingsSO.ReferenceSpeed, _gameplaySettingsSO.ZLimit, status.CurrentStatus, status.PredictedStatus);

        OnStartNewLevel?.Invoke();
        SetCurrentTrafficSettings(_predictionIndex);

        _predictionCTS = new CancellationTokenSource();
        _ = StartPrediction();
    }

    private async UniTask StartPrediction()
    {
        try
        {
            while (_predictionCTS != null && !_predictionCTS.Token.IsCancellationRequested && NextPrediction())
            {
                var estimatedTime = _levelData.TrafficSettings[_predictionIndex].EstimatedTime;

                await UniTask.Delay(TimeSpan.FromMilliseconds(estimatedTime), cancellationToken: _predictionCTS.Token);

                SetCurrentTrafficSettings(_predictionIndex);
            }

            EndLevel();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Log.log(ex);
        }
    }

    private bool NextPrediction()
    {
        _predictionIndex++;
        return _predictionIndex < _levelData.TrafficSettings.Length;
    }

    private void SetCurrentTrafficSettings(int index)
    {
        CurrentTrafficSettings = _levelData.TrafficSettings[index].Settings;
        OnTrafficSettingsChanged?.Invoke(CurrentTrafficSettings);
    }

    public void EndLevel()
    {
        if (!_isRunningLevel)
        {
            Log.log("Trying to end the level but it is not even running");
            return;
        }

        CurrentTrafficSettings = default;

        _predictionCTS?.Cancel();
        _predictionCTS = null;

        _isRunningLevel = false;

        OnStopLevel?.Invoke();
    }
}
