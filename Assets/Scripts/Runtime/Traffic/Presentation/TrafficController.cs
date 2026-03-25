using System;
using System.Threading;
using System.Threading.Tasks;
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
        public event Action<TrafficSettings> OnTrafficSettingsChanged;

        [SerializeField] private TrafficSpawner _spawner;
        [SerializeField] private TrafficView _view;
        
        private TrafficModel _model;
        private readonly Dictionary<VehicleModel, VehicleView> _vehicleModelToView = new();

        public void Initialize(TrafficModel model)
        {
            _model = model;
            _model.OnTrafficSettingsChanged += PublishOnTrafficSettingsChanged;

            const int maxVehicles = 10;
            _spawner.Initialize(maxVehicles);
            _spawner.OnSpawn += HandleOnSpawn;
            _spawner.OnRelease += HandleOnRelease;

            _view.Initialize();
            _model.OnPredictionCountdownChanged += _view.UpdatePredictionCountdown;
            _model.OnLevelCountdownChanged += HandleOnLevelCountdown;
        }

        private void PublishOnTrafficSettingsChanged(TrafficSettings newSettings)
        {
            OnTrafficSettingsChanged?.Invoke(newSettings);
        }
        public async UniTask ShowChangeTrafficSettings(CancellationToken token)
        {
            var newSettings = _model.CurrentTrafficSettings;
            _spawner.UpdateTrafficSettings(newSettings);
            _view.UpdateTrafficSettings(newSettings);
            await _view.ShowTrafficUpdateWarning(token);
        }

        private void HandleOnSpawn(VehicleView vehicle)
        {
            vehicle.OnLimitReached += HandleOnLimitReached;
            var vehicleModel = vehicle.GetModel();
            if (_model.TryRegistryVehicle(vehicleModel))
            {
                _vehicleModelToView[vehicleModel] = vehicle;
                vehicleModel.SetCanMove(true);
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
        private void HandleOnLevelCountdown(int time)
        {
            if (time == 0) EndGameplay();
        }

        public void StartGameplay(CancellationToken token)
        {
            _model.StartGameplay();
            _ = StartLevel(token);
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
        public bool HasNextLevel() => _model.CanNextLevel();
        public void IncreaseLevel() => _view.UpdateLevel(_model.IncreaseLevel());
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
            _spawner.StopSpawning();
            _model.EndGameplay();
            ReturnVehiclesToPool();
        }

        private void OnDestroy()
        {
            OnTrafficSettingsChanged = null;

            _model?.Dispose();
            _spawner?.Dispose();
        }
    }
}