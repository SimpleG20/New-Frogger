using System;
using System.Collections.Generic;
using System.Threading;

using CustomLogger;
using Cysharp.Threading.Tasks;
using NewFrogger.Core.Data;
using NewFrogger.Gameplay.Domain;
using NewFrogger.Traffic.Domain.Services;
using NewFrogger.Vehicle.Domain;

namespace NewFrogger.Traffic.Domain
{
    public class TrafficModel : IDisposable
    {
        public event Action OnLevelTimerEnded;
        public event Action OnChangeTrafficSettings;
        public event Action<int> OnPredictionCountdownChanged;

        public bool Paused => _paused;
        public TimerInSecs PredictionTimer => _predictionTimer;

        public int CurrentLevel { get; private set; }
        public TrafficSettings CurrentTrafficSettings { get; private set; }

        private int _predictionIndex;
        private bool _paused;
        private LevelData _levelData;
        private TimerInSecs _levelTimer;
        private TimerInSecs _predictionTimer;
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
        public bool TryRegistryVehicle(VehicleModel vehicle)
        {
            if (vehicle == null) return false;
            if (_activeVehicles.Contains(vehicle))
            {
                Log.log("Trying to add a vehicle already in");
                return false;
            }
            vehicle.SetActive(true);
            _activeVehicles.Add(vehicle);
            return true;
        }
        public bool TryUnregisterVehicle(VehicleModel vehicle)
        {
            if (_activeVehicles.Contains(vehicle))
            {
                vehicle.SetActive(false);
                _activeVehicles.Remove(vehicle);
                return true;
            }
            return false;
        }
        #endregion

        #region Gameplay
        public void StartGameplay()
        {
            CurrentLevel = 1;
        }
        public void PauseGameplay()
        {
            _paused = true;
            _levelTimer?.Pause();
            _predictionTimer?.Pause();
            foreach (var vehicle in _activeVehicles)
            {
                vehicle.SetCanMove(false);
            }
        }
        public void ResumeGameplay()
        {
            _paused = false;
            _levelTimer?.Resume();
            _predictionTimer?.Resume();
            foreach(var vehicle in _activeVehicles)
            {
                vehicle.SetCanMove(true);
            }
        }
        public void EndGameplay()
        {
            ClearWaitersAndCancellations();
        }
        #endregion

        #region Level
        public async UniTask StartNewLevel(CancellationToken externalToken)
        {
            try
            {
                SetVariablesToStartNewLevel(externalToken);

                await FetchTrafficStatus();

                OnChangeTrafficSettings?.Invoke();

                StartLevelTimer();
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
            _predictionIndex = -1;
        }
        private async UniTask FetchTrafficStatus()
        {
            var status = await _trafficStatsService.call(new StatsArg(CurrentLevel, _gameCT));
            _levelData = new LevelData(_trafficLevelSettings.ReferenceSpeed, _trafficLevelSettings.zVehicleLimit, status.CurrentStatus, status.PredictedStatus);
        }
        private void StartLevelTimer()
        {
            int time = _levelData.MaxTime / 1000;

            _levelTimer?.Dispose();
            _levelTimer = new TimerInSecs(time, OnLevelTimerEnded);
            if (_paused) _levelTimer.Pause();
            _levelTimer.Start(_gameCT);
        }
        public TrafficSettings SetNextTrafficSettings()
        {
            if (!NextPrediction()) return default;

            _predictionIndex++;
            SetCurrentTrafficSettings(_predictionIndex);
            StartPrediction();
            return CurrentTrafficSettings;
        }
        private bool NextPrediction()
        {
            if (_levelData.Equals(default)) return false;
            return _predictionIndex + 1 < _levelData.TrafficSettings.Length;
        }
        private void SetCurrentTrafficSettings(int index)
        {
            CurrentTrafficSettings = _levelData.TrafficSettings[index].Settings;
        }
        private void StartPrediction()
        {
            try
            {
                var predictionToken = _predictionCTS.Token;
                var estimatedTime = _levelData.TrafficSettings[_predictionIndex + 1].EstimatedTime;
                
                _predictionTimer?.Dispose();
                _predictionTimer = new TimerInSecs(estimatedTime/1000, OnChangeTrafficSettings);
                _predictionTimer.OnCountdown += HandleOnPredictionCountdown;

                if (_paused) _predictionTimer.Pause();
                _predictionTimer.Start(predictionToken);
            }
            catch (OperationCanceledException) 
            {
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void EndLevel()
        {
            CurrentTrafficSettings = default;
            ClearWaitersAndCancellations();
        }
        #endregion

        #region Handle Events
        private void HandleOnPredictionCountdown(int time)
        {
            OnPredictionCountdownChanged?.Invoke(time);
        }
        #endregion

        private void ClearWaitersAndCancellations()
        {
            _levelTimer?.Dispose();
            _levelTimer = null;

            _predictionTimer?.Dispose();
            _predictionTimer = null;

            _predictionCTS?.Cancel();
            _predictionCTS?.Dispose();
            _predictionCTS = null;
        }
        public void Dispose()
        {
            OnLevelTimerEnded = null;
            OnChangeTrafficSettings = null;
            OnPredictionCountdownChanged = null;

            _activeVehicles?.Clear();
            _activeVehicles = null;

            ClearWaitersAndCancellations();
        }

    }
}
