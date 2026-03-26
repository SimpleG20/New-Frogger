using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

using CustomLogger;
using Cysharp.Threading.Tasks;

using NewFrogger.Traffic.Domain;
using NewFrogger.Vehicle.Presentation;
using NewFrogger.Vehicle.Domain;

namespace NewFrogger.Traffic.Presentation
{
    public class TrafficController : MonoBehaviour
    {
        public event Action OnLevelTimerEnded;
        public event Action OnChangeTrafficSettings;

        [SerializeField] private TrafficSpawner _spawner;
        [SerializeField] private TrafficView _view;
        
        private TrafficModel _model;
        private readonly Dictionary<VehicleModel, VehicleView> _vehicleModelToView = new();

        public void Initialize(TrafficModel model)
        {
            _model = model;

            const int maxVehicles = 10;
            _spawner.Initialize(maxVehicles);
            _spawner.OnSpawn += HandleOnSpawn;
            _spawner.OnRelease += HandleOnRelease;

            _view.Initialize();

            _model.OnLevelTimerEnded += PublishOnLevelTimerEnded;
            _model.OnChangeTrafficSettings += PublishOnChangeTrafficSettings;
            _model.OnPredictionCountdownChanged += _view.UpdatePredictionCountdown;
        }

        private void PublishOnChangeTrafficSettings()
        {
            OnChangeTrafficSettings?.Invoke();
        }
        private void PublishOnLevelTimerEnded()
        {
            OnLevelTimerEnded?.Invoke();
        }

        #region Handle Events
        private void HandleOnSpawn(VehicleView vehicle)
        {
            vehicle.OnLimitReached += HandleOnLimitReached;
            var vehicleModel = vehicle.GetModel();
            if (_model.TryRegistryVehicle(vehicleModel))
            {
                _vehicleModelToView[vehicleModel] = vehicle;
                vehicleModel.ChangeSpeed(_model.CurrentTrafficSettings.AverageSpeed);
                vehicleModel.SetCanMove(!_model.Paused);
            }
        }
        private void HandleOnRelease(VehicleView vehicle)
        {
            vehicle.OnLimitReached -= HandleOnLimitReached;
            var vehicleModel = vehicle.GetModel();
            if (_model.TryUnregisterVehicle(vehicleModel))
            {
                _vehicleModelToView.Remove(vehicleModel);
            }
        }
        private void HandleOnLimitReached(VehicleView vehicle)
        {
            _spawner.Release(vehicle);
        }
        #endregion

        #region Gameplay
        public void StartGameplay(CancellationToken token)
        {
            _model.StartGameplay();
            _ = StartLevel(token);
        }
        public void PauseGameplay()
        {
            _model.PauseGameplay();
            _spawner.PauseSpawning();
        }
        public void ResumeGameplay()
        {
            _model.ResumeGameplay();
            _spawner.ResumeSpawning();
        }
        public void EndGameplay()
        {
            _view.ResetView();
            _spawner.StopSpawning();
            _model.EndGameplay();
            ReturnVehiclesToPool();
        }
        #endregion

        #region Level
        public bool HasNextLevel() => _model.CanNextLevel();
        public void IncreaseLevel() => _view.UpdateLevel(_model.IncreaseLevel());
        public async UniTask<TrafficSettings> ChangeTrafficSettings(CancellationToken token)
        {
            var newSettings = _model.SetNextTrafficSettings();
            if (newSettings.Equals(default))
            {
                return default;
            }

            _spawner.UpdateTrafficSettings(newSettings);
            if (_model.TryGetActiveVehicles(out var vehicles))
            {
                foreach (var v in vehicles) v.ChangeSpeed(newSettings.AverageSpeed);
            }
            _view.UpdateTrafficSettings(newSettings);

            await _view.TimedShowTrafficUpdateWarning(seconds: 2, token);
            
            return newSettings;
        }
        public async UniTask StartLevel(CancellationToken ct)
        {
            try
            {
                await _model.StartNewLevel(ct);
                _spawner.StartSpawning(_model.CurrentTrafficSettings);
                _view.StartLevel(_model.CurrentLevel, _model.PredictionTimer.TimeInSecs);
            }
            catch (Exception ex)
            {
                Log.log(ex);
                EndGameplay();
            }
        }
        public void EndLevel()
        {
            _spawner.StopSpawning();
            ReturnVehiclesToPool();
            _model.EndLevel();
            _view.EndLevel();
        }
        private void ReturnVehiclesToPool()
        {
            if (_model.TryGetActiveVehicles(out var activeVehicles))
            {
                foreach (var model in activeVehicles)
                {
                    if (_vehicleModelToView.TryGetValue(model, out var view))
                    {
                        _spawner.Release(view);
                    }
                }
            }
        }
        #endregion

        private void OnDestroy()
        {
            OnLevelTimerEnded = null;
            OnChangeTrafficSettings = null;

            _model?.Dispose();
            _spawner?.Dispose();
        }
    }
}