using System;
using System.Collections.Generic;
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
        public event Action<int> OnCountdownChanged;
        public event Action<TrafficSettings> OnTrafficSettingsChanged;

        public Timer Timer => _timer;

        public int CurrentLevel { get; private set; }
        public TrafficSettings CurrentTrafficSettings { get; private set; }

        private int _predictionIndex;

        private Timer _timer;
        private LevelData _levelData;
        private List<VehicleModel> _activeVehicles;
        private readonly IGetTrafficStatsService _trafficStatsService;
        private readonly ITrafficLevelSettings _trafficLevelSettings;
        
        private CancellationTokenSource _predictionCTS;
        private CancellationToken _gameCT;

        public TrafficModel(ITrafficLevelSettings settings, IGetTrafficStatsService trafficService)
        {
            _trafficLevelSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            _trafficStatsService = trafficService ?? throw new ArgumentNullException(nameof(trafficService));

            _activeVehicles = new();
            CurrentLevel = 1;
        }

        public bool CanNextLevel() => CurrentLevel < _trafficLevelSettings.MaxLevels;
        public int IncreaseLevel() => CurrentLevel = CurrentLevel + 1;

        #region Vehicles
        public bool TryGetActiveVehicles(out List<VehicleModel> activeVehicles)
        {
            if (_activeVehicles != null && _activeVehicles.Count > 0)
            {
                activeVehicles = new List<VehicleModel>(_activeVehicles);
                return true;
            }

            activeVehicles = null;
            return false;
        }
        public void RegistryVehicle(VehicleModel vehicle)
        {
            if (vehicle == null) return;
            if (_activeVehicles.Contains(vehicle))
            {
                Log.log("Trying to add a vehicle already in");
                return;
            }
            vehicle.SetActive(true);
            _activeVehicles.Add(vehicle);
        }
        public void UnregisterVehicle(VehicleModel vehicle)
        {
            if (_activeVehicles.Contains(vehicle))
            {
                vehicle.SetActive(false);
                _activeVehicles.Remove(vehicle);
            }
        }
        #endregion

        #region Gameplay
        public void StartGameplay()
        {
            CurrentLevel = 1;
        }
        public void PauseGameplay()
        {
            foreach (var vehicle in _activeVehicles)
            {
                vehicle.SetCanMove(false);
            }
        }
        public void ResumeGameplay()
        {
            foreach(var vehicle in _activeVehicles)
            {
                vehicle.SetCanMove(true);
            }
        }

        #region Level
        public async UniTask StartNewLevel(CancellationToken externalToken)
        {
            try
            {
                SetVariablesToStartNewLevel(externalToken);

                await FetchTrafficStatus();

                SetCurrentTrafficSettings(_predictionIndex);

                StartTimer();
                var task = StartPrediction();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void SetVariablesToStartNewLevel(CancellationToken externalToken)
        {
            _predictionCTS = new();
            _gameCT = CancellationTokenSource.CreateLinkedTokenSource(externalToken, _predictionCTS.Token).Token;
            _predictionIndex = 0;
        }
        private async UniTask FetchTrafficStatus()
        {
            var status = await _trafficStatsService.call(new StatsArg(CurrentLevel, _gameCT));
            _levelData = new LevelData(_trafficLevelSettings.ReferenceSpeed, _trafficLevelSettings.zVehicleLimit, status.CurrentStatus, status.PredictedStatus);
        }
        private void StartTimer()
        {
            int time = _levelData.MaxTime / 1000;
            _timer = new Timer(time);
            _timer.OnCountdown += HandleOnCountdown;
            _ = _timer.StartTimer(_gameCT);
        }
        private void HandleOnCountdown(int time)
        {
            OnCountdownChanged?.Invoke(time);
        }

        private async UniTask StartPrediction()
        {
            try
            {
                var predictionToken = _predictionCTS.Token;

                while (!predictionToken.IsCancellationRequested && NextPrediction())
                {
                    var estimatedTime = _levelData.TrafficSettings[_predictionIndex].EstimatedTime;

                    await UniTask.Delay(TimeSpan.FromMilliseconds(estimatedTime), cancellationToken: predictionToken);

                    SetCurrentTrafficSettings(_predictionIndex);
                }
            }
            catch (OperationCanceledException) 
            {
            }
            catch (Exception)
            {
                throw;
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

            foreach (var vehicle in _activeVehicles)
            {
                vehicle.ChangeSpeed(CurrentTrafficSettings.AverageSpeed);
            }

            OnTrafficSettingsChanged?.Invoke(CurrentTrafficSettings);
        }

        public void EndLevel()
        {
            PartialClean();
        }

        private void PartialClean()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;

            _predictionCTS?.Cancel();
            _predictionCTS?.Dispose();
            _predictionCTS = null;

            CurrentTrafficSettings = default;
        }
        #endregion

        #endregion

        public void Dispose()
        {
            PartialClean();

            OnCountdownChanged = null;
            OnTrafficSettingsChanged = null;
        }
    }
}
