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
    private readonly object _levelLock = new object();

    private LevelData _levelData;
    private GetTrafficStatsService _trafficStatsService;
    private GameplaySettingsSO _gameplaySettingsSO;
    private CancellationTokenSource _predictionCTS;
    private CancellationTokenSource _gameCTS;
    private CancellationToken _gameCT;

    public GameplayModel(GameplaySettingsSO settingsSO, GetTrafficStatsService trafficService)
    {
        _gameplaySettingsSO = settingsSO;
        _trafficStatsService = trafficService;
        _gameCTS = new CancellationTokenSource();
        _gameCT = _gameCTS.Token;
    }

    public async UniTaskVoid StartNewLevel()
    {
        lock (_levelLock)
        {
            if (_isRunningLevel)
            {
                Log.log("Level is running, please end the level, before start a new one");
                return;
            }
            _isRunningLevel = true;
        }

        try
        {
            _predictionIndex = 0;

            CurrentLevel++;

            var status = await _trafficStatsService.call(new StatsArg(CurrentLevel, _gameCT));
            _levelData = new LevelData(_gameplaySettingsSO.ReferenceSpeed, _gameplaySettingsSO.ZLimit, status.CurrentStatus, status.PredictedStatus);

            OnStartNewLevel?.Invoke();
            SetCurrentTrafficSettings(_predictionIndex);

            _predictionCTS = new CancellationTokenSource();
            _ = StartPrediction();
        }
        catch (Exception ex)
        {
            Log.log(ex);
        }
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
        if (_levelData.Equals(default)) return false;

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

    public void Dispose()
    {
        _predictionCTS?.Cancel();
        _predictionCTS?.Dispose();
        _predictionCTS = null;

        _gameCTS?.Cancel();
        _gameCTS.Dispose();
        _gameCTS = null;
    }
}
