using System;
using System.Threading;

using CustomLogger;
using Cysharp.Threading.Tasks;

using NewFrogger.Gameplay.Domain;
using NewFrogger.Traffic.Domain.Services;
using NewFrogger.Vehicle.Domain;

namespace NewFrogger.Traffic.Domain
{
    public class TrafficModel : IDisposable
    {
        public event Action OnGameEnded;
        public event Action OnStopLevel;
        public event Action<int, int> OnStartNewLevel;
        public event Action<int> OnCountdownChanged;
        public event Action<TrafficSettings> OnTrafficSettingsChanged;

        public int CurrentLevel { get; private set; }
        public TrafficSettings CurrentTrafficSettings { get; private set; }

        private int _predictionIndex;
        private bool _isRunningLevel;

        private Timer _timer;
        private LevelData _levelData;
        private readonly IGetTrafficStatsService _trafficStatsService;
        private readonly ITrafficLevelSettings _trafficLevelSettings;
        
        private CancellationTokenSource _predictionCTS;
        private CancellationTokenSource _gameCTS;
        private CancellationToken _gameCT;

        public TrafficModel(ITrafficLevelSettings settings, IGetTrafficStatsService trafficService)
        {
            _trafficLevelSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            _trafficStatsService = trafficService ?? throw new ArgumentNullException(nameof(trafficService));
            
            _gameCTS = new CancellationTokenSource();
            
            _isRunningLevel = false;
        }

        public bool CanNextLevel()
        {
            return CurrentLevel < _trafficLevelSettings.MaxLevels;
        }

        public async UniTaskVoid StartNewLevel(CancellationToken externalToken = default)
        {
            if (_isRunningLevel)
            {
                Log.log("Level is running, please end the level, before start a new one");
                return;
            }
            _isRunningLevel = true;

            if (externalToken != default)
                _gameCT = CancellationTokenSource.CreateLinkedTokenSource(externalToken, _gameCTS.Token).Token;
            else
                _gameCT = _gameCTS.Token;

            try
            {
                _predictionIndex = 0;

                CurrentLevel++;

                var status = await _trafficStatsService.call(new StatsArg(CurrentLevel, _gameCT));
                _levelData = new LevelData(_trafficLevelSettings.ReferenceSpeed, _trafficLevelSettings.zVehicleLimit, status.CurrentStatus, status.PredictedStatus);

                int time = _levelData.MaxTime / 1000;
                _timer = new Timer(time);
                _timer.OnCountdown += HandleOnCountdown;

                OnStartNewLevel?.Invoke(CurrentLevel, time);
                SetCurrentTrafficSettings(_predictionIndex);

                _predictionCTS = CancellationTokenSource.CreateLinkedTokenSource(_gameCT);

                _ = _timer.StartTimer(_gameCT);
                var task = StartPrediction(_gameCT);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void HandleOnCountdown(int time)
        {
            OnCountdownChanged?.Invoke(time);
        }

        private async UniTask StartPrediction(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && NextPrediction())
                {
                    var estimatedTime = _levelData.TrafficSettings[_predictionIndex].EstimatedTime;

                    await UniTask.Delay(TimeSpan.FromMilliseconds(estimatedTime), cancellationToken: _predictionCTS.Token);

                    SetCurrentTrafficSettings(_predictionIndex);
                }

                OnGameEnded?.Invoke();
            }
            catch (OperationCanceledException) 
            {
                EndLevel();
            }
            catch (Exception ex)
            {
                Log.log(ex);
                EndLevel();
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

            PartialClean();

            OnStopLevel?.Invoke();
        }

        private void PartialClean()
        {
            _timer?.Stop();
            _timer.Dispose();
            _timer = null;

            CurrentTrafficSettings = default;

            _predictionCTS?.Cancel();
            _predictionCTS?.Dispose();
            _predictionCTS = null;

            _isRunningLevel = false;
        }

        public void Dispose()
        {
            PartialClean();

            _gameCTS?.Cancel();
            _gameCTS?.Dispose();
            _gameCTS = null;

            CurrentLevel = 0;
        }
    }

    public class Timer
    {
        public event Action<int> OnCountdown;
        public int TimeInSecs { get; private set; }
        public bool Running { get; private set; }

        private bool _paused;
        private Action _onEnd;

        public Timer(int time, Action onEnd = default) 
        {
            TimeInSecs = time; 
            _onEnd = onEnd;
        }

        public async UniTask StartTimer(CancellationToken ct)
        {
            if (Running) return;
            Running = true;

            try
            {
                while (Running && TimeInSecs > 0)
                {
                    if (_paused)
                    {
                        await UniTask.Yield();
                        continue;
                    }
                    await UniTask.Delay(1000, cancellationToken: ct);
                    TimeInSecs--;
                    OnCountdown?.Invoke(TimeInSecs);
                }

                _onEnd?.Invoke();
            }
            finally
            {
                Running = false;
            }
        }

        public void Pause() => _paused = true;
        public void Resume() => _paused = false;
        public void Stop() => Running = false;

        public void Dispose()
        {
            _onEnd = null;
            OnCountdown = null;
        }
    }
}
